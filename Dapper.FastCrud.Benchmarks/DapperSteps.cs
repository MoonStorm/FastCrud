namespace Dapper.FastCrud.Benchmarks
{
    using Dapper.FastCrud.Benchmarks.Models;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Dapper.FastCrud.Tests;
    using Dapper.FastCrud.Tests.Contexts;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using System;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class DapperSteps:EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        private const string _tableName = "SimpleBenchmarkEntities";
        private string _insertSql = $"INSERT INTO {_tableName} ({nameof(SimpleBenchmarkEntity.FirstName)}, {nameof(SimpleBenchmarkEntity.LastName)}, {nameof(SimpleBenchmarkEntity.DateOfBirth)}) VALUES (@FirstName, @LastName, @DateOfBirth); SELECT SCOPE_IDENTITY() AS {nameof(SimpleBenchmarkEntity.Id)} ";
        private string _selectAllSql = $"SELECT {nameof(SimpleBenchmarkEntity.Id)}, {nameof(SimpleBenchmarkEntity.FirstName)}, {nameof(SimpleBenchmarkEntity.LastName)}, {nameof(SimpleBenchmarkEntity.DateOfBirth)} FROM {_tableName}";
        private string _selectByIdSql = $"SELECT {nameof(SimpleBenchmarkEntity.Id)}, {nameof(SimpleBenchmarkEntity.FirstName)}, {nameof(SimpleBenchmarkEntity.LastName)}, {nameof(SimpleBenchmarkEntity.DateOfBirth)} FROM {_tableName} where {nameof(SimpleBenchmarkEntity.Id)}=@Id";
        private string _updateSql = $"UPDATE {_tableName} SET {nameof(SimpleBenchmarkEntity.FirstName)}=@{nameof(SimpleBenchmarkEntity.FirstName)}, {nameof(SimpleBenchmarkEntity.LastName)}=@{nameof(SimpleBenchmarkEntity.LastName)}, {nameof(SimpleBenchmarkEntity.DateOfBirth)}=@{nameof(SimpleBenchmarkEntity.DateOfBirth)}  WHERE {nameof(SimpleBenchmarkEntity.Id)}=@Id";
        private string _deleteByIdSql = $"DELETE FROM {_tableName} WHERE {nameof(SimpleBenchmarkEntity.Id)}=@Id";

        public DapperSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) benchmark entities using ADO \.NET")]
        public void WhenIInsertBenchmarkEntitiesUsingADONET(int entitiesCount)
        {
            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                using (var sqlInsertCommand = new SqlCommand(_insertSql, (SqlConnection)_testContext.DatabaseConnection))
                {
                    sqlInsertCommand.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = generatedEntity.FirstName });
                    sqlInsertCommand.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = generatedEntity.LastName });
                    sqlInsertCommand.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.DateTime) { Value = generatedEntity.DateOfBirth });
                    generatedEntity.Id = Convert.ToInt32(sqlInsertCommand.ExecuteScalar());
                }

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.RecordInsertedEntity(generatedEntity);
            }
        }

        [When(@"I insert (.*) benchmark entities using Dapper")]
        public void WhenIInsertSingleIntKeyEntitiesUsingDapper(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);
                generatedEntity.Id = dbConnection.ExecuteScalar<int>(_insertSql, generatedEntity);
                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.RecordInsertedEntity(generatedEntity);
            }
        }

        [When(@"I select all the benchmark entities using Dapper")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var queriedEntity in dbConnection.Query<SimpleBenchmarkEntity>(_selectAllSql))
            {
                _testContext.RecordInsertedEntity(queriedEntity);
            }
        }

        [When(@"I select all the benchmark entities that I previously inserted using Dapper")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                _testContext.RecordInsertedEntity(dbConnection.Query<SimpleBenchmarkEntity>(_selectByIdSql,new { Id = entity.Id }));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Dapper")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;

            var entityIndex = 0;
            foreach (var oldEntity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityIndex++);
                newEntity.Id = oldEntity.Id;
                dbConnection.Execute(_updateSql, newEntity);
                _testContext.RecordUpdatedEntity(newEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Dapper")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingDapper()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                dbConnection.Execute(_deleteByIdSql, new {Id=entity.Id});
            }
        }
    }
}
