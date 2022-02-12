namespace Dapper.FastCrud.Tests.DatabaseSetup
{
    using Dapper.FastCrud.Tests.Contexts;
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class PostgreSqlDatabaseSteps:CommonDatabaseSetup
    {
        private readonly DatabaseTestContext _testContext;
        private readonly IConfiguration _configuration;

        public PostgreSqlDatabaseSteps(DatabaseTestContext testContext, IConfiguration configuration)
        {
            _testContext = testContext;
            _configuration = configuration;
        }

        [Given(@"I have initialized a PostgreSql database")]
        public void GivenIHaveInitializedPostgreSqlDatabase()
        {
            var connectionString = this.GetConnectionStringFor(_configuration, "PostgreSql");
            this.CleanupPostgreSqlDatabase(connectionString);
            this.SetupPostgreSqlDatabase(connectionString);
        }

        private void CleanupPostgreSqlDatabase(string connectionString)
        {
            using (var dataConnection = new NpgsqlConnection($"{connectionString};Database=postgres"))
            {
                dataConnection.Open();

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText =
                        $@"
                        select pg_terminate_backend(pid)
                        from pg_stat_activity
                        where datname = '{
                            _testContext.DatabaseName.ToLowerInvariant()}'";
                    command.ExecuteNonQuery();
                }

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"DROP DATABASE IF EXISTS { _testContext.DatabaseName.ToLowerInvariant()}";
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SetupPostgreSqlDatabase(string connectionString)
        {
            this.SetupOrmConfiguration(SqlDialect.PostgreSql);

            using (var dataConnection = new NpgsqlConnection($"{connectionString};Database=postgres"))
            {
                dataConnection.Open();

                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"CREATE DATABASE {_testContext.DatabaseName.ToLowerInvariant()}";
                    command.ExecuteNonQuery();
                }
            }

            using (var dataConnection = new NpgsqlConnection(connectionString + $";Database={_testContext.DatabaseName.ToLowerInvariant()}"))
            {
                //  uuid_in((md5((random())::text))::cstring)
                dataConnection.Open();
                using (var command = dataConnection.CreateCommand())
                {
                    command.CommandText = $@"
                        CREATE TABLE ""Employee"" (
	                        ""Id"" SERIAL,
                            ""EmployeeId"" uuid NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
	                        ""KeyPass"" uuid NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
	                        ""LastName"" varchar(100) NOT NULL,
	                        ""FirstName"" varchar(100) NOT NULL,
                            ""FullName"" varchar(200) NOT NULL,
	                        ""BirthDate"" timestamp NOT NULL,
	                        ""RecordIndex"" int NOT NULL,
                            ""WorkstationId"" int NULL,
	                        ""SupervisorUserId"" int NULL,
                            ""SupervisorEmployeeId"" uuid NULL,
	                        ""ManagerUserId"" int NULL,
	                        ""ManagerEmployeeId"" uuid NULL,
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

                        CREATE TABLE ""Badges"" (
	                        ""Id"" int NOT NULL,
                            ""EmployeeId"" uuid NOT NULL,
	                        ""Barcode"" varchar(100) NOT NULL,
	                        PRIMARY KEY (""Id"", ""EmployeeId"")
                        );

                        CREATE TABLE ""Workstations"" (
	                        ""WorkstationId"" BIGSERIAL,
	                        ""Name"" varchar(100) NOT NULL,
                            ""InventoryIndex"" int NOT NULL,
                            ""AccessLevel"" int NOT NULL DEFAULT 1,
                            ""BuildingId"" int NULL,
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

            _testContext.DatabaseConnection = new NpgsqlConnection(connectionString + $";Database={_testContext.DatabaseName.ToLowerInvariant()}");
            _testContext.DatabaseConnection.Open();
        }

    }
}