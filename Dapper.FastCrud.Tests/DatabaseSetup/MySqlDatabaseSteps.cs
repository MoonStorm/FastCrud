namespace Dapper.FastCrud.Tests.DatabaseSetup
{
    using Dapper.FastCrud.Tests.Contexts;
    using Microsoft.Extensions.Configuration;
    using MySql.Data.MySqlClient;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class MySqlDatabaseSteps: CommonDatabaseSetup
    {
        private readonly DatabaseTestContext _testContext;
        private readonly IConfiguration _configuration;

        public MySqlDatabaseSteps(DatabaseTestContext testContext, IConfiguration configuration)
        {
            _testContext = testContext;
            _configuration = configuration;
        }

        [Given(@"I have initialized a MySql database")]
        public void GivenIHaveInitializedMySqlDatabase()
        {
            var connectionString = this.GetConnectionStringFor(_configuration, "MySql");
            this.CleanupMySqlDatabase(connectionString);
            this.SetupMySqlDatabase(connectionString);
        }

        private void CleanupMySqlDatabase(string connectionString)
        {
            using (var dataConnection = new MySqlConnection(connectionString))
            {
                dataConnection.Open();
                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"DROP DATABASE IF EXISTS {_testContext.DatabaseName}";
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetupMySqlDatabase(string connectionString)
        {
            this.SetupOrmConfiguration(SqlDialect.MySql);

            using (var dataConnection = new MySqlConnection(connectionString))
            {
                dataConnection.Open();

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"CREATE DATABASE {_testContext.DatabaseName}";
                    command.ExecuteNonQuery();
                }

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"USE {_testContext.DatabaseName};

                        CREATE TABLE `Employee` (
	                        Id int NOT NULL AUTO_INCREMENT,
                            EmployeeId CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	                        KeyPass CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	                        LastName nvarchar(100) NOT NULL,
	                        FirstName nvarchar(100) NOT NULL,
                            FullName nvarchar(200) AS (CONCAT(FirstName,LastName)),
	                        BirthDate datetime NOT NULL,
    	                    RecordIndex int NOT NULL,
                            RecordVersion binary(11) NOT NULL,
                            WorkstationId bigint NULL,
	                        SupervisorUserId int NULL,
                            SupervisorEmployeeId char(36) NULL,
	                        ManagerUserId int NULL,
	                        ManagerEmployeeId char(36) NULL,
	                        PRIMARY KEY (Id, EmployeeId)
                        );

                        ALTER TABLE `Employee` auto_increment=2;

                        CREATE TRIGGER `Employee_Assign_UUID`
                            BEFORE INSERT ON Employee
                            FOR EACH ROW
                            SET NEW.EmployeeId = UUID(),
                            New.KeyPass = UUID();

                        CREATE TRIGGER `Employee_Assign_RecordVersion`
                            BEFORE INSERT ON Employee
                            FOR EACH ROW
                            SET NEW.RecordVersion = CONVERT(UNIX_TIMESTAMP() USING BINARY);

                        CREATE TRIGGER `Employee_Update_RecordVersion`
                            BEFORE UPDATE ON Employee
                            FOR EACH ROW
                            SET NEW.RecordVersion = CONVERT(UNIX_TIMESTAMP() USING BINARY);

                        CREATE TABLE `Badges` (
	                        Id int NOT NULL,
                            EmployeeId CHAR(36) NOT NULL,
	                        Barcode nvarchar(100) NOT NULL,
	                        PRIMARY KEY (Id, EmployeeId)
                        );

                        CREATE TABLE `Workstations` (
	                        WorkstationId bigint NOT NULL AUTO_INCREMENT,
	                        Name nvarchar(100) NOT NULL,
                            InventoryIndex int NOT NULL,
                            AccessLevel int NOT NULL DEFAULT 1,
                            BuildingId int NULL,
	                        PRIMARY KEY (WorkstationId)
                        );

                        ALTER TABLE `Workstations` auto_increment=2;

                        CREATE TABLE `Buildings` (
	                        Id int NOT NULL AUTO_INCREMENT,
	                        BuildingName nvarchar(100) NULL,
                            Description nvarchar(100) NULL,
	                        PRIMARY KEY (Id)
                        );

                        ALTER TABLE `Buildings` auto_increment=2;

                    ";
                    command.ExecuteNonQuery();
                }
            }


            _testContext.DatabaseConnection = new MySqlConnection($"{connectionString};Database={_testContext.DatabaseName}");
            _testContext.DatabaseConnection.Open();
        }
    }
}