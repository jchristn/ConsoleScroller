namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;

    using global::Xunit;

    using Touchstone.Core;

    using Test.Shared;

    /// <summary>
    /// xUnit adapter over the shared Touchstone descriptors. Each non-skipped descriptor from
    /// <see cref="ScrollerTestSuites"/> becomes an individual xUnit theory row. Skip decisions are
    /// made by the shared library (see <see cref="ConsoleAvailability"/>), so console-dependent
    /// cases are omitted automatically when no interactive console is present.
    /// </summary>
    public sealed class ScrollerXunitTests
    {
        public static TheoryData<TestCaseDescriptor> Cases()
        {
            TheoryData<TestCaseDescriptor> data = new TheoryData<TestCaseDescriptor>();

            foreach (TestSuiteDescriptor suite in ScrollerTestSuites.All)
            {
                foreach (TestCaseDescriptor testCase in suite.Cases)
                {
                    if (!testCase.Skip) data.Add(testCase);
                }
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(Cases), DisableDiscoveryEnumeration = true)]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
