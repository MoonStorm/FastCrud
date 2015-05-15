namespace Dapper.FastCrud.Tests.Features
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Dapper.FastCrud.Tests.Models;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class DatabaseSteps
    {
        private DatabaseTestContext _testContext;
        private const string MsSqlDatabaseName = "TestDatabase";
        private static string OriginalDatabaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        private static string FinalDatabaseFolder = Path.Combine(OriginalDatabaseFolder, $"FastCrudDatabaseTests");

        private static string LocalDbConnectionString = $@"Data Source = (LocalDb)\v11.0; Initial Catalog = {MsSqlDatabaseName}; Integrated Security = True"; //AttachDbFilename =|DataDirectory|\{MsSqlDatabaseName}.mdf; Data Source = (LocalDb)\MSSQLLocalDB
        private static string MsSqlConnectionString = $@"Data Source=.; Initial Catalog = {MsSqlDatabaseName}; Integrated Security = True";

        public DatabaseSteps(DatabaseTestContext testContext)
        {
            this._testContext = testContext;
        }

        [Given(@"I have initialized a LocalDb database")]
        public void GivenIHaveInitializedLocalDbDatabase()
        {
            CleanupMsSqlDatabase(LocalDbConnectionString);
            SetupMsSqlDatabase(LocalDbConnectionString);
        }

        [Then(@"I cleanup the LocalDb database")]
        public void ThenICleanupTheLocalDbDatabase()
        {
            CleanupMsSqlDatabase(LocalDbConnectionString);
        }

        [Given(@"I have initialized a MsSqlServer database")]
        public void GivenIHaveInitializedAnMsSqlServerDatabase()
        {
            CleanupMsSqlDatabase(MsSqlConnectionString);
            SetupMsSqlDatabase(MsSqlConnectionString);
        }

        [Then(@"I cleanup the MsSqlServer database")]
        public void ThenICleanupTheMsSqlServerDatabase()
        {
            CleanupMsSqlDatabase(MsSqlConnectionString);
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

        [When(@"I report the stopwatch value for (.*)")]
        public void WhenIReportTheStopwatchValueFor(string ormType)
        {
            Trace.WriteLine($"Stopwatch reported: {_testContext.Stopwatch.Elapsed.TotalMilliseconds:0,0.00} milliseconds for {ormType}");
        }

        [Then(@"I should have queried (.*) entities")]
        public void ThenIShouldHaveQueriedEntities(int queryEntitiesCount)
        {
            Assert.AreEqual(queryEntitiesCount, _testContext.QueriedEntities.Count());
        }

        [Then(@"the queried entities should be the same as the ones I inserted")]
        public void ThenTheQueriedEntitiesShouldBeTheSameAsTheOnesIInserted()
        {
            CollectionAssert.AreEquivalent(_testContext.QueriedEntities, _testContext.InsertedEntities);
        }

        [When(@"I clear all the inserted single int key entities")]
        public void WhenIClearAllTheInsertedSingleIntKeyEntities()
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
            _testContext.DatabaseConnection?.Dispose();
        }

        private void SetupMsSqlDatabase(string connectionString)
        {
            using (var dataConnection = new SqlConnection(connectionString.Replace(MsSqlDatabaseName, "master")))
            {
                dataConnection.Open();
                var serverManagement = new Server(new ServerConnection(dataConnection));

                // attach the test dbs
                var dataFiles = new StringCollection();

                var finalSqlDatabaseDataFilePath = Path.Combine(FinalDatabaseFolder, $"{MsSqlDatabaseName}.mdf");
                var finalSqlDatabaseLogFilePath = Path.Combine(FinalDatabaseFolder, $"{MsSqlDatabaseName}_log.ldf");

                Directory.CreateDirectory(FinalDatabaseFolder);
                File.Copy(Path.Combine(OriginalDatabaseFolder, $"{MsSqlDatabaseName}.mdf"), finalSqlDatabaseDataFilePath, true);
                File.Copy(Path.Combine(OriginalDatabaseFolder, $"{MsSqlDatabaseName}_log.ldf"), finalSqlDatabaseLogFilePath, true);

                dataFiles.AddRange(new[] { finalSqlDatabaseDataFilePath, finalSqlDatabaseLogFilePath });
                serverManagement.AttachDatabase(MsSqlDatabaseName, dataFiles);
            }

            _testContext.DatabaseConnection = new SqlConnection(connectionString);
            _testContext.DatabaseConnection.Open();
        }

        private void CleanupMsSqlDatabase(string connectionString)
        {
            _testContext.DatabaseConnection?.Close();

            using (var dataConnection = new SqlConnection(connectionString.Replace(MsSqlDatabaseName, "master")))
            {
                dataConnection.Open();
                var serverManagement = new Server(new ServerConnection(dataConnection));
                var database = serverManagement.Databases[MsSqlDatabaseName];
                if (database != null)
                {
                    // drop any active connections
                    database.UserAccess = DatabaseUserAccess.Single;
                    database.Alter(TerminationClause.RollbackTransactionsImmediately);
                    database.Refresh();

                    database.UserAccess = DatabaseUserAccess.Multiple;
                    database.Alter(TerminationClause.RollbackTransactionsImmediately);
                    database.Refresh();

                    serverManagement.DetachDatabase(MsSqlDatabaseName, false, true);
                }
            }

            if (Directory.Exists(FinalDatabaseFolder))
            {
                Directory.Delete(FinalDatabaseFolder, true);
            }
        }
    }
}
