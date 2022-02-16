namespace Dapper.FastCrud.Benchmarks.SimpleCrud
{
    using Dapper.FastCrud.Benchmarks.Models;
    using Dapper.FastCrud.Tests.Contexts;
    using NUnit.Framework;
    using System.Collections;
    using System.Reflection;
    using TechTalk.SpecFlow;
    using SimpleCrud = global::Dapper.SimpleCRUD;

    [Binding]
    public class SimpleCrudSteps:EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public SimpleCrudSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [BeforeScenario]
        public static void TestSetup()
        {
            // clear caches
            var tableNamesPropInfo = typeof(SimpleCrud).GetField("TableNames", BindingFlags.Static | BindingFlags.NonPublic);
            var tableNamesInstance = tableNamesPropInfo.GetValue(null);
            ((IDictionary)tableNamesInstance).Clear();

            var columnNamesPropInfo = typeof(SimpleCrud).GetField("ColumnNames", BindingFlags.Static | BindingFlags.NonPublic);
            var columnNameInstance = columnNamesPropInfo.GetValue(null);
            ((IDictionary)columnNameInstance).Clear();
        }

        [When(@"I insert (.*) benchmark entities using Simple Crud")]
        public void WhenIInsertSingleIntKeyEntitiesUsingSimpleCrud(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                generatedEntity.Id = SimpleCrud.Insert(dbConnection, generatedEntity).Value;

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.RecordInsertedEntity(generatedEntity);
            }
        }

        [When(@"I select all the benchmark entities using Simple Crud (\d+) times")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingSimpleCrud(int opCount)
        {
            var dbConnection = _testContext.DatabaseConnection;
            while (--opCount >= 0)
            {
                foreach (var queriedEntity in SimpleCrud.GetList<SimpleBenchmarkEntity>(dbConnection))
                {
                    if (opCount == 0)
                    {
                        _testContext.RecordQueriedEntity(queriedEntity);
                    }
                }
            }
        }

        [When(@"I select all the benchmark entities that I previously inserted using Simple Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingSimpleCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                _testContext.RecordQueriedEntity(SimpleCrud.Get<SimpleBenchmarkEntity>(dbConnection, entity.Id));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Simple Crud")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingSimpleCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;

            var entityIndex = 0;
            foreach (var oldEntity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityIndex++);
                newEntity.Id = oldEntity.Id;
                SimpleCrud.Update(dbConnection, newEntity);
                _testContext.RecordUpdatedEntity(newEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Simple Crud")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingSimpleCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                SimpleCrud.Delete(dbConnection, entity);
            }
        }
    }
}
