namespace Dapper.FastCrud.Tests.DatabaseSetup
{
    using System.Data.SqlClient;
    using System.IO;
    using System.Text.RegularExpressions;
    using Dapper.FastCrud.Tests.Contexts;
    using Microsoft.Extensions.Configuration;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class MsSqlDatabaseSteps:CommonDatabaseSetup
    {
        private readonly DatabaseTestContext _testContext;
        private readonly IConfiguration _configuration;

        public MsSqlDatabaseSteps(DatabaseTestContext testContext, IConfiguration configuration)
        {
            _testContext = testContext;
            _configuration = configuration;
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
            this.CleanupLocalDbDatabase();
            this.SetupLocalDbDatabase();

            //var connectionString = this.GetConnectionStringFor("LocalDb");
            //this.CleanupMsSqlDatabase(connectionString);
            //this.SetupMsSqlDatabase(connectionString);
        }

        [Then(@"I cleanup the Benchmark LocalDb database")]
        public void ThenICleanupTheBenchmarkLocalDbDatabase()
        {
            this.CleanupLocalDbDatabase();
        }

        [Then(@"I cleanup the LocalDb database")]
        public void ThenICleanupTheLocalDbDatabase()
        {
            this.CleanupLocalDbDatabase();
            // this.CleanupMsSqlDatabase(ConfigurationManager.ConnectionStrings["LocalDb"].ConnectionString);
        }

        [Given(@"I have initialized a MsSqlServer database")]
        public void GivenIHaveInitializedAnMsSqlServerDatabase()
        {
            var connectionString = this.GetConnectionStringFor(_configuration, "MsSqlServer");
            this.CleanupMsSqlDatabase(connectionString);
            this.SetupMsSqlDatabase(connectionString);
        }

        [Then(@"I cleanup the MsSqlServer database")]
        public void ThenICleanupTheMsSqlServerDatabase()
        {
            var connectionString = this.GetConnectionStringFor(_configuration, "MsSqlServer");
            this.CleanupMsSqlDatabase(connectionString);
        }

        private void SetupLocalDbDatabase()
        {
            // UPDATE: We'll be using the single automatic instance of LocalDb instead
            // The commented code works but for an individual instance:
            //using (var localDbProvider = new SqlLocalDbApi())
            //{
            //    var localInstance = localDbProvider.GetOrCreateInstance(DatabaseName);
            //    var localInstanceManager = localInstance.Manage();
            //    // instance name = db name, the api will always use the latest version
            //    if (!localInstance.IsRunning)
            //    {
            //        localInstanceManager.Start();
            //    }
            //    this.SetupMsSqlDatabase(localInstance.CreateConnectionStringBuilder().ConnectionString);
            //}
            var connectionString = this.GetConnectionStringFor(_configuration, "LocalDb");
            this.SetupMsSqlDatabase($"{connectionString}");
        }

        private void SetupMsSqlDatabase(string connectionString)
        {
            this.SetupOrmConfiguration(SqlDialect.MsSql);

            using (var dataConnection = new SqlConnection(connectionString))
            {
                dataConnection.Open();

                var server = new Server(new ServerConnection(dataConnection));
                var database = new Database(server, _testContext.DatabaseName);
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

                database.ExecuteNonQuery($@"ALTER DATABASE {_testContext.DatabaseName} SET SINGLE_USER"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {_testContext.DatabaseName} SET COMPATIBILITY_LEVEL = 110"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {_testContext.DatabaseName} SET RECOVERY BULK_LOGGED"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {_testContext.DatabaseName} SET AUTO_CREATE_STATISTICS OFF"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {_testContext.DatabaseName} SET AUTO_UPDATE_STATISTICS OFF"); // for benchmarking purposes
                database.ExecuteNonQuery($@"ALTER DATABASE {_testContext.DatabaseName} MODIFY FILE(NAME=[{_testContext.DatabaseName}], SIZE=100MB, FILEGROWTH=10%)"); // for benchmarking purposes 5MB approx 20k records

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
	                    [UserId] [int] IDENTITY(2,1) NOT NULL,
	                    [EmployeeId] [uniqueidentifier] NOT NULL DEFAULT(newid()),
	                    [KeyPass] [uniqueidentifier] NOT NULL DEFAULT(newid()),
	                    [LastName] [nvarchar](100) NOT NULL,
	                    [FirstName] [nvarchar](100) NULL,
	                    [BirthDate] [datetime] NOT NULL,
	                    [WorkstationId] [bigint] NULL,
                        [FullName] AS ([FirstName] + [LastName]),
                        CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED
                        (
	                        [UserId] ASC,
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

                database.ExecuteNonQuery($@"ALTER DATABASE {_testContext.DatabaseName} SET MULTI_USER"); // for benchmarking purposes

                // no longer required. local db instances are destroyed and re-created
                //database.ExecuteNonQuery(@"CHECKPOINT;");
                //database.ExecuteNonQuery(@"DBCC DROPCLEANBUFFERS;");
                //database.ExecuteNonQuery(@"DBCC FREESYSTEMCACHE('ALL') WITH NO_INFOMSGS;");
                //database.ExecuteNonQuery(@"DBCC FREESESSIONCACHE WITH NO_INFOMSGS;");
                //database.ExecuteNonQuery(@"DBCC FREEPROCCACHE WITH NO_INFOMSGS;");
            }

            _testContext.DatabaseConnection = new SqlConnection(connectionString + $";Initial Catalog={_testContext.DatabaseName};"); //Max Pool Size=1; Pooling = False
            _testContext.DatabaseConnection.Open();
        }

        //private void CleanupSqlCeDatabase(string connectionString)
        //{
        //    _testContext.DatabaseConnection?.Close();

        //    var dbFile = Path.Combine(typeof(DatabaseSteps).Assembly.GetDirectory(), $"{DatabaseName}.sdf");

        //    if (File.Exists(dbFile))
        //    {
        //        File.Delete(dbFile);
        //    }
        //}

        private void CleanupLocalDbDatabase()
        {
            //UPDATE: We'll be using the single automatic instance of LocalDb instead
            // The commented code works but for an individual instance:
            //using (var localDbProvider = new SqlLocalDbApi())
            //{
            //    var localInstance = localDbProvider.GetInstanceInfo(DatabaseName);
            //    if (localInstance != null && localInstance.Exists)
            //    {
            //        try
            //        {
            //            if (localInstance.IsRunning)
            //            {
            //                this.CleanupMsSqlDatabase(localInstance.CreateConnectionStringBuilder().ConnectionString);
            //                localDbProvider.StopInstance(DatabaseName);
            //            }
            //        }
            //        catch
            //        {
            //            // this might fail for whatever reason
            //        }
            //        localDbProvider.DeleteInstance(DatabaseName, true);
            //    }
            //}
            var connectionString = this.GetConnectionStringFor(_configuration, "LocalDb");
            this.CleanupMsSqlDatabase($"{connectionString}");
        }

        private void CleanupMsSqlDatabase(string connectionString)
        {
            _testContext.DatabaseConnection?.Close();

            using (var dataConnection = new SqlConnection(connectionString))
            {
                dataConnection.Open();
                var serverManagement = new Server(new ServerConnection(dataConnection));
                var database = serverManagement.Databases[_testContext.DatabaseName];
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

        //[Given(@"I have initialized a SQLCE database")]
        //public void GivenIHaveInitializedSqlCeDatabase()
        //{
        //    var connectionString = this.GetConnectionStringFor("SqlCompact");
        //    this.CleanupSqlCeDatabase(connectionString);
        //    this.SetupSqlCeDatabase(connectionString);
        //}

        //[Then(@"I cleanup the SQLCE database")]
        //public void ThenICleanupTheSqlCeDatabase()
        //{
        //    var connectionString = this.GetConnectionStringFor("SqlCompact");
        //    this.CleanupSqlCeDatabase(connectionString);
        //}

        //private void SetupSqlCeDatabase(string connectionString)
        //{
        //    this.SetupOrmConfiguration(SqlDialect.MsSql);

        //    connectionString = string.Format(
        //        CultureInfo.InvariantCulture,
        //        connectionString,
        //        Path.Combine(typeof(DatabaseSteps).Assembly.GetDirectory(), $"{DatabaseName}.sdf"));

        //    // create the database
        //    using(var sqlCeEngine = new SqlCeEngine(connectionString))
        //    {
        //        sqlCeEngine.CreateDatabase();
        //    }

        //    _testContext.DatabaseConnection = new SqlCeConnection(connectionString);
        //    _testContext.DatabaseConnection.Open();

        //    using(var command = _testContext.DatabaseConnection.CreateCommand())
        //    {
        //        command.CommandText = @"CREATE TABLE [SimpleBenchmarkEntities](
        //             [Id] [int] IDENTITY(2,1) PRIMARY KEY,
        //             [FirstName] [nvarchar](50) NULL,
        //             [LastName] [nvarchar](50) NOT NULL,
        //             [DateOfBirth] [datetime] NULL
        //        )";
        //        command.ExecuteNonQuery();
        //    }
        //}

    }
}