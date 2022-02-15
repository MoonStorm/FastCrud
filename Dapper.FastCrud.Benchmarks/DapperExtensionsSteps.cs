namespace Dapper.FastCrud.Benchmarks
{
    using Dapper.FastCrud.Benchmarks.Models;
    using System.Linq;
    using Dapper.FastCrud.Tests.Contexts;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;
    using DapperExtensions = global::DapperExtensions.DapperExtensions;

    [Binding]
    public class DapperExtensionsSteps : EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public DapperExtensionsSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [BeforeScenario()]
        public void SetupPluralTableMapping()
        {
            DapperExtensions.DefaultMapper = typeof(global::DapperExtensions.Mapper.PluralizedAutoClassMapper<>);
        }

        [When(@"I insert (.*) benchmark entities using Dapper Extensions")]
        public void WhenIInsertSingleIntKeyEntitiesUsingDapperExtensions(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                generatedEntity.Id = DapperExtensions.Insert(dbConnection, generatedEntity);

                Assert.That(generatedEntity.Id, Is.GreaterThan(1)); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.RecordInsertedEntity(generatedEntity);
            }
        }

        [When(@"I select all the benchmark entities using Dapper Extensions")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var queriedEntity in DapperExtensions.GetList<SimpleBenchmarkEntity>(dbConnection))
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I select all the benchmark entities that I previously inserted using Dapper Extensions")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                _testContext.RecordQueriedEntity(DapperExtensions.Get<SimpleBenchmarkEntity>(dbConnection, entity.Id));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Dapper Extensions")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;

            var entityIndex = 0;
            foreach (var oldInsertedEntity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityIndex++);
                newEntity.Id = oldInsertedEntity.Id;
                DapperExtensions.Update(dbConnection, newEntity);
                _testContext.RecordUpdatedEntity(newEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Dapper Extensions")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                DapperExtensions.Delete(dbConnection, entity);
            }
        }

    }
}
