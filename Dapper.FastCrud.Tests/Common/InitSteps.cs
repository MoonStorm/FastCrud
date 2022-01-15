namespace Dapper.FastCrud.Tests.Common
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using BoDi;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using TechTalk.SpecFlow;

    [Binding]
    public class InitSteps
    {
        private readonly IObjectContainer _specflowContainer;
        private static readonly string CurrentExecutionFolder = typeof(InitSteps).Assembly.GetDirectory();

        public InitSteps(IObjectContainer specflowContainer)
        {
            _specflowContainer = specflowContainer;
        }

        [BeforeScenario(Order = 0)]
        public void InitializeIOC()
        {
            var consoleTraceListener = new TextWriterTraceListener(Console.Out);
            Trace.Listeners.Clear(); // the default one is in there
            Trace.Listeners.Add(consoleTraceListener);
            Trace.AutoFlush = true;

            var logFactory = LoggerFactory.Create(
                options =>
                {
                    options.ClearProviders();
                    options.AddConsole(
                        consoleOptions =>
                        {
                        });
                    options.SetMinimumLevel(LogLevel.Debug);
                });

            var configurationBuilder = new ConfigurationBuilder()
                .AddXmlFile(Path.Combine(CurrentExecutionFolder, $"App.Config"));
            var configuration = configurationBuilder.Build();

            _specflowContainer.RegisterInstanceAs<IConfiguration>(configuration);
        }
    }
}
