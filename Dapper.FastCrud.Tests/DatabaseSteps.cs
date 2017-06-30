namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Data.SqlLocalDb;
    using System.Data.SqlServerCe;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
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
        private const string DatabaseName = "FastCrudTestDatabase";
        //private static string OriginalDatabaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        //private static string FinalDatabaseFolder = Path.Combine(OriginalDatabaseFolder, $"FastCrudDatabaseTests");

        public DatabaseSteps(DatabaseTestContext testContext)
        {
            this._testContext = testContext;
        }

        [AfterScenario]
        public void Cleanup()
        {
            _testContext.DatabaseConnection?.Close();
            _testContext.DatabaseConnection?.Dispose();
            OrmConfiguration.ClearEntityRegistrations();
        }

        [Given(@"I have initialized a PostgreSql database")]
        public void GivenIHaveInitializedPostgreSqlDatabase()
        {
            this.CleanupPostgreSqlDatabase(ConfigurationManager.ConnectionStrings["PostgreSql"].ConnectionString);
            this.SetupPostgreSqlDatabase(ConfigurationManager.ConnectionStrings["PostgreSql"].ConnectionString);
        }

        [Given(@"I have initialized a Benchmark LocalDb database")]
        public void GivenIHaveInitializedBenchmarkLocalDbDatabase()
        {
            this.CleanupLocalDbDatabase();
            this.SetupLocalDbDatabase();
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

        [Given(@"I have initialized a SQLCE database")]
        public void GivenIHaveInitializedSqlCeDatabase()
        {
            this.CleanupSqlCeDatabase(ConfigurationManager.ConnectionStrings["SqlCompact"].ConnectionString);
            this.SetupSqlCeDatabase(ConfigurationManager.ConnectionStrings["SqlCompact"].ConnectionString);
        }

        [Then(@"I cleanup the SQLCE database")]
        public void ThenICleanupTheSqlCeDatabase()
        {
            this.CleanupSqlCeDatabase(ConfigurationManager.ConnectionStrings["SqlCompact"].ConnectionString);
        }

        [Then(@"I cleanup the Benchmark LocalDb database")]
        public void ThenICleanupTheBenchmarkLocalDbDatabase()
        {
            this.CleanupLocalDbDatabase();
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

            var reportTitle = $"| {ormType} | {operation} | {opCount} |";
            var report = $"{reportTitle} {_testContext.Stopwatch.Elapsed.TotalMilliseconds:0,0.00} | {_testContext.Stopwatch.Elapsed.TotalMilliseconds*1000/opCount:0,0.00} |{Environment.NewLine}";

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
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheOnesIInsertedReverseStartingFromCounting(int? skip, int? max)
        {
            var expectedEntities = ((IEnumerable<object>)_testContext.LocalInsertedEntities).Reverse().Skip(skip??0).Take(max??int.MaxValue);
            CollectionAssert.AreEqual(expectedEntities, _testContext.QueriedEntities);
        }

        [Then(@"the queried entities should be the same as the local ones")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheLocalOnes()
        {
            this.CompareQueriedEntitiesWithLocalEntities<object>();
        }

        [Then(@"the queried workstation entities should be the same as the local ones")]
        public void ThenTheQueriedWorkstationEntitiesShouldBeTheSameAsTheLocalOnes()
        {
            this.CompareQueriedEntitiesWithLocalEntities<Workstation>();
        }

        [Then(@"the queried employee entities should be the same as the local ones")]
        public void ThenTheQueriedEmployeeEntitiesShouldBeTheSameAsTheLocalOnes()
        {
            this.CompareQueriedEntitiesWithLocalEntities<Employee>();
        }

        [Then(@"the queried building entities should be the same as the local ones")]
        public void ThenTheQueriedBuildingEntitiesShouldBeTheSameAsTheLocalOnes()
        {
            this.CompareQueriedEntitiesWithLocalEntities<Building>();
        }

        [When(@"I clear all the queried entities")]
        public void WhenIClearAllTheQueriedEntities()
        {
            _testContext.QueriedEntities.Clear();
        }

        [When(@"I clear all the local entities")]
        public void WhenIClearAllTheInsertedEntities()
        {
            _testContext.LocalInsertedEntities.Clear();
        }

        private void SetupSqlCeDatabase(string connectionString)
        {
            this.SetupOrmConfiguration(SqlDialect.MsSql);

            connectionString = string.Format(
                CultureInfo.InvariantCulture,
                connectionString,
                Path.Combine(Path.GetDirectoryName(typeof(DatabaseSteps).Assembly.Location), $"{DatabaseName}.sdf"));

            // create the database
            using(var sqlCeEngine = new SqlCeEngine(connectionString))
            {
                sqlCeEngine.CreateDatabase();
            }

            _testContext.DatabaseConnection = new SqlCeConnection(connectionString);
            _testContext.DatabaseConnection.Open();

            using(var command = _testContext.DatabaseConnection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE [SimpleBenchmarkEntities](
	                    [Id] [int] IDENTITY(2,1) PRIMARY KEY,
	                    [FirstName] [nvarchar](50) NULL,
	                    [LastName] [nvarchar](50) NOT NULL,
	                    [DateOfBirth] [datetime] NULL
                )";
                command.ExecuteNonQuery();
            }
        }

        private void SetupSqLiteDatabase(string connectionString)
        {
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
                            BuildingId int NULL,
                            ""10PinSlots"" int NULL
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
	                        Id integer primary key AUTOINCREMENT,
	                        BuildingName nvarchar(100) NULL,
	                        Description nvarchar(100) NULL
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
            this.SetupOrmConfiguration(SqlDialect.PostgreSql);

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
                        CREATE TABLE ""Employee"" (
	                        ""Id"" SERIAL,
                            ""EmployeeId"" uuid NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
	                        ""KeyPass"" uuid NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
	                        ""LastName"" varchar(100) NOT NULL,
	                        ""FirstName"" varchar(100) NOT NULL,
                            ""FullName"" varchar(200) NOT NULL,
	                        ""BirthDate"" timestamp NOT NULL,
                            ""WorkstationId"" int NULL,
	                        PRIMARY KEY (""Id"", ""EmployeeId"")
                        );

                        CREATE OR REPLACE FUNCTION computed_full_name()
                        RETURNS trigger
                        LANGUAGE plpgsql
                        SECURITY DEFINER
                        AS $BODY$
                        DECLARE
                            payload text;
                        BEGIN
                            NEW.""FullName"" = NEW.""FirstName"" || NEW.""LastName"";

                            RETURN NEW;
                        END
                        $BODY$;

                        CREATE TRIGGER ""computed_full_name_trigger""
                        BEFORE INSERT OR UPDATE
                        ON ""Employee""
                        FOR EACH ROW
                        EXECUTE PROCEDURE computed_full_name();

                        CREATE TABLE ""Workstations"" (
	                        ""WorkstationId"" BIGSERIAL,
	                        ""Name"" varchar(100) NOT NULL,
                            ""InventoryIndex"" int NOT NULL,
                            ""AccessLevel"" int NOT NULL DEFAULT 1,
                            ""BuildingId"" int NULL,
                            ""10PinSlots"" int NULL,
	                        PRIMARY KEY (""WorkstationId"")
                        );

                        CREATE TABLE ""Buildings"" (
	                        ""Id"" SERIAL,
	                        ""BuildingName"" varchar(100) NULL,
	                        ""Description"" varchar(100) NULL,
	                        PRIMARY KEY (""Id"")
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
            this.SetupOrmConfiguration(SqlDialect.MySql);

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
	                        Id int NOT NULL AUTO_INCREMENT,
                            EmployeeId CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	                        KeyPass CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
	                        LastName nvarchar(100) NOT NULL,
	                        FirstName nvarchar(100) NOT NULL,
                            FullName nvarchar(200) AS (CONCAT(FirstName,LastName)),
	                        BirthDate datetime NOT NULL,
                            WorkstationId int NULL,
	                        PRIMARY KEY (Id, EmployeeId)
                        );

                        ALTER TABLE `Employee` auto_increment=2;

                        CREATE TRIGGER `Employee_Assign_UUID`
                            BEFORE INSERT ON Employee
                            FOR EACH ROW
                            SET NEW.EmployeeId = UUID(),
                            New.KeyPass = UUID();

                        CREATE TABLE `Workstations` (
	                        WorkstationId bigint NOT NULL AUTO_INCREMENT,
	                        Name nvarchar(100) NOT NULL,
                            InventoryIndex int NOT NULL,
                            AccessLevel int NOT NULL DEFAULT 1,
                            BuildingId int NULL,
                            10PinSlots int NULL,
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


            _testContext.DatabaseConnection = new MySqlConnection(connectionString+$";Database={DatabaseName}");
            _testContext.DatabaseConnection.Open();
        }

        private void SetupLocalDbDatabase()
        {
            var localDbProvider = new SqlLocalDbProvider();
            var localInstance = localDbProvider.CreateInstance(DatabaseName); // instance name = db name, the api will always use the latest version
            localInstance.Start();
            this.SetupMsSqlDatabase(localInstance.CreateConnectionStringBuilder().ConnectionString);
        }

        private void SetupMsSqlDatabase(string connectionString)
        {
            this.SetupOrmConfiguration(SqlDialect.MsSql);

            using (var dataConnection = new SqlConnection(connectionString))
            {
                dataConnection.Open();

                var server = new Server(new ServerConnection(dataConnection));
                var database = new Database(server, DatabaseName);
                try
                {
                    database.Create();
                }
                catch (FailedOperationException ex)
                {
					// sometimes db test files are left behind

					var fileInUseErrorMatch =
						new Regex("Cannot create file '(.*?)' because it already exists.").Match(
							((ex.InnerException as ExecutionFailureException)?.InnerException as SqlException)?.Message ?? string.Empty);
					if (fileInUseErrorMatch.Success)
                    {
                        var dbFilePath = fileInUseErrorMatch.Groups[1].Value;
                        var dbLogFilePath = Path.Combine(Path.GetDirectoryName(dbFilePath), $"{Path.GetFileNameWithoutExtension(dbFilePath)}_log.ldf");

                        if (File.Exists(dbFilePath))
                        {
                            File.Delete(dbFilePath);
                        }
                        if (File.Exists(dbLogFilePath))
                        {
                            File.Delete(dbLogFilePath);
                        }

                        database.Create();
                    }
                    else
                    {
                        throw;
                    }
                }

                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET SINGLE_USER"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET COMPATIBILITY_LEVEL = 110"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET RECOVERY BULK_LOGGED"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET AUTO_CREATE_STATISTICS OFF"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET AUTO_UPDATE_STATISTICS OFF"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} MODIFY FILE(NAME=[{DatabaseName}], SIZE=100MB, FILEGROWTH=10%)"); // for benchmarking purposes 5MB approx 20k records

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
	                    [Name] [nvarchar](100) NULL,
                        [InventoryIndex] [int] NOT NULL,
                        [AccessLevel] [int] NOT NULL DEFAULT(1),
                        [BuildingId] [int] NULL,
                        -- Verify that column names that begin with numbers will work
                        [10PinSlots] [int] NULL,
                        CONSTRAINT [PK_Workstations] PRIMARY KEY CLUSTERED
                        (
	                        [WorkstationId] ASC
                        ))");

                database.ExecuteNonQuery(@"CREATE TABLE [dbo].[Employee](
	                    [Id] [int] IDENTITY(2,1) NOT NULL,
	                    [EmployeeId] [uniqueidentifier] NOT NULL DEFAULT(newid()),
	                    [KeyPass] [uniqueidentifier] NOT NULL DEFAULT(newid()),
	                    [LastName] [nvarchar](100) NOT NULL,
	                    [FirstName] [nvarchar](100) NULL,
	                    [BirthDate] [datetime] NOT NULL,
	                    [WorkstationId] [bigint] NULL,
                        [FullName] AS ([FirstName] + [LastName]),
                        CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED
                        (
	                        [Id] ASC,
	                        [EmployeeId] ASC
                        ),
                        CONSTRAINT [FK_Workstations_Employee] FOREIGN KEY (WorkstationId) REFERENCES [dbo].[Workstations] (WorkstationId))");

                database.ExecuteNonQuery(@"CREATE TABLE [dbo].[Buildings](
	                    [Id] [int] IDENTITY(2,1) NOT NULL,
	                    [BuildingName] [nvarchar](100) NULL,
                        [Description] [nvarchar](100) NULL,
                        CONSTRAINT [PK_Buildings] PRIMARY KEY CLUSTERED
                        (
	                        [Id] ASC
                        ))");

                database.ExecuteNonQuery($@"ALTER DATABASE {DatabaseName} SET MULTI_USER"); // for benchmarking purposes

                // no longer required. local db instances are destroyed and re-created
                //database.ExecuteNonQuery(@"CHECKPOINT;");
                //database.ExecuteNonQuery(@"DBCC DROPCLEANBUFFERS;");
                //database.ExecuteNonQuery(@"DBCC FREESYSTEMCACHE('ALL') WITH NO_INFOMSGS;");
                //database.ExecuteNonQuery(@"DBCC FREESESSIONCACHE WITH NO_INFOMSGS;");
                //database.ExecuteNonQuery(@"DBCC FREEPROCCACHE WITH NO_INFOMSGS;");
            }

            _testContext.DatabaseConnection = new SqlConnection(connectionString+$";Initial Catalog={DatabaseName};"); //Max Pool Size=1; Pooling = False
            _testContext.DatabaseConnection.Open();
        }

        private void CleanupSqlCeDatabase(string connectionString)
        {
            _testContext.DatabaseConnection?.Close();

            var dbFile = Path.Combine(Path.GetDirectoryName(typeof(DatabaseSteps).Assembly.Location), $"{DatabaseName}.sdf");

            if (File.Exists(dbFile))
            {
                File.Delete(dbFile);
            }
        }

        private void CleanupLocalDbDatabase()
        {
            SqlLocalDbApi.AutomaticallyDeleteInstanceFiles = true;
			SqlLocalDbApi.StopOptions = StopInstanceOptions.KillProcess;

			var localDbProvider = new SqlLocalDbProvider();
            var localDbInstanceInfo = localDbProvider.GetInstances().FirstOrDefault(instance => instance.Name==DatabaseName);
            if (localDbInstanceInfo != null)
            {
                var localDbInstance = localDbProvider.GetInstance(DatabaseName);
                if (!localDbInstance.IsRunning)
                {
                    localDbInstance.Start();
                }
				this.CleanupMsSqlDatabase(localDbInstance.CreateConnectionStringBuilder().ConnectionString);
				SqlLocalDbApi.StopInstance(DatabaseName, TimeSpan.FromSeconds(20.0));
				SqlLocalDbApi.DeleteInstance(DatabaseName);
			}
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
                    try
                    {
                        // drop any active connections
                        database.UserAccess = DatabaseUserAccess.Single;
                        database.Alter(TerminationClause.RollbackTransactionsImmediately);
                        database.Refresh();

                        database.UserAccess = DatabaseUserAccess.Multiple;
                        database.Alter(TerminationClause.RollbackTransactionsImmediately);
                        database.Refresh();
                    }
                    catch
                    {
                        // this may fail if the db files are no longer there
                    }

                    database.Drop();
                    //serverManagement.DetachDatabase(MsSqlDatabaseName, false, true);
                }
            }
        }

        private void SetupOrmConfiguration(SqlDialect dialect)
        {
            OrmConfiguration.DefaultDialect = dialect;

            OrmConfiguration.RegisterEntity<Building>()
                .SetTableName("Buildings")
                .SetProperty(building => building.BuildingId,propMapping => propMapping.SetPrimaryKey().SetDatabaseGenerated(DatabaseGeneratedOption.Identity).SetDatabaseColumnName("Id"))
                .SetProperty(building => building.Name, propMapping=> propMapping.SetDatabaseColumnName("BuildingName"))
                .SetProperty(building => building.Description); // test mapping w/o name
        }

        private void CompareQueriedEntitiesWithLocalEntities<TEntity>()
        {
            CollectionAssert.AreEquivalent(_testContext.QueriedEntities.OfType<TEntity>(), _testContext.LocalInsertedEntities.OfType<TEntity>());
        }
    }
}
