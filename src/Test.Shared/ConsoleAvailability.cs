namespace Test.Shared
{
    using System;

    /// <summary>
    /// Detects whether an interactive console is available in the current host.
    /// <para>
    /// <see cref="ConsoleScroller.Scroller"/> depends on console cursor and buffer APIs
    /// (<see cref="Console.CursorTop"/>, <see cref="Console.BufferHeight"/>,
    /// <see cref="Console.WindowWidth"/> and <see cref="Console.SetCursorPosition(int, int)"/>).
    /// Those APIs throw <see cref="System.IO.IOException"/> ("The handle is invalid.") when the
    /// standard output handle is a redirected pipe or file, which is the case under
    /// <c>dotnet test</c> and any captured/headless execution.
    /// </para>
    /// <para>
    /// Test cases that must construct a live <see cref="ConsoleScroller.Scroller"/> are gated on
    /// <see cref="IsInteractive"/>: they execute and assert real behavior in a genuine terminal and
    /// are skipped (rather than failing spuriously) when no interactive console is present.
    /// </para>
    /// </summary>
    public static class ConsoleAvailability
    {
        private static readonly object _Lock = new object();
        private static bool? _Cached;

        /// <summary>
        /// True when the console cursor/buffer APIs the scroller relies on are usable.
        /// The result is probed once and cached for the lifetime of the process.
        /// </summary>
        public static bool IsInteractive
        {
            get
            {
                if (_Cached.HasValue) return _Cached.Value;

                lock (_Lock)
                {
                    if (!_Cached.HasValue) _Cached = Probe();
                    return _Cached.Value;
                }
            }
        }

        /// <summary>
        /// The reason surfaced to every runner (CLI, xUnit, NUnit) when a console-dependent
        /// test case is skipped because no interactive console is available.
        /// </summary>
        public static string SkipReason
        {
            get
            {
                return "Requires an interactive console; the console cursor/buffer APIs used by " +
                       "Scroller are unavailable in a redirected or headless host " +
                       "(Console.SetCursorPosition throws IOException when stdout is not a console).";
            }
        }

        private static bool Probe()
        {
            try
            {
                if (Console.IsOutputRedirected) return false;

                // Touch every member Scroller uses; any of these throws IOException when the
                // underlying stdout handle is not an interactive console screen buffer.
                int left = Console.CursorLeft;
                int top = Console.CursorTop;
                int bufferHeight = Console.BufferHeight;
                int windowWidth = Console.WindowWidth;

                // A no-op reposition proves SetCursorPosition is usable without moving anything.
                Console.SetCursorPosition(left, top);

                return bufferHeight > 0 && windowWidth > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
