namespace Dapper.FastCrud.Tests.Features
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;
    using TechTalk.SpecFlow;

    [Binding]
    public class DatabaseSteps
    {
        private DatabaseTestContext _testContext;

        public DatabaseSteps(DatabaseTestContext testContext)
        {
            this._testContext = testContext;
        }

        [Given(@"I have initialized a MsSql database")]
        public void GivenIHaveInitializedAMsSqlDatabase()
        {
            var originalDatabaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            var deployedDatabaseDirectory = Path.Combine(originalDatabaseDirectory, "Deployed");
            //AppDomain.CurrentDomain.SetData("DataDirectory", databaseDirectory); can be used with |DataDirectory|

            var databaseName = "TestDatabase";

            // cleanup / drop
            var connectionString = $@"Data Source = (LocalDb)\MSSQLLocalDB; AttachDbFilename ={deployedDatabaseDirectory}\{databaseName}.mdf; Initial Catalog = {databaseName}; Integrated Security = True";
            try
            {
                using (var dataConnection = new SqlConnection(connectionString))
                {
                    dataConnection.Open();
                    var serverManagement = new Server(new ServerConnection(dataConnection));
                    serverManagement.Databases[databaseName].Drop();
                }
            }
            catch
            {
                // it's alright, that means the file is not there yet
            }
            finally
            {
                var databaseDataFile = $"{databaseName}.mdf";
                var databaseLogFile = $"{databaseName}_log.ldf";

                Directory.CreateDirectory(deployedDatabaseDirectory);
                File.Copy(Path.Combine(originalDatabaseDirectory, databaseDataFile), Path.Combine(deployedDatabaseDirectory, databaseDataFile), true);
                File.Copy(Path.Combine(originalDatabaseDirectory, databaseLogFile), Path.Combine(deployedDatabaseDirectory, databaseLogFile), true);
            }

            _testContext.DatabaseConnection = new SqlConnection(connectionString);
            _testContext.DatabaseConnection.Open();
        }

        [Given(@"I started the stopwatch")]
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

        [AfterScenario]
        public void Cleanup()
        {            
            if(_testContext.DatabaseConnection!= null)
                _testContext.DatabaseConnection.Dispose();
        }
    }
}
