namespace Dapper.FastCrud.Tests.Common
{
    using System;
    using System.Collections;
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

        [Then(@"the queried (.+) entities should be the same as the inserted ones")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheInsertedOnes(Type entityType)
        {
            var queriedEntities = _testContext.GetQueriedEntitiesOfType(entityType);
            var insertedEntities = _testContext.GetInsertedEntitiesOfType(entityType);

            this.CompareCollections(
                queriedEntities,
                insertedEntities,
                true,
                false,
                true);
        }

        [Then(@"the queried (.+) entities should be the same as the updated ones")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheUpdatedOnes(Type entityType)
        {
            var queriedEntities = _testContext.GetQueriedEntitiesOfType(entityType);
            var updatedEntities = _testContext.GetUpdatedEntitiesOfType(entityType);

            this.CompareCollections(
                queriedEntities, 
                updatedEntities, 
                true, 
                false,
                true);
        }

        [Then(@"the queried (.+) entities should be the same as the ones I inserted, in reverse order, starting from (.*) counting (.*)")]
        public void ThenTheQueriedWorkstationEntitiesShouldBeTheSameAsTheOnesIInsertedInReverseOrderStartingFromCounting(Type entityType, int? startIndex, int? maxCount)
        {
            var queriedEntities = _testContext.GetQueriedEntitiesOfType(entityType);
            var insertedEntities = _testContext.GetInsertedEntitiesOfType(entityType)
                                               .Reverse()
                                               .Skip(startIndex ?? 0)
                                               .Take(maxCount ?? int.MaxValue);
            this.CompareCollections(
                queriedEntities, 
                insertedEntities, 
                true, 
                true,
                true);
        }

        /// <summary>
        /// TODO: 
        /// </summary>
        private bool CompareCollections(IEnumerable<object> actualEntityCollection,
                                        IEnumerable<object> expectedEntityCollection,
                                        bool compareComplexTypedProperties,
                                        bool enforceOrder,
                                        bool assertOnMismatch,
                                        Dictionary<Tuple<object, object>, bool?>? alreadyChecked = null)
        {
            // some of the tests that join to entities that don't exist will create null IEnumerables
            // we'll consider them identical to empty IEnumerables
            var actualCollectionLength = actualEntityCollection?.Count() ?? 0;
            var expectedCollectionLength = expectedEntityCollection?.Count() ?? 0;
            if (actualCollectionLength != expectedCollectionLength)
            {
                if (assertOnMismatch)
                {
                    Assert.Fail($"Expected a collection with {expectedCollectionLength} entities, but found {actualCollectionLength} entities.");
                }

                return false;
            }
            if (expectedCollectionLength == 0)
            {
                return true;
            }

            bool? perhapsDecision = null;
            if (alreadyChecked?.TryGetValue(new Tuple<object, object>(actualEntityCollection!, expectedEntityCollection!), out perhapsDecision) == true)
            {
                // if we're already testing ourselves, and we're asked to do it again, pretend we've passed
                return perhapsDecision??true;
            }

            alreadyChecked ??= new Dictionary<Tuple<object, object>, bool?>();
            alreadyChecked[new Tuple<object, object>(actualEntityCollection!, expectedEntityCollection!)] = null;
            bool decision = true;
            try
            {

                var expectedEntityIndex = 0;
                foreach (var expectedEntity in expectedEntityCollection!)
                {
                    var matchFound = false;
                    var actualEntityIndex = 0;
                    foreach (var actualEntity in actualEntityCollection!)
                    {
                        if (this.CompareObjects(actualEntity, expectedEntity, compareComplexTypedProperties, alreadyChecked))
                        {
                            matchFound = true;
                            if (enforceOrder)
                            {
                                if (actualEntityIndex != expectedEntityIndex)
                                {
                                    decision = false;
                                    if (assertOnMismatch)
                                    {
                                        Assert.Fail($"Expected entity to be located at index {expectedEntityIndex} but it was found in index {actualEntityIndex}");
                                    }

                                    return decision;
                                }
                            }

                            break;
                        }

                        actualEntityIndex++;
                    }

                    if (!matchFound)
                    {
                        decision = false;
                        if (assertOnMismatch)
                        {
                            Assert.Fail($"The entity at index {expectedEntityIndex} could not be located");
                        }

                        return decision;
                    }

                    expectedEntityIndex++;
                }

                decision = true;
                return decision;
            }
            catch
            {
                decision = false;
                throw;
            }
            finally
            {
                alreadyChecked[new Tuple<object, object>(actualEntityCollection, expectedEntityCollection)] = decision;
            }
        }


        private bool CompareObjects(object actualEntity, object expectedEntity, bool compareComplexTypedProperties, Dictionary<Tuple<object, object>, bool?>? alreadyChecked = null)
        {
            if (ReferenceEquals(actualEntity, null) && ReferenceEquals(expectedEntity, null))
            {
                return true;
            }

            if (ReferenceEquals(actualEntity, null) || ReferenceEquals(expectedEntity, null))
            {
                return false;
            }

            var actualEntityType = actualEntity.GetType();
            var expectedEntityType = expectedEntity.GetType();

            if (!expectedEntityType.IsAssignableFrom(actualEntityType))
            {
                return false;
            }

            bool? perhapsDecision = null;
            if (alreadyChecked?.TryGetValue(new Tuple<object, object>(actualEntity, expectedEntity), out perhapsDecision) == true)
            {
                // if we're already testing ourselves, and we're asked to do it again, pretend we've passed
                return perhapsDecision ?? true;
            }

            alreadyChecked ??= new Dictionary<Tuple<object, object>, bool?>();
            alreadyChecked[new Tuple<object, object>(actualEntity, expectedEntity)] = null;
            bool decision = true;

            try
            {
                foreach (var propDescriptor in expectedEntityType.GetProperties())
                {
                    var propType = propDescriptor.PropertyType;
                    var actualEntityPropValue = propDescriptor.GetValue(actualEntity);
                    var expectedEntityPropValue = propDescriptor.GetValue(expectedEntity);

                    // for the time being, ignore the properties that are of complex type
                    // normally they are foreign key entities
                    if (propType.IsValueType || propType == typeof(string))
                    {
                        decision = (Comparer.Default.Compare(actualEntityPropValue, expectedEntityPropValue) == 0);

                        if (!decision)
                        {
                            // for dates, SQL Server only stores time to approximately 1/300th of a second or 3.33ms so we need to treat them differently
                            var dateComparisonsMaxAllowedTicks = TimeSpan.FromMilliseconds(2 * 3.33).Ticks;
                            if (propType == typeof(Nullable<DateTime>) || propType == typeof(DateTime))
                            {
                                if (Math.Abs(((DateTime)expectedEntityPropValue).Ticks - ((DateTime)actualEntityPropValue).Ticks) <= dateComparisonsMaxAllowedTicks)
                                {
                                    decision = true;
                                }
                            }

                            if (propType == typeof(Nullable<DateTimeOffset>) || propType == typeof(DateTimeOffset))
                            {
                                if (Math.Abs(((DateTimeOffset)expectedEntityPropValue).Ticks - ((DateTimeOffset)actualEntityPropValue).Ticks) <= dateComparisonsMaxAllowedTicks)
                                {
                                    decision = true;
                                }
                            }
                        }
                    }
                    else if (compareComplexTypedProperties && typeof(IEnumerable).IsAssignableFrom(propType))
                    {
                        // two collections
                        var actualPropValueEnumeration = actualEntityPropValue as IEnumerable<object>;
                        var expectedPropValueEnumeration = expectedEntityPropValue as IEnumerable<object>;
                        decision = CompareCollections(
                            actualPropValueEnumeration,
                            expectedPropValueEnumeration,
                            true,
                            false,
                            false,
                            alreadyChecked);
                    }
                    else if (compareComplexTypedProperties)
                    {
                        // complex object
                        decision = CompareObjects(actualEntityPropValue, expectedEntityPropValue, true, alreadyChecked);
                    }

                    if (!decision)
                    {
                        // no reason to continue
                        return false;
                    }
                }

                return decision;
            }
            catch
            {
                decision = false;
                throw;
            }
            finally
            {
                alreadyChecked[new Tuple<object, object>(actualEntity, expectedEntity)] = decision;
            }
        }
    }
}
