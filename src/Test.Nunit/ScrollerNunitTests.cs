namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;

    using global::NUnit.Framework;

    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    using Test.Shared;

    /// <summary>
    /// NUnit adapter over the shared Touchstone descriptors. Each non-skipped descriptor from
    /// <see cref="ScrollerTestSuites"/> becomes an individual NUnit test case. Skip decisions are
    /// made by the shared library (see <see cref="ConsoleAvailability"/>), so console-dependent
    /// cases are omitted automatically when no interactive console is present.
    /// </summary>
    [TestFixture]
    public sealed class ScrollerNunitTests
    {
        private static IEnumerable Cases()
        {
            return new TouchstoneTestCaseSource(ScrollerTestSuites.All);
        }

        [Test]
        [TestCaseSource(nameof(Cases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
