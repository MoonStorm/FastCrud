namespace Dapper.FastCrud.Tests.Features
{
    using System.Linq;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class DapperSteps:EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public DapperSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) single int key entities using Dapper")]
        public void WhenIInsertSingleIntKeyEntitiesUsingDapper(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SingleIntPrimaryKeyEntity>();

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSingleIntPrimaryKeyEntity(entityIndex);

                generatedEntity.Id = dbConnection.ExecuteScalar<int>(
                    $"INSERT INTO {tableName} ({nameof(SingleIntPrimaryKeyEntity.FirstName)}, {nameof(SingleIntPrimaryKeyEntity.LastName)}, {nameof(SingleIntPrimaryKeyEntity.DateOfBirth)}) VALUES (@FirstName, @LastName, @BirthDate); select scope_identity();",
                    new {
                            FirstName = generatedEntity.FirstName,
                            LastName = generatedEntity.LastName,
                            BirthDate = generatedEntity.DateOfBirth
                        });

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I select all the single int key entities using Dapper")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SingleIntPrimaryKeyEntity>();
            
            _testContext.QueriedEntities.AddRange(dbConnection.Query<SingleIntPrimaryKeyEntity>($"SELECT {nameof(SingleIntPrimaryKeyEntity.Id)}, {nameof(SingleIntPrimaryKeyEntity.FirstName)}, {nameof(SingleIntPrimaryKeyEntity.LastName)}, {nameof(SingleIntPrimaryKeyEntity.DateOfBirth)} FROM {tableName}"));
        }

        [When(@"I select all the single int key entities that I previously inserted using Dapper")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SingleIntPrimaryKeyEntity>();
            foreach (var entity in _testContext.InsertedEntities.OfType<SingleIntPrimaryKeyEntity>())
            {
                _testContext.QueriedEntities.AddRange(
                    dbConnection.Query<SingleIntPrimaryKeyEntity>(
                        $"SELECT {nameof(SingleIntPrimaryKeyEntity.Id)}, {nameof(SingleIntPrimaryKeyEntity.FirstName)}, {nameof(SingleIntPrimaryKeyEntity.LastName)}, {nameof(SingleIntPrimaryKeyEntity.DateOfBirth)} FROM {tableName} where {nameof(SingleIntPrimaryKeyEntity.Id)}=@Id",
                        new { Id = entity.Id }));
            }
        }

        [When(@"I update all the single int key entities that I previously inserted using Dapper")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SingleIntPrimaryKeyEntity>();
            var entityIndex = _testContext.InsertedEntities.Count;

            foreach (var entity in _testContext.InsertedEntities.OfType<SingleIntPrimaryKeyEntity>())
            {
                var newEntity = this.GenerateSingleIntPrimaryKeyEntity(entityIndex++);
                newEntity.Id = entity.Id;

                Assert.AreEqual(
                    dbConnection.Execute(
                        $"UPDATE {tableName} SET {nameof(SingleIntPrimaryKeyEntity.FirstName)}=@{nameof(SingleIntPrimaryKeyEntity.FirstName)}, {nameof(SingleIntPrimaryKeyEntity.LastName)}=@{nameof(SingleIntPrimaryKeyEntity.LastName)}, {nameof(SingleIntPrimaryKeyEntity.DateOfBirth)}=@{nameof(SingleIntPrimaryKeyEntity.DateOfBirth)}  WHERE {nameof(SingleIntPrimaryKeyEntity.Id)}=@Id",
                        newEntity),
                    1);

                _testContext.UpdatedEntities.Add(newEntity);
            }
        }

        [When(@"I delete all the inserted single int key entities using Dapper")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SingleIntPrimaryKeyEntity>();

            foreach (var entity in _testContext.InsertedEntities.OfType<SingleIntPrimaryKeyEntity>())
            {
                Assert.AreEqual(dbConnection.Execute($"DELETE FROM {tableName} WHERE {nameof(SingleIntPrimaryKeyEntity.Id)}=@Id", entity),1);
            }
        }
    }
}
