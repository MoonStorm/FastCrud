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
    using Newtonsoft.Json;
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

        [Then(@"I should have queried (\d+) (.+) entities")]
        public void ThenIShouldHaveQueriedEntities(int queryEntitiesCount, Type entityType)
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

        [Then(@"the queried (.+) entities should be the same as the last (\d+) inserted ones")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheLastInsertedOnes(Type entityType, int lastEntityCount)
        {
            var queriedEntities = _testContext.GetQueriedEntitiesOfType(entityType);
            var insertedEntities = _testContext.GetInsertedEntitiesOfType(entityType).TakeLast(lastEntityCount).ToArray();

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

        private ObjectMatchResult CompareCollections(IEnumerable<object> actualEntityCollection,
                                                     IEnumerable<object> expectedEntityCollection,
                                                     bool compareComplexTypedProperties,
                                                     bool enforceOrder,
                                                     bool assertOnMismatch,
                                                     int maxLevelToCheck = int.MaxValue,
                                                     int currentLevel = 1,
                                                     Dictionary<Tuple<object, object>, ObjectMatchResult?>? alreadyChecked = null)
        {
            var finalMatchResult = new ObjectMatchResult(expectedEntityCollection, actualEntityCollection);
            if (currentLevel > maxLevelToCheck)
            {
                return finalMatchResult;
            }

            // some of the tests that join to entities that don't exist will create null IEnumerables
            // we'll consider them identical to empty IEnumerables
            var actualCollectionLength = actualEntityCollection?.Count() ?? 0;
            var expectedCollectionLength = expectedEntityCollection?.Count() ?? 0;
            if (actualCollectionLength != expectedCollectionLength)
            {
                finalMatchResult.RegisterMismatch(ObjectMatchMismatchType.CollectionLength, currentLevel, "", expectedCollectionLength, actualEntityCollection, null);
                if (assertOnMismatch)
                {
                    Assert.Fail($"Expected a collection with {expectedCollectionLength} entities, but found {actualCollectionLength} entities.");
                }

                return finalMatchResult;
            }

            if (expectedCollectionLength == 0)
            {
                return finalMatchResult;
            }

            ObjectMatchResult? perhapsDecision = null;
            if (alreadyChecked?.TryGetValue(new Tuple<object, object>(actualEntityCollection!, expectedEntityCollection!), out perhapsDecision) == true)
            {
                // if we're already testing ourselves, and we're asked to do it again, pretend we've passed
                return perhapsDecision ?? finalMatchResult;
            }

            alreadyChecked ??= new Dictionary<Tuple<object, object>, ObjectMatchResult?>();
            alreadyChecked[new Tuple<object, object>(actualEntityCollection!, expectedEntityCollection!)] = null;

            var expectedEntityIndex = 0;
            foreach (var expectedEntity in expectedEntityCollection!)
            {
                var mismatchedExpectedEncounters = new List<ObjectMatchResult>();
                var actualEntityIndex = 0;
                var expectedEntityFound = false;
                foreach (var actualEntity in actualEntityCollection!)
                {
                    var expectedToActualEntityMatchResult = this.CompareObjects(
                        actualEntity,
                        expectedEntity,
                        compareComplexTypedProperties,
                        maxLevelToCheck,
                        currentLevel + 1,
                        alreadyChecked);
                    if (expectedToActualEntityMatchResult.IsMatch)
                    {
                        if (enforceOrder && (actualEntityIndex != expectedEntityIndex))
                        {
                            finalMatchResult.RegisterMismatch(ObjectMatchMismatchType.CollectionElementOrder, currentLevel, $"Expected index {expectedEntityIndex}, found at {actualEntityIndex}", expectedEntityIndex, actualEntityIndex);
                            if (assertOnMismatch)
                            {
                                Assert.Fail($"[Level: {currentLevel}] Expected entity to be located at index {expectedEntityIndex} but it was found in index {actualEntityIndex}");
                            }

                            break;
                        }
                        else
                        {
                            finalMatchResult.MatchingScore += 1;
                            expectedEntityFound = true;
                            break;
                        }
                    }
                    else
                    {
                        mismatchedExpectedEncounters.Add(expectedToActualEntityMatchResult);
                    }

                    actualEntityIndex++;
                }

                if (!expectedEntityFound)
                {
                    // mismatchedExpectedEncounters
                    finalMatchResult.RegisterMismatch(ObjectMatchMismatchType.CollectionElementNotFound,
                                                      currentLevel,
                                                      $"Index {expectedEntityIndex}",
                                                      expectedEntity,
                                                      null,
                                                      mismatchedExpectedEncounters.OrderByDescending(mismatch => mismatch.MatchingScore).ToArray());
                    if (assertOnMismatch)
                    {
                        Assert.Fail($"[Level: {currentLevel}] Could not locate the expect entity at the index {expectedEntityIndex} (Final match result: {finalMatchResult})");
                    }
                }

                expectedEntityIndex++;
            }

            alreadyChecked[new Tuple<object, object>(actualEntityCollection!, expectedEntityCollection!)] = finalMatchResult;
            return finalMatchResult;
        }

        private ObjectMatchResult CompareObjects(
            object actualEntity,
            object expectedEntity,
            bool compareComplexTypedProperties,
            int maxLevelToCheck = int.MaxValue,
            int currentLevel = 1,
            Dictionary<Tuple<object, object>, ObjectMatchResult?>? alreadyChecked = null)
        {
            ObjectMatchResult matchResult = new ObjectMatchResult(actualEntity, expectedEntity);
            if (currentLevel > maxLevelToCheck)
            {
                return matchResult;
            }

            if (ReferenceEquals(actualEntity, null) && ReferenceEquals(expectedEntity, null))
            {
                return matchResult;
            }

            if (ReferenceEquals(actualEntity, null) || ReferenceEquals(expectedEntity, null))
            {
                return matchResult;
            }

            var actualEntityType = actualEntity.GetType();
            var expectedEntityType = expectedEntity.GetType();

            if (!expectedEntityType.IsAssignableFrom(actualEntityType))
            {
                matchResult.RegisterMismatch(ObjectMatchMismatchType.Type, currentLevel, "", expectedEntityType, actualEntityType, null);
                return matchResult;
            }

            ObjectMatchResult? perhapsDecision = null;
            if (alreadyChecked?.TryGetValue(new Tuple<object, object>(actualEntity, expectedEntity), out perhapsDecision) == true)
            {
                // if we're already testing ourselves, and we're asked to do it again, pretend we've passed
                return perhapsDecision ?? matchResult;
            }

            alreadyChecked ??= new Dictionary<Tuple<object, object>, ObjectMatchResult?>();
            alreadyChecked[new Tuple<object, object>(actualEntity, expectedEntity)] = null;

            foreach (var propDescriptor in expectedEntityType.GetProperties())
            {
                var propType = propDescriptor.PropertyType;
                var actualEntityPropValue = propDescriptor.GetValue(actualEntity);
                var expectedEntityPropValue = propDescriptor.GetValue(expectedEntity);

                // for the time being, ignore the properties that are of complex type
                // normally they are foreign key entities
                if (propType.IsValueType || propType == typeof(string))
                {
                    bool decision = (Comparer.Default.Compare(actualEntityPropValue, expectedEntityPropValue) == 0);

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

                    if (decision)
                    {
                        matchResult.MatchingScore += 10;
                    }
                    else
                    {
                        matchResult.RegisterMismatch(ObjectMatchMismatchType.PropertyValue, currentLevel, propDescriptor.Name, expectedEntityPropValue, actualEntityPropValue, null);
                    }
                }
                else if (compareComplexTypedProperties && typeof(IEnumerable).IsAssignableFrom(propType))
                {
                    // two collections
                    var actualPropValueEnumeration = actualEntityPropValue as IEnumerable<object>;
                    var expectedPropValueEnumeration = expectedEntityPropValue as IEnumerable<object>;
                    var decision = CompareCollections(
                        actualPropValueEnumeration,
                        expectedPropValueEnumeration,
                        true,
                        false,
                        false,
                        maxLevelToCheck,
                        currentLevel + 1,
                        alreadyChecked);
                    if (decision.IsMatch)
                    {
                        matchResult.MatchingScore += 1;
                    }
                    else
                    {
                        matchResult.RegisterMismatch(ObjectMatchMismatchType.PropertyValue, currentLevel, propDescriptor.Name, expectedEntityPropValue, actualEntityPropValue, decision);
                    }
                }
                else if (compareComplexTypedProperties)
                {
                    // complex object
                    var decision = CompareObjects(
                        actualEntityPropValue,
                        expectedEntityPropValue,
                        true,
                        maxLevelToCheck,
                        currentLevel + 1,
                        alreadyChecked);
                    if (decision.IsMatch)
                    {
                        matchResult.MatchingScore += 1;
                    }
                    else
                    {
                        matchResult.RegisterMismatch(ObjectMatchMismatchType.PropertyValue, currentLevel, propDescriptor.Name, expectedEntityPropValue, actualEntityPropValue, decision);
                    }
                }
            }

            alreadyChecked[new Tuple<object, object>(actualEntity, expectedEntity)] = matchResult;
            return matchResult;
        }
    }
}