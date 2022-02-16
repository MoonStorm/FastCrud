namespace Dapper.FastCrud.Benchmarks
{
    using Dapper.FastCrud.Tests.Common;
    using Dapper.FastCrud.Tests.Contexts;
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class BenchmarkSteps
    {
        private readonly DatabaseTestContext _testContext;
        private static readonly Regex _benchmarkHeaderRegex = new Regex($@"(?<=#+\s*?Automatic Benchmark Report)[^{Environment.NewLine}]*", RegexOptions.Singleline| RegexOptions.Compiled);
        private static readonly Regex _newEntryInsertRegex = new Regex($@"(?<=\|\s*\<a name=""new_entry_marker""\/\>\s*\|\s*){Environment.NewLine}", RegexOptions.Singleline|RegexOptions.Compiled);

        public BenchmarkSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [BeforeFeature]
        public static void BenchmarksSetup()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Assert.That(Process.GetCurrentProcess().PriorityClass, Is.EqualTo(ProcessPriorityClass.High));
        }

        [When(@"I report the stopwatch value for (.*) finished processing (.*) operations of type (.*)")]
        public void WhenIReportTheStopwatchValueFor(string ormType, int opCount, string operation)
        {
            var elapsedTime = _testContext.Stopwatch.Elapsed;
            Trace.WriteLine($"Stopwatch reported: {elapsedTime.TotalMilliseconds:0,0.00} milliseconds for {ormType}");

 #if !DEBUG
            // automatically update the docs
            var docsPath = Path.Combine(typeof(BenchmarkSteps).Assembly.GetDirectory(), "../../../../README.MD");
            var docsContents = File.ReadAllText(docsPath);

            var reportTitle = $"| {ormType} | {operation} | {opCount} |";
            var report = $"{reportTitle} {elapsedTime.TotalMilliseconds:0,0.00} | {elapsedTime.TotalMilliseconds * 1000 / opCount:0,0.00} |{Environment.NewLine}";

            var reportReplaceRegex = new Regex($@"{reportTitle.Replace("|", @"\|")}.*?{Environment.NewLine}", RegexOptions.Singleline);

            if (reportReplaceRegex.Match(docsContents).Success)
            {
                docsContents = reportReplaceRegex.Replace(docsContents, report, 1);
            }
            else
            {
                docsContents = _newEntryInsertRegex.Replace(docsContents, $@"{Environment.NewLine}{report}", 1);
            }

            docsContents = _benchmarkHeaderRegex.Replace(docsContents, $" (Last Run: {DateTime.Now:D})", 1);

            File.WriteAllText(docsPath, docsContents);
#endif
        }
    }
}