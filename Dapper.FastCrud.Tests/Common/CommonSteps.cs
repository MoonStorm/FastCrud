namespace Dapper.FastCrud.Tests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using BoDi;
    using Dapper.FastCrud.Tests.Contexts;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CommonSteps
    {
        private readonly IObjectContainer _specflowContainer;
        private readonly DatabaseTestContext _testContext;

        public CommonSteps(IObjectContainer specflowContainer, DatabaseTestContext testContext)
        {
            _specflowContainer = specflowContainer;
            _testContext = testContext;
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
                .AddXmlFile(Path.Combine(_testContext.CurrentExecutionFolder, $"App.Config"));
            var configuration = configurationBuilder.Build();

            _specflowContainer.RegisterInstanceAs<IConfiguration>(configuration);
        }

        [AfterScenario]
        public void Cleanup()
        {
            _testContext.DatabaseConnection?.Close();
            _testContext.DatabaseConnection?.Dispose();
            OrmConfiguration.ClearEntityRegistrations();
        }

        [When(@"I refresh the database connection")]
        public void WhenIRefreshTheDatabaseConnection()
        {
            _testContext.DatabaseConnection.Close();
            _testContext.DatabaseConnection.Open();
        }

        [Given(@"I started the stopwatch")]
        [When(@"I start the stopwatch")]
        public void GivenIStartedTheStopwatch()
        {
            _testContext.Stopwatch.Restart();
        }

        [When(@"I stop the stopwatch")]
        public void WhenIStopTheStopwatch()
        {
            _testContext.Stopwatch.Stop();
        }

        [Then(@"I should have queried (\d+) (.+) ")]
        public void ThenIShouldHaveQueriedEntities(Type entityType, int queryEntitiesCount)
        {
            var queriedEntities = _testContext.GetQueriedEntitiesOfType(entityType);
            Assert.That(queriedEntities.Length, Is.EqualTo(queryEntitiesCount));
        }

        [Then(@"the queried (.+) entities should be the same as the updated ones")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheUpdatedOnes(Type entityType)
        {
            var queriedEntities = _testContext.GetQueriedEntitiesOfType(entityType);
            var updatedEntities = _testContext.GetUpdatedEntitiesOfType(entityType);

            this.AssertEquivalentEntityCollections(queriedEntities, updatedEntities);
        }

        private void AssertEquivalentEntityCollections(IEnumerable<object> firstEntityCollection, IEnumerable<object> secondEntityCollection)
        {
            Assert.AreEqual(firstEntityCollection.Count(), secondEntityCollection.Count());

            foreach (var firstEntity in firstEntityCollection)
            {
                var matchFound = false;
                foreach (var secondEntity in secondEntityCollection)
                {
                    if (DbEntityComparer.Instance.Compare(firstEntity, secondEntity) == 0)
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    Assert.Fail($"Two collections were found not to be equivalent. The check failed for the entity '{firstEntity}'");
                }
            }
        }

    }
}
