namespace Dapper.FastCrud.Tests.DatabaseSetup
{
    using System.Data.Odbc;
    using Dapper.FastCrud.Tests.Contexts;
    using Microsoft.Extensions.Configuration;
    using System.Data.SQLite;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class SqlAnywhereDatabaseSteps : CommonDatabaseSetup
    {
        private readonly DatabaseTestContext _testContext;
        private readonly IConfiguration _configuration;

        public SqlAnywhereDatabaseSteps(DatabaseTestContext testContext, IConfiguration configuration)
        {
            _testContext = testContext;
            _configuration = configuration;
        }

        [Given(@"I have initialized a SqlAnywhere database")]
        public void GivenIHaveInitializedSqlAnywhereDatabase()
        {
            this.SetupOrmConfiguration(SqlDialect.SqlAnywhere);

            var connectionString = this.GetConnectionStringFor(_configuration, "SQLAnywhere");

            _testContext.DatabaseConnection = new OdbcConnection(connectionString);
            _testContext.DatabaseConnection.Open();

            using (var command = _testContext.DatabaseConnection.CreateCommand())
            {
                command.CommandText =
                    $@"
                        DROP TABLE IF EXISTS ""Employee""

                        CREATE TABLE ""Employee"" (
	                        ""Id"" int NOT NULL DEFAULT AUTOINCREMENT,
                            ""EmployeeId"" uniqueidentifier NOT NULL DEFAULT NEWID(),
	                        ""KeyPass"" uniqueidentifier NOT NULL DEFAULT NEWID(),
	                        ""LastName"" varchar(100) NOT NULL,
	                        ""FirstName"" varchar(100) NOT NULL,
                            ""FullName"" varchar(200) NOT NULL COMPUTE( ""FirstName"" + ""LastName"" ),
	                        ""BirthDate"" timestamp NOT NULL,
	                        ""RecordIndex"" int NOT NULL,
                            ""WorkstationId"" int NULL,
	                        ""SupervisorUserId"" int NULL,
                            ""SupervisorEmployeeId"" uniqueidentifier NULL,
	                        ""ManagerUserId"" int NULL,
	                        ""ManagerEmployeeId"" uniqueidentifier NULL,
	                        PRIMARY KEY (""Id"", ""EmployeeId"")
                        );

                        DROP TABLE IF EXISTS ""Buildings""

                        CREATE TABLE ""Buildings"" (
	                        ""Id"" int NOT NULL DEFAULT AUTOINCREMENT,
	                        ""BuildingName"" nvarchar(100) NULL,
                            ""Description"" nvarchar(100) NULL,
                            PRIMARY KEY(""Id"")
                        );

                        DROP TABLE IF EXISTS ""Workstations""

                        CREATE TABLE ""Workstations"" (
	                        ""WorkstationId"" bigint NOT NULL DEFAULT AUTOINCREMENT,
	                        ""Name"" nvarchar(100) NOT NULL,
                            ""InventoryIndex"" int NOT NULL,
                            ""AccessLevel"" int NOT NULL DEFAULT 1,
                            ""BuildingId"" int NULL,
	                        PRIMARY KEY (""WorkstationId"")
                        );

                    ";

                command.ExecuteNonQuery();
            }
        }
    }
}
