namespace Dapper.FastCrud.Tests.Features.Benchmarks
{
    using System.Linq;
    using Dapper.FastCrud.Benchmarks;
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

        [When(@"I insert (.*) benchmark entities using Dapper")]
        public void WhenIInsertSingleIntKeyEntitiesUsingDapper(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SimpleBenchmarkEntity>();
            string sql = $"INSERT INTO {tableName} ({nameof(SimpleBenchmarkEntity.FirstName)}, {nameof(SimpleBenchmarkEntity.LastName)}, {nameof(SimpleBenchmarkEntity.DateOfBirth)}) OUTPUT inserted.{nameof(SimpleBenchmarkEntity.Id)} VALUES (@FirstName, @LastName, @BirthDate)";

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                generatedEntity.Id = dbConnection.ExecuteScalar<int>(sql,
                    new {
                            FirstName = generatedEntity.FirstName,
                            LastName = generatedEntity.LastName,
                            BirthDate = generatedEntity.DateOfBirth
                        });

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I select all the benchmark entities using Dapper")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SimpleBenchmarkEntity>();
            
            _testContext.QueriedEntities.AddRange(dbConnection.Query<SimpleBenchmarkEntity>($"SELECT {nameof(SimpleBenchmarkEntity.Id)}, {nameof(SimpleBenchmarkEntity.FirstName)}, {nameof(SimpleBenchmarkEntity.LastName)}, {nameof(SimpleBenchmarkEntity.DateOfBirth)} FROM {tableName}"));
        }

        [When(@"I select all the benchmark entities that I previously inserted using Dapper")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SimpleBenchmarkEntity>();
            string sql = $"SELECT {nameof(SimpleBenchmarkEntity.Id)}, {nameof(SimpleBenchmarkEntity.FirstName)}, {nameof(SimpleBenchmarkEntity.LastName)}, {nameof(SimpleBenchmarkEntity.DateOfBirth)} FROM {tableName} where {nameof(SimpleBenchmarkEntity.Id)}=@Id";

            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                _testContext.QueriedEntities.AddRange(
                    dbConnection.Query<SimpleBenchmarkEntity>(
                        sql,
                        new { Id = entity.Id }));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Dapper")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SimpleBenchmarkEntity>();
            var entityIndex = _testContext.InsertedEntities.Count;
            string sql = $"UPDATE {tableName} SET {nameof(SimpleBenchmarkEntity.FirstName)}=@{nameof(SimpleBenchmarkEntity.FirstName)}, {nameof(SimpleBenchmarkEntity.LastName)}=@{nameof(SimpleBenchmarkEntity.LastName)}, {nameof(SimpleBenchmarkEntity.DateOfBirth)}=@{nameof(SimpleBenchmarkEntity.DateOfBirth)}  WHERE {nameof(SimpleBenchmarkEntity.Id)}=@Id";

            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityIndex++);
                newEntity.Id = entity.Id;

                Assert.AreEqual(dbConnection.Execute(sql,newEntity),1);

                _testContext.UpdatedEntities.Add(newEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Dapper")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SimpleBenchmarkEntity>();
            string sql = $"DELETE FROM {tableName} WHERE {nameof(SimpleBenchmarkEntity.Id)}=@Id";

            foreach (var entity in _testContext.InsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                Assert.AreEqual(dbConnection.Execute(sql, entity),1);
            }
        }
    }
}
