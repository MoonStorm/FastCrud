using Dapper.FastCrud.Tests.Contexts;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;

namespace Dapper.FastCrud.Benchmarks
{
    [Binding]
    public sealed class BenchmarkSteps
    {
        private readonly DatabaseTestContext _testContext;

        public BenchmarkSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I report the stopwatch value for (.*) finished processing (.*) operations of type (.*)")]
        public void WhenIReportTheStopwatchValueFor(string ormType, int opCount, string operation)
        {
            Trace.WriteLine($"Stopwatch reported: {_testContext.Stopwatch.Elapsed.TotalMilliseconds:0,0.00} milliseconds for {ormType}");

            // automatically update the docs
            var docsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../README.MD");
            var docsContents = File.ReadAllText(docsPath);

            var reportTitle = $"| {ormType} | {operation} | {opCount} |";
            var report = $"{reportTitle} {_testContext.Stopwatch.Elapsed.TotalMilliseconds:0,0.00} | {_testContext.Stopwatch.Elapsed.TotalMilliseconds * 1000 / opCount:0,0.00} |{Environment.NewLine}";

            var benchmarkHeaderRegex = new Regex($@"(?<=#+\s*?Automatic Benchmark Report)[^{Environment.NewLine}]*", RegexOptions.Singleline);
            var emptySpaceInsertRegex = new Regex($@"(?<=#+\s*?Automatic Benchmark Report(.*?{Environment.NewLine}){{3,3}})\s*?", RegexOptions.Singleline);
            var reportReplaceRegex = new Regex($@"{reportTitle.Replace("|", @"\|")}.*?{Environment.NewLine}", RegexOptions.Singleline);

            if (reportReplaceRegex.Match(docsContents).Success)
            {
                docsContents = reportReplaceRegex.Replace(docsContents, report, 1);
            }
            else
            {
                docsContents = emptySpaceInsertRegex.Replace(docsContents, report, 1);
            }

            docsContents = benchmarkHeaderRegex.Replace(docsContents, $" (Last Run: {DateTime.Now:D})", 1);

            File.WriteAllText(docsPath, docsContents);
        }

    }
}