namespace Dapper.FastCrud.Tests.Features
{
    using System;
    using System.Linq;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class FastCrudTests : EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public FastCrudTests(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) single int key entities using Fast Crud")]
        public void WhenIInsertSingleIntKeyEntitiesUsingSimpleCrud(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSingleIntPrimaryKeyEntity(entityIndex);

                dbConnection.Insert(generatedEntity, commandTimeout: (TimeSpan?)null);
                // the entity already has the associated id set

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [Then(@"I should have (.*) single int key entities in the database")]
        public void ThenIShouldHaveSingleIntKeyEntitiesInTheDatabase(int entitiesCount)
        {
            var entities = _testContext.DatabaseConnection.GetAll<SingleIntPrimaryKeyEntity>();
            Assert.AreEqual(entities.Count(), entitiesCount);
        }

        [When(@"I select all the single int key entities using Fast Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            _testContext.QueriedEntities.AddRange(dbConnection.GetAll<SingleIntPrimaryKeyEntity>());
        }

        [When(@"I select all the single int key entities that I previously inserted using Fast Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SingleIntPrimaryKeyEntity>();
            foreach (var entity in _testContext.InsertedEntities.OfType<SingleIntPrimaryKeyEntity>())
            {
                _testContext.QueriedEntities.Add(dbConnection.GetByPrimaryKeys<SingleIntPrimaryKeyEntity>(new SingleIntPrimaryKeyEntity() {Id = entity.Id}));
            }
        }

        [When(@"I update all the single int key entities that I previously inserted using Fast Crud")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingFastCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var entityIndex = _testContext.InsertedEntities.Count;

            foreach (var entity in _testContext.InsertedEntities.OfType<SingleIntPrimaryKeyEntity>())
            {
                var newEntity = this.GenerateSingleIntPrimaryKeyEntity(entityIndex++);
                newEntity.Id = entity.Id;
                Assert.IsTrue(dbConnection.UpdateByPrimaryKeys(newEntity));
                _testContext.UpdatedEntities.Add(newEntity);
            }
        }

    }
}
