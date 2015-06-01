namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Tests.Models;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;
    using MySql.Data.MySqlClient;
    using Npgsql;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class DatabaseSteps
    {
        private DatabaseTestContext _testContext;
        private const string DatabaseName = "TestDatabase";
        //private static string OriginalDatabaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        //private static string FinalDatabaseFolder = Path.Combine(OriginalDatabaseFolder, $"FastCrudDatabaseTests");

        public DatabaseSteps(DatabaseTestContext testContext)
        {
            this._testContext = testContext;
        }

        [Given(@"I have initialized a PostgreSql database")]
        public void GivenIHaveInitializedPostgreSqlDatabase()
        {
            this.CleanupPostgreSqlDatabase(ConfigurationManager.ConnectionStrings["PostgreSql"].ConnectionString);
            this.SetupPostgreSqlDatabase(ConfigurationManager.ConnectionStrings["PostgreSql"].ConnectionString);
        }

        [Given(@"I have initialized a LocalDb database")]
        public void GivenIHaveInitializedLocalDbDatabase()
        {
            this.CleanupMsSqlDatabase(ConfigurationManager.ConnectionStrings["LocalDb"].ConnectionString);
            this.SetupMsSqlDatabase(ConfigurationManager.ConnectionStrings["LocalDb"].ConnectionString);
        }

        [Given(@"I have initialized a SqLite database")]
        public void GivenIHaveInitializedSqlLiteDatabase()
        {
            this.SetupSqLiteDatabase(ConfigurationManager.ConnectionStrings["SqLite"].ConnectionString);
        }

        [Given(@"I have initialized a MySql database")]
        public void GivenIHaveInitializedMySqlDatabase()
        {
            this.CleanupMySqlDatabase(ConfigurationManager.ConnectionStrings["MySql"].ConnectionString);
            this.SetupMySqlDatabase(ConfigurationManager.ConnectionStrings["MySql"].ConnectionString);
        }

        [Then(@"I cleanup the LocalDb database")]
        public void ThenICleanupTheLocalDbDatabase()
        {
            this.CleanupMsSqlDatabase(ConfigurationManager.ConnectionStrings["LocalDb"].ConnectionString);
        }

        [Given(@"I have initialized a MsSqlServer database")]
        public void GivenIHaveInitializedAnMsSqlServerDatabase()
        {
            this.CleanupMsSqlDatabase(ConfigurationManager.ConnectionStrings["MsSqlServer"].ConnectionString);
            this.SetupMsSqlDatabase(ConfigurationManager.ConnectionStrings["MsSqlServer"].ConnectionString);
        }

        [Then(@"I cleanup the MsSqlServer database")]
        public void ThenICleanupTheMsSqlServerDatabase()
        {
            this.CleanupMsSqlDatabase(ConfigurationManager.ConnectionStrings["MsSqlServer"].ConnectionString);
        }

        [When(@"I refresh the database connection")]
        public void WhenIRefreshTheDatabaseConnection()
        {
            _testContext.DatabaseConnection.Close();
            _testContext.DatabaseConnection.Open();
        }

        [Given(@"I started the stopwatch")]
        [When(@"I start the stopwatch")]
        public void GivenIStartedTheStopwatch()
        {
            _testContext.Stopwatch.Restart();
        }

        [When(@"I stop the stopwatch")]
        public void WhenIStopTheStopwatch()
        {
            _testContext.Stopwatch.Stop();
        }

        [When(@"I report the stopwatch value for (.*) finished processing (.*) operations of type (.*)")]
        public void WhenIReportTheStopwatchValueFor(string ormType, int opCount, string operation)
        {
            Trace.WriteLine($"Stopwatch reported: {_testContext.Stopwatch.Elapsed.TotalMilliseconds:0,0.00} milliseconds for {ormType}");

            // automatically update the docs
            var docsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../README.MD");
            var docsContents = File.ReadAllText(docsPath);

            var reportTitle = $"{ormType} | {operation} | {opCount} |";
            var report = $"{reportTitle} {_testContext.Stopwatch.Elapsed.TotalMilliseconds:0,0.00} | {_testContext.Stopwatch.Elapsed.TotalMilliseconds*1000/opCount:0,0.00}  {Environment.NewLine}";

            var benchmarkHeaderRegex = new Regex($@"(?<=#+\s*?Automatic Benchmark Report)[^{Environment.NewLine}]*", RegexOptions.Singleline);
            var emptySpaceInsertRegex = new Regex($@"(?<=#+\s*?Automatic Benchmark Report(.*?{Environment.NewLine}){{3,3}})\s*?", RegexOptions.Singleline);
            var reportReplaceRegex = new Regex($@"{reportTitle.Replace("|",@"\|")}.*?{Environment.NewLine}", RegexOptions.Singleline);

            if (reportReplaceRegex.Match(docsContents).Success)
            {
                docsContents = reportReplaceRegex.Replace(docsContents, report, 1);
            }
            else
            {
                docsContents = emptySpaceInsertRegex.Replace(docsContents, report, 1);
            }

            docsContents = benchmarkHeaderRegex.Replace(docsContents, $" (Last Run: {DateTime.Now:D})", 1);

            File.WriteAllText(docsPath, docsContents);
        }

        [Then(@"I should have queried (.*) entities")]
        public void ThenIShouldHaveQueriedEntities(int queryEntitiesCount)
        {
            Assert.AreEqual(queryEntitiesCount, _testContext.QueriedEntities.Count());
        }

        [Then(@"the queried entities should be the same as the ones I inserted, in reverse order, starting from (.*) counting (.*)")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheOnesIInsertedReverseStartingFromCounting(int skip, int max)
        {
            var expectedEntities = ((IEnumerable<object>)_testContext.InsertedEntities).Reverse().Skip(skip).Take(max);
            CollectionAssert.AreEqual(expectedEntities, _testContext.QueriedEntities);
        }

        [Then(@"the queried entities should be the same as the ones I inserted")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheOnesIInserted()
        {
            CollectionAssert.AreEquivalent(_testContext.QueriedEntities, _testContext.InsertedEntities);
        }

        [When(@"I clear all the queried entities")]
        public void WhenIClearAllTheQueriedEntities()
        {
            _testContext.QueriedEntities.Clear();
        }

        [When(@"I clear all the inserted entities")]
        public void WhenIClearAllTheInsertedEntities()
        {
            _testContext.InsertedEntities.Clear();
        }

        [Then(@"the queried entities should be the same as the ones I updated")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheOnesIUpdated()
        {
            CollectionAssert.AreEquivalent(_testContext.QueriedEntities, _testContext.UpdatedEntities);
        }

        [AfterScenario]
        public void Cleanup()
        {
            _testContext.DatabaseConnection?.Close();
            _testContext.DatabaseConnection?.Dispose();
        }

        private void SetupSqLiteDatabase(string connectionString)
        {
            SetupOrmConfiguration(SqlDialect.SqLite);

            _testContext.DatabaseConnection = new SQLiteConnection(connectionString);
            _testContext.DatabaseConnection.Open();

            using (var command = _testContext.DatabaseConnection.CreateCommand())
            {
                command.CommandText =
                    $@"CREATE TABLE Workstations (
	                        WorkstationId integer primary key AUTOINCREMENT,
	                        Name nvarchar(50) NULL,
                            AccessLevel int NOT NULL DEFAULT(1)
                        )";

                command.ExecuteNonQuery();
            }

            ////using (var command = _testContext.DatabaseConnection.CreateCommand())
            ////{
            ////    command.CommandText =
            ////        $@"CREATE TABLE SimpleBenchmarkEntities(
	           ////         Id integer primary key AUTOINCREMENT,
            ////            FirstName nvarchar(50) NULL,
	           ////         LastName  nvarchar(50) NOT NULL,
            ////            DateOfBirth datetime NULL
            ////        ";

            ////    command.ExecuteNonQuery();
            ////}

            using (var command = _testContext.DatabaseConnection.CreateCommand())
            {
                command.CommandText = $@"CREATE TABLE Buildings (
	                        BuildingId integer primary key,
	                        Name nvarchar(50) NULL
                        )";

                command.ExecuteNonQuery();
            }

        }

        private void CleanupPostgreSqlDatabase(string connectionString)
        {
            using (var dataConnection = new NpgsqlConnection(connectionString))
            {
                dataConnection.Open();

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText =
                        $@"
                        select pg_terminate_backend(pid)
                        from pg_stat_activity
                        where datname = '{
                            DatabaseName.ToLowerInvariant()}'";
                    command.ExecuteNonQuery();
                }

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"DROP DATABASE IF EXISTS { DatabaseName.ToLowerInvariant()}";
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetupPostgreSqlDatabase(string connectionString)
        {
            SetupOrmConfiguration(SqlDialect.PostgreSql);

            using (var dataConnection = new NpgsqlConnection(connectionString))
            {
                dataConnection.Open();

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"CREATE DATABASE {DatabaseName}";
                    command.ExecuteNonQuery();
                }
            }

            using (var dataConnection = new NpgsqlConnection(connectionString + $";Database={DatabaseName.ToLowerInvariant()}"))
            {
                //  uuid_in((md5((random())::text))::cstring)
                dataConnection.Open();
                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText =$@"
                        CREATE TABLE Employee (
	                        UserId SERIAL,
                            EmployeeId uuid NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
	                        KeyPass uuid NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
	                        LastName varchar(50) NOT NULL,
	                        FirstName varchar(50) NOT NULL,
	                        BirthDate timestamp NOT NULL,
                            WorkstationId int NULL,
	                        PRIMARY KEY (UserId, EmployeeId)
                        );

                        CREATE TABLE Workstations (
	                        WorkstationId BIGSERIAL,
	                        Name varchar(50) NOT NULL,
                            AccessLevel int NOT NULL DEFAULT 1,
	                        PRIMARY KEY (WorkstationId)
                        );

                        CREATE TABLE Buildings (
	                        BuildingId int NOT NULL,
	                        Name varchar(50) NULL,
	                        PRIMARY KEY (BuildingId)
                        );
                    ";
                    command.ExecuteNonQuery();
                }
            }

            _testContext.DatabaseConnection = new NpgsqlConnection(connectionString + $";Database={DatabaseName.ToLowerInvariant()}");
            _testContext.DatabaseConnection.Open();
        }


        private void CleanupMySqlDatabase(string connectionString)
        {
            using (var dataConnection = new MySqlConnection(connectionString))
            {
                dataConnection.Open();
                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"DROP DATABASE IF EXISTS {DatabaseName}";
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetupMySqlDatabase(string connectionString)
        {
            SetupOrmConfiguration(SqlDialect.MySql);

            using (var dataConnection = new MySqlConnection(connectionString))
            {
                dataConnection.Open();

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"CREATE DATABASE {DatabaseName}";
                    command.ExecuteNonQuery();
                }

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"USE {DatabaseName};

                        CREATE TABLE `Employee` (
	                        UserId int NOT NULL AUTO_INCREMENT,
                            EmployeeId CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	                        KeyPass CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	                        LastName nvarchar(50) NOT NULL,
	                        FirstName nvarchar(50) NOT NULL,
	                        BirthDate datetime NOT NULL,
                            WorkstationId int NULL,
	                        PRIMARY KEY (UserId, EmployeeId)
                        );

                        ALTER TABLE Employee auto_increment=2;

                        CREATE TRIGGER `Employee_Assign_UUID`
                            BEFORE INSERT ON Employee
                            FOR EACH ROW
                            SET NEW.EmployeeId = UUID(),
                            New.KeyPass = UUID();

                        CREATE TABLE `Workstations` (
	                        WorkstationId bigint NOT NULL AUTO_INCREMENT,
	                        Name nvarchar(50) NOT NULL,
                            AccessLevel int NOT NULL DEFAULT 1,
	                        PRIMARY KEY (WorkstationId)
                        );

                        ALTER TABLE Workstations auto_increment=2;

                        CREATE TABLE `Buildings` (
	                        BuildingId int NOT NULL,
	                        Name nvarchar(50) NULL,
	                        PRIMARY KEY (BuildingId)
                        );
                    ";
                    command.ExecuteNonQuery();
                }
            }


            _testContext.DatabaseConnection = new MySqlConnection(connectionString+$";Database={DatabaseName}");
            _testContext.DatabaseConnection.Open();
        }

        private void SetupMsSqlDatabase(string connectionString)
        {
            SetupOrmConfiguration(SqlDialect.MsSql);

            using (var dataConnection = new SqlConnection(connectionString))
            {
                dataConnection.Open();

                var server = new Server(new ServerConnection(dataConnection));
                var database = new Database(server, DatabaseName);
                database.Create();
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET AUTO_CREATE_STATISTICS OFF"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET AUTO_UPDATE_STATISTICS OFF"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} MODIFY FILE(NAME=[{DatabaseName}], SIZE=50MB, FILEGROWTH=10%)"); // for benchmarking purposes 5MB approx 20k records
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET RECOVERY SIMPLE"); // for benchmarking purposes                

                database.ExecuteNonQuery(@"CREATE TABLE [dbo].[SimpleBenchmarkEntities](
	                    [Id] [int] IDENTITY(2,1) NOT NULL,
	                    [FirstName] [nvarchar](50) NULL,
	                    [LastName] [nvarchar](50) NOT NULL,
	                    [DateOfBirth] [datetime] NULL,
                        CONSTRAINT [PK_SimpleBenchmarkEntities] PRIMARY KEY CLUSTERED 
                        (
	                        [Id] ASC
                        ))");

                database.ExecuteNonQuery(@"CREATE TABLE [dbo].[Workstations](
	                    [WorkstationId] [bigint] IDENTITY(2,1) NOT NULL,
	                    [Name] [nvarchar](50) NULL,
                        [AccessLevel] [int] NOT NULL DEFAULT(1),
                        CONSTRAINT [PK_Workstations] PRIMARY KEY CLUSTERED 
                        (
	                        [WorkstationId] ASC
                        ))");

                database.ExecuteNonQuery(@"CREATE TABLE [dbo].[Employee](
	                    [UserId] [int] IDENTITY(2,1) NOT NULL,
	                    [EmployeeId] [uniqueidentifier] NOT NULL DEFAULT(newid()),
	                    [KeyPass] [uniqueidentifier] NOT NULL DEFAULT(newid()),
	                    [LastName] [nvarchar](50) NOT NULL,
	                    [FirstName] [nvarchar](50) NULL,
	                    [BirthDate] [datetime] NOT NULL,
	                    [WorkstationId] [int] NULL,
                        CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
                        (
	                        [UserId] ASC,
	                        [EmployeeId] ASC
                        ))");

                database.ExecuteNonQuery(@"CREATE TABLE [dbo].[Buildings](
	                    [BuildingId] [int]  NOT NULL,
	                    [Name] [nvarchar](50) NULL,
                        CONSTRAINT [PK_Buildings] PRIMARY KEY CLUSTERED 
                        (
	                        [BuildingId] ASC
                        ))");

                ////// attach the test dbs
                ////var dataFiles = new StringCollection();

                ////var finalSqlDatabaseDataFilePath = Path.Combine(FinalDatabaseFolder, $"{MsSqlDatabaseName}.mdf");
                ////var finalSqlDatabaseLogFilePath = Path.Combine(FinalDatabaseFolder, $"{MsSqlDatabaseName}_log.ldf");

                ////Directory.CreateDirectory(FinalDatabaseFolder);
                ////File.Copy(Path.Combine(OriginalDatabaseFolder, $"{MsSqlDatabaseName}.mdf"), finalSqlDatabaseDataFilePath, true);
                ////File.Copy(Path.Combine(OriginalDatabaseFolder, $"{MsSqlDatabaseName}_log.ldf"), finalSqlDatabaseLogFilePath, true);

                ////dataFiles.AddRange(new[] { finalSqlDatabaseDataFilePath, finalSqlDatabaseLogFilePath });
                ////serverManagement.AttachDatabase(MsSqlDatabaseName, dataFiles);

                ////var database = serverManagement.Databases[MsSqlDatabaseName];
                //////database.ExecuteNonQuery(@"CHECKPOINT;");
                ////database.ExecuteNonQuery(@"DBCC DROPCLEANBUFFERS;");
                ////database.ExecuteNonQuery(@"DBCC FREEPROCCACHE WITH NO_INFOMSGS;");
            }

            _testContext.DatabaseConnection = new SqlConnection(connectionString+$";Initial Catalog={DatabaseName}");
            _testContext.DatabaseConnection.Open();
        }

        private void CleanupMsSqlDatabase(string connectionString)
        {
            _testContext.DatabaseConnection?.Close();

            using (var dataConnection = new SqlConnection(connectionString))
            {
                dataConnection.Open();
                var serverManagement = new Server(new ServerConnection(dataConnection));
                var database = serverManagement.Databases[DatabaseName];
                if (database != null)
                {
                    // drop any active connections
                    database.UserAccess = DatabaseUserAccess.Single;
                    database.Alter(TerminationClause.RollbackTransactionsImmediately);
                    database.Refresh();

                    database.UserAccess = DatabaseUserAccess.Multiple;
                    database.Alter(TerminationClause.RollbackTransactionsImmediately);
                    database.Refresh();

                    database.Drop();
                    //serverManagement.DetachDatabase(MsSqlDatabaseName, false, true);
                }
            }

            ////if (Directory.Exists(FinalDatabaseFolder))
            ////{
            ////    Directory.Delete(FinalDatabaseFolder, true);
            ////}
        }

        private void SetupOrmConfiguration(SqlDialect dialect)
        {
            OrmConfiguration.DefaultDialect = dialect;

            OrmConfiguration.RegisterEntity<Building>()
                .SetTableName("Buildings")
                .SetProperty(nameof(Building.BuildingId), PropertyMappingOptions.KeyProperty)
                .SetProperty(nameof(Building.Name));

        }
    }
}
