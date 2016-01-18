namespace Dapper.FastCrud.Benchmarks
{
    using System.Linq;
    using Dapper.FastCrud.Tests;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
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
                _testContext.LocalEntities.Add(generatedEntity);
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
            _testContext.QueriedEntities.AddRange(FastCrud.Find<SimpleBenchmarkEntity>(dbConnection));
        }

        [When(@"I select all the benchmark entities that I previously inserted using Fast Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var entity in _testContext.LocalEntities.OfType<SimpleBenchmarkEntity>())
            {
                _testContext.QueriedEntities.Add(FastCrud.Get<SimpleBenchmarkEntity>(dbConnection, new SimpleBenchmarkEntity() {Id = entity.Id}));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Fast Crud")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var entityCount = _testContext.LocalEntities.Count;

            for (var entityIndex = 0; entityIndex < _testContext.LocalEntities.Count; entityIndex++)
            {
                var oldEntity = _testContext.LocalEntities[entityIndex] as SimpleBenchmarkEntity;
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityCount++);
                newEntity.Id = oldEntity.Id;
                FastCrud.Update(dbConnection, newEntity);
                _testContext.LocalEntities[entityIndex] = newEntity;
            }
        }

        [When(@"I delete all the inserted benchmark entities using Fast Crud")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.LocalEntities.OfType<SimpleBenchmarkEntity>())
            {
                FastCrud.Delete(dbConnection, entity);
            }
        }
    }
}
