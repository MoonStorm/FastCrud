namespace Dapper.FastCrud.Tests.Features
{
    using System.Linq;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class SimpleCrudSteps:EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public SimpleCrudSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) single int key entities using Simple Crud")]
        public void WhenIInsertSingleIntKeyEntitiesUsingSimpleCrud(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSingleIntPrimaryKeyEntity(entityIndex);

                generatedEntity.Id = dbConnection.Insert(generatedEntity, commandTimeout: (int?)null).Value;

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I select all the single int key entities using Simple Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingSimpleCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            _testContext.QueriedEntities.AddRange(dbConnection.GetList<SingleIntPrimaryKeyEntity>());
        }

        [When(@"I select all the single int key entities that I previously inserted using Simple Crud")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingSimpleCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var entity in _testContext.InsertedEntities.OfType<SingleIntPrimaryKeyEntity>())
            {
                _testContext.QueriedEntities.Add(dbConnection.Get<SingleIntPrimaryKeyEntity>(entity.Id));
            }
        }

        [When(@"I update all the single int key entities that I previously inserted using Simple Crud")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingSimpleCrud()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var entityIndex = _testContext.InsertedEntities.Count;

            foreach (var entity in _testContext.InsertedEntities.OfType<SingleIntPrimaryKeyEntity>())
            {
                var newEntity = this.GenerateSingleIntPrimaryKeyEntity(entityIndex++);
                newEntity.Id = entity.Id;
                Assert.AreEqual(dbConnection.Update(newEntity), 1);
                _testContext.UpdatedEntities.Add(newEntity);
            }
        }
    }
}
