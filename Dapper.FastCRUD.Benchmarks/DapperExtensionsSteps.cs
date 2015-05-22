namespace Dapper.FastCrud.Benchmarks
{
    using System.Linq;
    using Dapper.FastCrud.Tests;
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

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I select all the benchmark entities using Dapper Extensions")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;
            _testContext.QueriedEntities.AddRange(DapperExtensions.GetList<SimpleBenchmarkEntity>(dbConnection));
        }

        [When(@"I select all the benchmark entities that I previously inserted using Dapper Extensions")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                _testContext.QueriedEntities.Add(DapperExtensions.Get<SimpleBenchmarkEntity>(dbConnection, entity.Id));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Dapper Extensions")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var entityIndex = _testContext.InsertedEntities.Count;

            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityIndex++);
                newEntity.Id = entity.Id;
                DapperExtensions.Update(dbConnection, newEntity);
                _testContext.UpdatedEntities.Add(newEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Dapper Extensions")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                DapperExtensions.Delete(dbConnection, entity);
            }
        }

    }
}
