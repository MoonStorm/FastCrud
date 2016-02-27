namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class SqlBuilderSteps
    {
        private GenericStatementSqlBuilder _currentSqlBuilder;
        private string _rawSqlStatement;
        private string[] _selectColumnNames;
        private SqlDialect _currentDialect;
        private string _buildingRawJoinQueryStatement;


        [Given(@"I extract the SQL builder for LocalDb and workstation")]
        public void GivenIExtractTheSQLBuilderForLocalDbAndWorkstation()
        {
            PrepareEnvironment<Workstation>(SqlDialect.MsSql);
        }

        [Given(@"I extract the SQL builder for PostgreSql and workstation")]
        public void GivenIExtractTheSQLBuilderForPostgreSqlAndWorkstation()
        {
            PrepareEnvironment<Workstation>(SqlDialect.PostgreSql);
        }

        [Given(@"I extract the SQL builder for MySql and workstation")]
        public void GivenIExtractTheSQLBuilderForMySqlAndWorkstation()
        {
            PrepareEnvironment<Workstation>(SqlDialect.MySql);
        }

        [Given(@"I extract the SQL builder for SqLite and workstation")]
        public void GivenIExtractTheSQLBuilderForSqLiteAndWorkstation()
        {
            PrepareEnvironment<Workstation>(SqlDialect.SqLite);
        }

        [When(@"I construct the select column enumeration")]
        public void WhenIConstructTheSelectColumnEnumeration()
        {
            _rawSqlStatement = _currentSqlBuilder.ConstructColumnEnumerationForSelect();
        }

        [Then(@"I should get a valid select column enumeration")]
        public void ThenIShouldGetAValidSelectColumnEnumeration()
        {
            var databaseOptions = OrmConfiguration.Conventions.GetDatabaseOptions(_currentDialect);
            var expectedSql = string.Join(
                ",",
                _selectColumnNames.Select(colName => $"{databaseOptions.StartDelimiter}{colName}{databaseOptions.EndDelimiter}"));
            Assert.That(_rawSqlStatement, Is.EqualTo(expectedSql));
        }

        [When(@"I construct a complex join query for workstation using individual identifier resolvers")]
        public void WhenIConstructAComplexJoinQueryForWorkstationUsingIndividualIdentifierResolvers()
        {
            _buildingRawJoinQueryStatement = _currentSqlBuilder.Format(
                $@" SELECT {nameof(Workstation):T}.{nameof(Workstation.WorkstationId):C}, {Sql.Table<Building>()}.{Sql.Column<Building>(nameof(Building.BuildingId))}
                    FROM {nameof(Workstation):T}, {Sql.Table<Building>()}
                    WHERE {nameof(Workstation):T}.{nameof(Workstation.BuildingId):C} = {Sql.Table<Building>()}.{Sql.Column<Building>(nameof(Building.BuildingId))}" );
        }

        [When(@"I construct a complex join query for workstation using combined table and column identifier resolvers")]
        public void WhenIConstructAComplexJoinQueryForWorkstationUsingCombinedResolvers()
        {
            _buildingRawJoinQueryStatement = _currentSqlBuilder.Format(
                $@" SELECT {nameof(Workstation.WorkstationId):TC}, {Sql.TableAndColumn<Building>(nameof(Building.BuildingId))}
                    FROM {nameof(Workstation):T}, {Sql.Table<Building>()}
                    WHERE {nameof(Workstation.BuildingId):TC} = {Sql.TableAndColumn<Building>(nameof(Building.BuildingId))}");
        }

        [Then(@"I should get a valid join query statement for workstation")]
        public void ThenIShouldGetAValidJoinQueryStatementForWorkstation()
        {
            var buildingSqlBuilder = OrmConfiguration.GetSqlBuilder<Building>();
            var expectedQuery =
                $@" SELECT {_currentSqlBuilder.GetTableName()}.{_currentSqlBuilder.GetColumnName(nameof(Workstation.WorkstationId))}, {buildingSqlBuilder.GetTableName()}.{buildingSqlBuilder.GetColumnName(nameof(Building.BuildingId))}
                    FROM {_currentSqlBuilder.GetTableName()}, {buildingSqlBuilder.GetTableName()}
                    WHERE {_currentSqlBuilder.GetTableName()}.{_currentSqlBuilder.GetColumnName(nameof(Workstation.BuildingId))} = {buildingSqlBuilder.GetTableName()}.{buildingSqlBuilder.GetColumnName(nameof(Building.BuildingId))}";

            Assert.AreEqual(expectedQuery, _buildingRawJoinQueryStatement);
        }

        private void PrepareEnvironment<TEntity>(SqlDialect dialect)
        {
            OrmConfiguration.DefaultDialect = dialect;

            // in real library usage, people will use the ISqlBuilder, but for our tests we're gonna need more than that
            _currentSqlBuilder = OrmConfiguration.GetSqlBuilder<TEntity>() as GenericStatementSqlBuilder;
            _currentDialect = dialect;
            _selectColumnNames = _currentSqlBuilder.SelectProperties.Select(propInfo => propInfo.DatabaseColumnName).ToArray();
        }
    }
}
