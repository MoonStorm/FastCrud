namespace Dapper.FastCrud.Benchmarks.Targets.FastCrud
{
    using global::Dapper.FastCrud.Benchmarks.Models;
    using global::Dapper.FastCrud.Tests.Contexts;
    using NUnit.Framework;
    using System.Linq;
    using TechTalk.SpecFlow;
    using FastCrud = global::Dapper.FastCrud.DapperExtensions;

    [Binding]
    public class FastCrudTests : EntityGenerationSteps
    {
        private readonly DatabaseTestContext _testContext;

        public FastCrudTests(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) benchmark entities using Fast Crud")]
        public void WhenIInsertSingleIntKeyEntitiesUsingFastCrud(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                FastCrud.Insert(dbConnection, generatedEntity);
                // the entity already has the associated id set

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.RecordInsertedEntity(generatedEntity);
            }
        }

        [Then(@"I should have (.*) benchmark entities in the database")]
        public void ThenIShouldHaveSingleIntKeyEntitiesInTheDatabase(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;
            var entities =  FastCrud.Find<SimpleBenchmarkEntity>(dbConnection);
            Assert.That(entities.Count(), Is.EqualTo(entitiesCount));
        }

        [When(@"I select all the benchmark entities using Fast Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var queriedEntity in FastCrud.Find<SimpleBenchmarkEntity>(dbConnection))
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I select all the benchmark entities that I previously inserted using Fast Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                _testContext.RecordQueriedEntity(FastCrud.Get(dbConnection, new SimpleBenchmarkEntity() {Id = entity.Id}));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Fast Crud")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;

            var entityIndex = 0;
            foreach (var oldEntity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityIndex++);
                newEntity.Id = oldEntity.Id;
                FastCrud.Update(dbConnection, newEntity);
                _testContext.RecordUpdatedEntity(newEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Fast Crud")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                FastCrud.Delete(dbConnection, entity);
            }
        }
    }
}
