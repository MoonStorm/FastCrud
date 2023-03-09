namespace Dapper.FastCrud.Tests.DatabaseSetup
{
    using System.Data.SQLite;
    using Dapper.FastCrud.Tests.Contexts;
    using Microsoft.Extensions.Configuration;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class SqLiteDatabaseSteps:CommonDatabaseSetup
    {
        private readonly DatabaseTestContext _testContext;
        private readonly IConfiguration _configuration;

        public SqLiteDatabaseSteps(DatabaseTestContext testContext, IConfiguration configuration)
        {
            _testContext = testContext;
            _configuration = configuration;
        }

        [Given(@"I have initialized a SqLite database")]
        public void GivenIHaveInitializedSqlLiteDatabase()
        {
            var connectionString = this.GetConnectionStringFor(_configuration, "SqLite");

            this.SetupOrmConfiguration(SqlDialect.SqLite);

            _testContext.DatabaseConnection = new SQLiteConnection(connectionString);
            _testContext.DatabaseConnection.Open();

            using (var command = _testContext.DatabaseConnection.CreateCommand())
            {
                command.CommandText =
                    $@"CREATE TABLE Workstations (
	                        WorkstationId integer primary key AUTOINCREMENT,
                            InventoryIndex int NOT NULL,
	                        Name nvarchar(100) NULL,
                            AccessLevel int NOT NULL DEFAULT(1),
                            BuildingId int NULL
                        )";

                command.ExecuteNonQuery();
            }

            using (var command = _testContext.DatabaseConnection.CreateCommand())
            {
                command.CommandText = $@"CREATE TABLE Buildings (
	                        Id integer primary key AUTOINCREMENT,
	                        BuildingName nvarchar(100) NULL,
	                        Description nvarchar(100) NULL
                        )";

                command.ExecuteNonQuery();
            }

        }

    }
}