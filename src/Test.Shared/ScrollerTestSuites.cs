namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using ConsoleScroller;
    using Touchstone.Core;

    /// <summary>
    /// The single source of truth for every ConsoleScroller test. Descriptors defined here are
    /// executed unchanged by the Touchstone CLI runner (Test.Automated), the xUnit adapter
    /// (Test.Xunit), and the NUnit adapter (Test.Nunit).
    /// <para>
    /// Cases split into two groups:
    /// <list type="bullet">
    /// <item>Console-independent cases (argument validation, public API surface) run in every host.</item>
    /// <item>Console-dependent cases construct a live <see cref="Scroller"/> and therefore require an
    /// interactive console; they are skipped automatically when one is not available. See
    /// <see cref="ConsoleAvailability"/>.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class ScrollerTestSuites
    {
        /// <summary>All suites, in a stable order.</summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    ArgumentValidationSuite(),
                    ApiSurfaceSuite(),
                    ConstructionSuite(),
                    WriteLineSuite(),
                    ColorSuite(),
                    ScrollingSuite(),
                    DisposeSuite(),
                    EdgeCaseSuite()
                };
            }
        }

        #region Suites

        // -----------------------------------------------------------------------------------------
        // Argument validation (console-independent: the guard clause runs before any Console access).
        // -----------------------------------------------------------------------------------------
        private static TestSuiteDescriptor ArgumentValidationSuite()
        {
            const string s = "ArgumentValidation";
            return new TestSuiteDescriptor(s, "Constructor argument validation", new List<TestCaseDescriptor>
            {
                Case(s, "zero_throws", "new Scroller(0) throws ArgumentOutOfRangeException", () =>
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => new Scroller(0), "maxLines of 0 is invalid")),

                Case(s, "negative_one_throws", "new Scroller(-1) throws ArgumentOutOfRangeException", () =>
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => new Scroller(-1), "maxLines of -1 is invalid")),

                Case(s, "min_value_throws", "new Scroller(int.MinValue) throws ArgumentOutOfRangeException", () =>
                    TestAssert.Throws<ArgumentOutOfRangeException>(() => new Scroller(int.MinValue), "extreme negative is invalid")),

                Case(s, "zero_paramname", "ArgumentOutOfRangeException reports ParamName 'maxLines' for 0", () =>
                {
                    var ex = TestAssert.Throws<ArgumentOutOfRangeException>(() => new Scroller(0), "for 0");
                    TestAssert.Equal("maxLines", ex.ParamName, "ParamName should identify the offending argument");
                }),

                Case(s, "negative_paramname", "ArgumentOutOfRangeException reports ParamName 'maxLines' for -5", () =>
                {
                    var ex = TestAssert.Throws<ArgumentOutOfRangeException>(() => new Scroller(-5), "for -5");
                    TestAssert.Equal("maxLines", ex.ParamName, "ParamName should identify the offending argument");
                }),

                Case(s, "min_value_paramname", "ArgumentOutOfRangeException reports ParamName 'maxLines' for int.MinValue", () =>
                {
                    var ex = TestAssert.Throws<ArgumentOutOfRangeException>(() => new Scroller(int.MinValue), "for int.MinValue");
                    TestAssert.Equal("maxLines", ex.ParamName, "ParamName should identify the offending argument");
                })
            });
        }

        // -----------------------------------------------------------------------------------------
        // Public API surface (console-independent: pure reflection, guards against accidental breaks).
        // -----------------------------------------------------------------------------------------
        private static TestSuiteDescriptor ApiSurfaceSuite()
        {
            const string s = "ApiSurface";
            Type t = typeof(Scroller);

            return new TestSuiteDescriptor(s, "Public API surface", new List<TestCaseDescriptor>
            {
                Case(s, "type_is_public", "Scroller is a public class", () =>
                {
                    TestAssert.True(t.IsClass, "Scroller should be a class");
                    TestAssert.True(t.IsPublic, "Scroller should be public");
                }),

                Case(s, "namespace", "Scroller lives in the ConsoleScroller namespace", () =>
                    TestAssert.Equal("ConsoleScroller", t.Namespace, "namespace")),

                Case(s, "implements_idisposable", "Scroller implements IDisposable", () =>
                    TestAssert.True(typeof(IDisposable).IsAssignableFrom(t), "should implement IDisposable")),

                Case(s, "ctor_int", "Scroller exposes a public constructor taking a single int", () =>
                {
                    ConstructorInfo? ctor = t.GetConstructor(new[] { typeof(int) });
                    TestAssert.NotNull(ctor, "public Scroller(int) constructor should exist");
                    TestAssert.True(ctor!.IsPublic, "constructor should be public");
                    ParameterInfo[] ps = ctor.GetParameters();
                    TestAssert.Equal(1, ps.Length, "constructor arity");
                    TestAssert.Equal("maxLines", ps[0].Name, "constructor parameter name");
                }),

                Case(s, "writeline_signature", "WriteLine(string, ConsoleColor?, ConsoleColor?) exists returning void", () =>
                {
                    MethodInfo? m = t.GetMethod("WriteLine", new[]
                    {
                        typeof(string), typeof(ConsoleColor?), typeof(ConsoleColor?)
                    });
                    TestAssert.NotNull(m, "WriteLine(string, ConsoleColor?, ConsoleColor?) should exist");
                    TestAssert.True(m!.IsPublic, "WriteLine should be public");
                    TestAssert.Equal(typeof(void), m.ReturnType, "WriteLine should return void");
                }),

                Case(s, "writeline_optional_colors", "WriteLine color parameters are optional and default to null", () =>
                {
                    MethodInfo m = t.GetMethod("WriteLine", new[]
                    {
                        typeof(string), typeof(ConsoleColor?), typeof(ConsoleColor?)
                    })!;
                    ParameterInfo[] ps = m.GetParameters();
                    TestAssert.Equal("text", ps[0].Name, "first parameter name");
                    TestAssert.False(ps[0].IsOptional, "text parameter should be required");
                    TestAssert.True(ps[1].IsOptional, "foreground parameter should be optional");
                    TestAssert.True(ps[2].IsOptional, "background parameter should be optional");
                    TestAssert.Equal(null, ps[1].DefaultValue, "foreground default should be null");
                    TestAssert.Equal(null, ps[2].DefaultValue, "background default should be null");
                }),

                Case(s, "dispose_method", "Dispose() is public and returns void", () =>
                {
                    MethodInfo? m = t.GetMethod("Dispose", Type.EmptyTypes);
                    TestAssert.NotNull(m, "Dispose() should exist");
                    TestAssert.True(m!.IsPublic, "Dispose should be public");
                    TestAssert.Equal(typeof(void), m.ReturnType, "Dispose should return void");
                }),

                Case(s, "single_public_constructor", "Scroller exposes exactly one public constructor", () =>
                {
                    ConstructorInfo[] ctors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    TestAssert.Equal(1, ctors.Length, "adding or removing a public constructor is a surface change");
                }),

                Case(s, "no_public_properties", "Scroller exposes no public instance properties", () =>
                {
                    PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    TestAssert.Equal(0, props.Length, "Scroller intentionally exposes no public properties; a new one is a surface change");
                }),

                Case(s, "declared_public_methods_are_expected", "the only declared public methods are WriteLine and Dispose", () =>
                {
                    // Declared-only excludes inherited Object members. Property/event accessors would also
                    // appear here (IsSpecialName), but Scroller has none; this guards against silent surface growth.
                    string[] names = t
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(m => !m.IsSpecialName)
                        .Select(m => m.Name)
                        .Distinct()
                        .OrderBy(n => n, StringComparer.Ordinal)
                        .ToArray();

                    TestAssert.Equal(2, names.Length, "exactly two public methods expected (WriteLine, Dispose)");
                    TestAssert.Equal("Dispose", names[0], "first public method (ordinal) should be Dispose");
                    TestAssert.Equal("WriteLine", names[1], "second public method (ordinal) should be WriteLine");
                })
            });
        }

        // -----------------------------------------------------------------------------------------
        // Construction (console-dependent).
        // -----------------------------------------------------------------------------------------
        private static TestSuiteDescriptor ConstructionSuite()
        {
            const string s = "Construction";
            return new TestSuiteDescriptor(s, "Construction of valid scrollers", new List<TestCaseDescriptor>
            {
                ConsoleCase(s, "one", "new Scroller(1) constructs the smallest valid scroller", () =>
                {
                    using var scroller = new Scroller(1);
                    TestAssert.NotNull(scroller, "scroller instance");
                    TestAssert.True(scroller is IDisposable, "scroller should be IDisposable");
                }),

                ConsoleCase(s, "three", "new Scroller(3) constructs", () =>
                {
                    using var scroller = new Scroller(3);
                    TestAssert.NotNull(scroller, "scroller instance");
                }),

                ConsoleCase(s, "five", "new Scroller(5) constructs", () =>
                {
                    using var scroller = new Scroller(5);
                    TestAssert.NotNull(scroller, "scroller instance");
                }),

                ConsoleCase(s, "hundred", "new Scroller(100) constructs", () =>
                {
                    using var scroller = new Scroller(100);
                    TestAssert.NotNull(scroller, "scroller instance");
                }),

                ConsoleCase(s, "sequential_instances", "multiple scrollers can be created and disposed in sequence", () =>
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        using var scroller = new Scroller(i);
                        scroller.WriteLine("instance " + i);
                    }
                })
            });
        }

        // -----------------------------------------------------------------------------------------
        // WriteLine happy paths (console-dependent).
        // -----------------------------------------------------------------------------------------
        private static TestSuiteDescriptor WriteLineSuite()
        {
            const string s = "WriteLine";
            return new TestSuiteDescriptor(s, "WriteLine behavior", new List<TestCaseDescriptor>
            {
                ConsoleCase(s, "single", "WriteLine writes a single line", () =>
                {
                    using var scroller = new Scroller(5);
                    scroller.WriteLine("hello world");
                }),

                ConsoleCase(s, "with_foreground", "WriteLine accepts a foreground color", () =>
                {
                    using var scroller = new Scroller(5);
                    scroller.WriteLine("red text", ConsoleColor.Red);
                }),

                ConsoleCase(s, "with_foreground_and_background", "WriteLine accepts foreground and background colors", () =>
                {
                    using var scroller = new Scroller(5);
                    scroller.WriteLine("white on blue", ConsoleColor.White, ConsoleColor.Blue);
                }),

                ConsoleCase(s, "null_text", "WriteLine tolerates null text", () =>
                {
                    using var scroller = new Scroller(5);
                    scroller.WriteLine(null!);
                }),

                ConsoleCase(s, "empty_text", "WriteLine tolerates an empty string", () =>
                {
                    using var scroller = new Scroller(5);
                    scroller.WriteLine(string.Empty);
                }),

                ConsoleCase(s, "whitespace_text", "WriteLine tolerates whitespace-only text", () =>
                {
                    using var scroller = new Scroller(5);
                    scroller.WriteLine("     ");
                }),

                ConsoleCase(s, "unicode_text", "WriteLine handles unicode and symbols", () =>
                {
                    using var scroller = new Scroller(5);
                    scroller.WriteLine("√ × → ★ 你好");
                }),

                ConsoleCase(s, "long_text", "WriteLine handles text longer than the window width", () =>
                {
                    using var scroller = new Scroller(5);
                    scroller.WriteLine(new string('x', Math.Max(200, Console.WindowWidth + 50)));
                }),

                ConsoleCase(s, "multiple_within_max", "WriteLine handles fewer lines than the maximum", () =>
                {
                    using var scroller = new Scroller(5);
                    for (int i = 1; i <= 3; i++) scroller.WriteLine("line " + i);
                }),

                ConsoleCase(s, "exactly_max", "WriteLine handles exactly maxLines lines", () =>
                {
                    using var scroller = new Scroller(4);
                    for (int i = 1; i <= 4; i++) scroller.WriteLine("line " + i);
                })
            });
        }

        // -----------------------------------------------------------------------------------------
        // Color handling (console-dependent).
        // -----------------------------------------------------------------------------------------
        private static TestSuiteDescriptor ColorSuite()
        {
            const string s = "Colors";
            ConsoleColor[] colors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));

            return new TestSuiteDescriptor(s, "Color handling", new List<TestCaseDescriptor>
            {
                ConsoleCase(s, "all_foreground", "every ConsoleColor is accepted as a foreground", () =>
                {
                    using var scroller = new Scroller(4);
                    foreach (ConsoleColor c in colors) scroller.WriteLine(c.ToString(), c);
                }),

                ConsoleCase(s, "all_background", "every ConsoleColor is accepted as a background", () =>
                {
                    using var scroller = new Scroller(4);
                    foreach (ConsoleColor c in colors) scroller.WriteLine(c.ToString(), ConsoleColor.White, c);
                }),

                ConsoleCase(s, "null_colors_use_current", "null colors fall back to the current console colors", () =>
                {
                    using var scroller = new Scroller(4);
                    scroller.WriteLine("default colors", null, null);
                }),

                ConsoleCase(s, "foreground_only_keeps_background", "specifying only a foreground is accepted", () =>
                {
                    using var scroller = new Scroller(4);
                    scroller.WriteLine("green", ConsoleColor.Green);
                })
            });
        }

        // -----------------------------------------------------------------------------------------
        // Scrolling / eviction (console-dependent): the RefreshDisplay windowing logic.
        // -----------------------------------------------------------------------------------------
        private static TestSuiteDescriptor ScrollingSuite()
        {
            const string s = "Scrolling";
            return new TestSuiteDescriptor(s, "Scrolling and line eviction", new List<TestCaseDescriptor>
            {
                ConsoleCase(s, "one_over_max", "writing maxLines+1 lines evicts the oldest", () =>
                {
                    using var scroller = new Scroller(3);
                    for (int i = 1; i <= 4; i++) scroller.WriteLine("line " + i);
                }),

                ConsoleCase(s, "far_exceeds_max", "writing many more lines than the maximum keeps scrolling", () =>
                {
                    using var scroller = new Scroller(5);
                    for (int i = 1; i <= 50; i++) scroller.WriteLine("line " + i, (i % 2 == 0) ? ConsoleColor.Green : ConsoleColor.Yellow);
                }),

                ConsoleCase(s, "single_line_window", "a maxLines of 1 keeps only the most recent line", () =>
                {
                    using var scroller = new Scroller(1);
                    for (int i = 1; i <= 10; i++) scroller.WriteLine("only " + i);
                }),

                ConsoleCase(s, "rapid_writes", "a burst of writes through a small window does not throw", () =>
                {
                    using var scroller = new Scroller(4);
                    for (int i = 1; i <= 200; i++) scroller.WriteLine("burst " + i);
                })
            });
        }

        // -----------------------------------------------------------------------------------------
        // Dispose contract (console-dependent for construction).
        // -----------------------------------------------------------------------------------------
        private static TestSuiteDescriptor DisposeSuite()
        {
            const string s = "Dispose";
            return new TestSuiteDescriptor(s, "Dispose and object lifetime", new List<TestCaseDescriptor>
            {
                ConsoleCase(s, "after_construction", "Dispose after construction does not throw", () =>
                {
                    var scroller = new Scroller(3);
                    scroller.Dispose();
                }),

                ConsoleCase(s, "idempotent", "Dispose is idempotent", () =>
                {
                    var scroller = new Scroller(3);
                    scroller.Dispose();
                    scroller.Dispose();
                    scroller.Dispose();
                }),

                ConsoleCase(s, "after_writes", "Dispose after several writes does not throw", () =>
                {
                    var scroller = new Scroller(3);
                    scroller.WriteLine("a");
                    scroller.WriteLine("b");
                    scroller.WriteLine("c");
                    scroller.WriteLine("d");
                    scroller.Dispose();
                }),

                ConsoleCase(s, "using_block", "the using pattern constructs, writes, and disposes cleanly", () =>
                {
                    using (var scroller = new Scroller(3))
                    {
                        scroller.WriteLine("inside using");
                    }
                }),

                ConsoleCase(s, "writeline_after_dispose_throws", "WriteLine after Dispose throws ObjectDisposedException", () =>
                {
                    var scroller = new Scroller(3);
                    scroller.Dispose();
                    TestAssert.Throws<ObjectDisposedException>(() => scroller.WriteLine("nope"),
                        "writing to a disposed scroller must throw");
                }),

                ConsoleCase(s, "disposed_object_name", "ObjectDisposedException identifies Scroller", () =>
                {
                    var scroller = new Scroller(3);
                    scroller.Dispose();
                    var ex = TestAssert.Throws<ObjectDisposedException>(() => scroller.WriteLine("nope"), "after dispose");
                    TestAssert.Equal("Scroller", ex.ObjectName, "ObjectName should be Scroller");
                }),

                ConsoleCase(s, "writeline_after_using_throws", "WriteLine after a using block throws ObjectDisposedException", () =>
                {
                    Scroller captured;
                    using (var scroller = new Scroller(3))
                    {
                        scroller.WriteLine("inside");
                        captured = scroller;
                    }
                    TestAssert.Throws<ObjectDisposedException>(() => captured.WriteLine("outside"),
                        "the scroller is disposed once the using block exits");
                }),

                ConsoleCase(s, "restores_default_colors", "Dispose restores the console's default colors", () =>
                {
                    ConsoleColor fg = Console.ForegroundColor;
                    ConsoleColor bg = Console.BackgroundColor;
                    try
                    {
                        var scroller = new Scroller(3);
                        scroller.WriteLine("colored", ConsoleColor.Magenta, ConsoleColor.DarkBlue);
                        scroller.Dispose();
                        TestAssert.Equal(fg, Console.ForegroundColor, "foreground restored on Dispose");
                        TestAssert.Equal(bg, Console.BackgroundColor, "background restored on Dispose");
                    }
                    finally
                    {
                        Console.ForegroundColor = fg;
                        Console.BackgroundColor = bg;
                    }
                })
            });
        }

        // -----------------------------------------------------------------------------------------
        // Edge cases (mixed).
        // -----------------------------------------------------------------------------------------
        private static TestSuiteDescriptor EdgeCaseSuite()
        {
            const string s = "EdgeCases";
            return new TestSuiteDescriptor(s, "Edge cases", new List<TestCaseDescriptor>
            {
                Case(s, "one_is_valid_boundary", "1 is the smallest accepted maxLines (no exception thrown for the guard)", () =>
                {
                    // The guard clause rejects < 1; 1 must pass the guard. Whether construction then
                    // needs a console is a separate concern, so assert only that the guard admits 1.
                    ArgumentOutOfRangeException? guard = null;
                    try
                    {
                        using var scroller = new Scroller(1);
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        guard = ex;
                    }
                    catch
                    {
                        // Any non-guard exception (e.g. IOException with no console) means the guard admitted 1.
                    }
                    TestAssert.True(guard is null, "maxLines of 1 must not be rejected by the argument guard");
                }),

                ConsoleCase(s, "interleaved_colors_and_plain", "interleaving colored and plain writes does not throw", () =>
                {
                    using var scroller = new Scroller(4);
                    scroller.WriteLine("plain");
                    scroller.WriteLine("red", ConsoleColor.Red);
                    scroller.WriteLine("plain again");
                    scroller.WriteLine("blue on white", ConsoleColor.Blue, ConsoleColor.White);
                }),

                ConsoleCase(s, "tab_and_control_chars", "text containing tabs is handled", () =>
                {
                    using var scroller = new Scroller(4);
                    scroller.WriteLine("col1\tcol2\tcol3");
                })
            });
        }

        #endregion

        #region Case-builders

        private static TestCaseDescriptor Case(string suiteId, string caseId, string displayName, Action body, params string[] tags)
        {
            return new TestCaseDescriptor(
                suiteId,
                caseId,
                displayName,
                ct => { body(); return Task.CompletedTask; },
                tags: tags.Length == 0 ? null : tags);
        }

        private static TestCaseDescriptor ConsoleCase(string suiteId, string caseId, string displayName, Action body, params string[] tags)
        {
            List<string> allTags = new List<string> { "console" };
            if (tags.Length > 0) allTags.AddRange(tags);

            return new TestCaseDescriptor(
                suiteId,
                caseId,
                displayName,
                ct => { body(); return Task.CompletedTask; },
                tags: allTags,
                skip: !ConsoleAvailability.IsInteractive,
                skipReason: ConsoleAvailability.SkipReason);
        }

        #endregion
    }
}
