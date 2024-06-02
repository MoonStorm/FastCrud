namespace Dapper.FastCrud.Tests
{
    using Dapper.FastCrud.Tests.Models.Metadata;
    using NUnit.Framework;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Dapper.FastCrud.Tests.Models.CodeFirst;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class SqlFormatterSteps
    {
        [Then(@"I should be able to construct a simple manual query for the dialect MsSql using the FastCrud SQL formatter")]
        public void ThenIShouldBeAbleToConstructASimpleManualQueryForTheDialectMsSqlUsingTheSQLFormatter()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.MsSql;
            var searchParams = new
                {
                    FullName = "John Doe"
                };
            var manualSqlQuery = Sql.Format<EmployeeDbEntity>(@$"
                SELECT
                    {nameof(EmployeeDbEntity.BirthDate):C},
                    {nameof(EmployeeDbEntity.HireDate):C}
                FROM
                    {nameof(EmployeeDbEntity):T}
                WHERE
                    {nameof(EmployeeDbEntity.FullName):C} = {nameof(searchParams.FullName):P}
            ");

            var expectedSqlQuery = @"
                SELECT
                    [BirthDate],
                    [HiringDate]
                FROM
                    [Employee]
                WHERE
                    [FullName] = @FullName
            ";
            Assert.That(manualSqlQuery, Is.EqualTo(expectedSqlQuery));
        }

        [Then(@"I should be able to construct a simple manual query for the dialect PostgreSql using the FastCrud SQL formatter")]
        public void ThenIShouldBeAbleToConstructASimpleManualQueryForTheDialectPostgreSqlUsingTheFastCrudSQLFormatter()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.PostgreSql;
            var searchParams = new
                {
                    FullName = "John Doe"
                };
            var manualSqlQuery = Sql.Format<EmployeeDbEntity>(@$"
                SELECT
                    {nameof(EmployeeDbEntity.BirthDate):C},
                    {nameof(EmployeeDbEntity.HireDate):C}
                FROM
                    {nameof(EmployeeDbEntity):T}
                WHERE
                    {nameof(EmployeeDbEntity.FullName):C} = {nameof(searchParams.FullName):P}
            ");

            var expectedSqlQuery = @"
                SELECT
                    ""BirthDate"",
                    ""HiringDate""
                FROM
                    ""Employee""
                WHERE
                    ""FullName"" = @FullName
            ";
            Assert.That(manualSqlQuery, Is.EqualTo(expectedSqlQuery));
        }

        [Then(@"I should be able to construct a manual query involving multiple entities for the dialect MsSql using the FastCrud SQL formatter")]
        public void ThenIShouldBeAbleToConstructAManualQueryInvolvingMultipleEntitiesForTheDialectMsSqlUsingTheFastCrudSQLFormatter()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.MsSql;
            var searchParams = new
                {
                    FullName = "John Doe"
                };
            // when working with multiple entities, a context set on a single entity is not going to be enough
            // note that when using the FastCrud's formatter with an alias, TC is redundant 
            var manualQuery = Sql.Format<EmployeeDbEntity>($@"
                SELECT
                    {Sql.Entity<EmployeeDbEntity>(em => em.HireDate, "em")},
                    {Sql.Entity<EmployeeDbEntity>(em => em.ShiftStartingTime, "em")},
                    {Sql.Entity<WorkstationDbEntity>(ws => ws.WorkstationId, "ws")}
                FROM
                    {Sql.Entity<EmployeeDbEntity>():T} AS {Sql.Identifier("em")}
                    INNER JOIN {Sql.Entity<WorkstationDbEntity>():T} AS {Sql.Identifier("ws")}
                        ON {Sql.Entity<WorkstationDbEntity>(ws => ws.WorkstationId, "ws")} = {Sql.Entity<EmployeeDbEntity>(em => em.WorkstationId, "em")}
                WHERE
                    {Sql.Entity<EmployeeDbEntity>(em => em.FullName, "em")} = {Sql.Parameter(nameof(searchParams.FullName))}
            ");
            var expectedSqlQuery = @"
                SELECT
                    [em].[HiringDate],
                    [em].[ShiftStartingTime],
                    [ws].[WorkstationId]
                FROM
                    [Employee] AS [em]
                    INNER JOIN [Workstations] AS [ws]
                        ON [ws].[WorkstationId] = [em].[WorkstationId]
                WHERE
                    [em].[FullName] = @FullName
            ";
            Assert.That(manualQuery, Is.EqualTo(expectedSqlQuery));
        }

        [Then(@"I should be able to construct a manual query involving multiple entities for the dialect PostgreSql using the FastCrud SQL formatter")]
        public void ThenIShouldBeAbleToConstructAManualQueryInvolvingMultipleEntitiesForTheDialectPostgreSqlUsingTheFastCrudSQLFormatter()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.PostgreSql;
            var searchParams = new
            {
                FullName = "John Doe"
            };
            // when working with multiple entities, a context set on a single entity is not going to be enough
            // note that when using the FastCrud's formatter with an alias, TC is redundant 
            var manualQuery = Sql.Format<EmployeeDbEntity>($@"
                SELECT
                    {Sql.Entity<EmployeeDbEntity>(em => em.HireDate, "em")},
                    {Sql.Entity<EmployeeDbEntity>(em => em.ShiftStartingTime, "em")},
                    {Sql.Entity<WorkstationDbEntity>(ws => ws.WorkstationId, "ws")}
                FROM
                    {Sql.Entity<EmployeeDbEntity>():T} AS {Sql.Identifier("em")}
                    INNER JOIN {Sql.Entity<WorkstationDbEntity>()} AS {Sql.Identifier("ws")}
                        ON {Sql.Entity<WorkstationDbEntity>(ws => ws.WorkstationId, "ws")} = {Sql.Entity<EmployeeDbEntity>(em => em.WorkstationId, "em")}
                WHERE
                    {Sql.Entity<EmployeeDbEntity>(em => em.FullName, "em")} = {Sql.Parameter(nameof(searchParams.FullName))}
            ");
            var expectedSqlQuery = @"
                SELECT
                    ""em"".""HiringDate"",
                    ""em"".""ShiftStartingTime"",
                    ""ws"".""WorkstationId""
                FROM
                    ""Employee"" AS ""em""
                    INNER JOIN ""Workstations"" AS ""ws""
                        ON ""ws"".""WorkstationId"" = ""em"".""WorkstationId""
                WHERE
                    ""em"".""FullName"" = @FullName
            ";
            Assert.That(manualQuery, Is.EqualTo(expectedSqlQuery));
        }

        [Then(@"I should be able to construct a manual query involving multiple entities using the standard formatter")]
        public void ThenIShouldBeAbleToConstructAManualQueryInvolvingMultipleEntitiesUsingTheStandardFormatter()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.MsSql;
            var searchParams = new
                {
                    FullName = "John Doe"
                };
            var manualQuery = FormattableString.Invariant($@"
                SELECT
                    em.{Sql.Entity<EmployeeDbEntity>(em => em.HireDate)},
                    em.{Sql.Entity<EmployeeDbEntity>(em => em.ShiftStartingTime)},
                    ws.{Sql.Entity<WorkstationDbEntity>(ws => ws.WorkstationId)}
                FROM
                    {Sql.Entity<EmployeeDbEntity>():T} AS em
                    INNER JOIN {Sql.Entity<WorkstationDbEntity>()} AS ws
                        ON ws.{Sql.Entity<WorkstationDbEntity>(ws => ws.WorkstationId)} = em.{Sql.Entity<EmployeeDbEntity>(em => em.WorkstationId)}
                WHERE
                    em.{Sql.Entity<EmployeeDbEntity>(em => em.FullName)} = @{Sql.Parameter(nameof(searchParams.FullName))}
            ");
            var expectedSqlQuery = @"
                SELECT
                    em.HiringDate,
                    em.ShiftStartingTime,
                    ws.WorkstationId
                FROM
                    [Employee] AS em
                    INNER JOIN Workstations AS ws
                        ON ws.WorkstationId = em.WorkstationId
                WHERE
                    em.FullName = @FullName
            ";
            Assert.That(manualQuery, Is.EqualTo(expectedSqlQuery));
        }

        [Then(@"I should be able to construct a manual query involving schema qualified code first entities for the dialect MsSql using the standard formatter")]
        public void ThenIShouldBeAbleToConstructAManualQueryInvolvingSchemaQualifiedCodeFirstEntitiesForTheDialectMsSqlUsingTheStandardFormatter()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.MsSql;
            var manualQuery = Sql.Format<TestSchemaQualifiedFormatterEntity>($@"
                SELECT
                    {nameof(TestSchemaQualifiedFormatterEntity.Name):C},
                    {nameof(TestSchemaQualifiedFormatterEntity.EntityId):TC},
                    {Sql.Entity<TestSchemaQualifiedFormatterEntity>(e=>e.ParentEntityId):C},
                    {Sql.Entity<TestSchemaQualifiedFormatterEntity>(e => e.Quantity):TC}
                FROM
                    {nameof(TestSchemaQualifiedFormatterEntity):T},
                    {Sql.Entity<TestSchemaQualifiedFormatterEntity>():T} AS {Sql.Identifier("parent")}
            ");
            var expectedSqlQuery = @"
                SELECT
                    [Name],
                    [TestTable].[Id],
                    [ParentId],
                    [TestTable].[Quantity]
                FROM
                    [dbo].[TestTable],
                    [dbo].[TestTable] AS [parent]
            ";
            Assert.That(manualQuery, Is.EqualTo(expectedSqlQuery));
        }

        [Then(@"I should be able to construct a manual query involving schema qualified code first entities for the dialect PostgreSql using the standard formatter")]
        public void ThenIShouldBeAbleToConstructAManualQueryInvolvingSchemaQualifiedCodeFirstEntitiesForTheDialectPostgreSqlUsingTheStandardFormatter()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.PostgreSql;
            var manualQuery = Sql.Format<TestSchemaQualifiedFormatterEntity>($@"
                SELECT
                    {nameof(TestSchemaQualifiedFormatterEntity.Name):C},
                    {nameof(TestSchemaQualifiedFormatterEntity.EntityId):TC},
                    {Sql.Entity<TestSchemaQualifiedFormatterEntity>(e => e.ParentEntityId):C},
                    {Sql.Entity<TestSchemaQualifiedFormatterEntity>(e => e.Quantity):TC}
                FROM
                    {nameof(TestSchemaQualifiedFormatterEntity):T},
                    {Sql.Entity<TestSchemaQualifiedFormatterEntity>():T} AS {Sql.Identifier("parent")}
            ");
            var expectedSqlQuery = @"
                SELECT
                    ""Name"",
                    ""TestTable"".""Id"",
                    ""ParentId"",
                    ""TestTable"".""Quantity""
                FROM
                    ""dbo"".""TestTable"",
                    ""dbo"".""TestTable"" AS ""parent""
            ";
            Assert.That(manualQuery, Is.EqualTo(expectedSqlQuery));
        }

        [Table("TestTable", Schema = "dbo")]
        private record TestSchemaQualifiedFormatterEntity
        {
            [Column("Id")]
            public int EntityId { get; set; }

            [Column("ParentId")]
            public int ParentEntityId { get; set; }

            public string Name { get; set; }

            public int Quantity { get; set; }
        }
    }
}
