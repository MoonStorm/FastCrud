namespace Dapper.FastCrud.Benchmarks
{
    using System;
    using System.Linq;
    using Dapper.FastCrud.Tests;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;
    using FastCrud = global::Dapper.DapperExtensions;

    [Binding]
    public class FastCrudTests : EntityGenerationSteps
    {
        private readonly DatabaseTestContext _testContext;

        public FastCrudTests(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) benchmark entities using Fast Crud")]
        public void WhenIInsertSingleIntKeyEntitiesUsingSimpleCrud(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                FastCrud.Insert(dbConnection, generatedEntity);
                // the entity already has the associated id set

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [Then(@"I should have (.*) benchmark entities in the database")]
        public void ThenIShouldHaveSingleIntKeyEntitiesInTheDatabase(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;
            var entities =  FastCrud.Get<SimpleBenchmarkEntity>(dbConnection);
            Assert.AreEqual(entities.Count(), entitiesCount);
        }

        [When(@"I select all the benchmark entities using Fast Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            _testContext.QueriedEntities.AddRange(FastCrud.Get<SimpleBenchmarkEntity>(dbConnection));
        }

        [When(@"I select all the benchmark entities that I previously inserted using Fast Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                _testContext.QueriedEntities.Add(FastCrud.Get<SimpleBenchmarkEntity>(dbConnection, new SimpleBenchmarkEntity() {Id = entity.Id}));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Fast Crud")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var entityIndex = _testContext.InsertedEntities.Count;

            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityIndex++);
                newEntity.Id = entity.Id;
                FastCrud.Update(dbConnection, newEntity);
                _testContext.UpdatedEntities.Add(newEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Fast Crud")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                FastCrud.Delete(dbConnection, entity);
            }
        }
    }
}
