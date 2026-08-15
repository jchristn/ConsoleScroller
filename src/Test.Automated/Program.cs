namespace Test.Automated
{
    using System.Threading.Tasks;

    using Touchstone.Cli;

    using Test.Shared;

    /// <summary>
    /// Touchstone CLI runner for the ConsoleScroller test suites. Executes every descriptor defined
    /// in <see cref="ScrollerTestSuites"/> and returns a process exit code of 0 when all tests pass
    /// (or skip) and 1 when any test fails.
    /// </summary>
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            string? resultsPath = args.Length > 0 ? args[0] : null;
            return await ConsoleRunner.RunAsync(ScrollerTestSuites.All, resultsPath: resultsPath);
        }
    }
}
