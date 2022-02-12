namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Linq;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Tests.Models;
    using Dapper.FastCrud.Tests.Models.CodeFirst;
    using Dapper.FastCrud.Tests.Models.Poco;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class SqlBuilderSteps
    {
        private GenericStatementSqlBuilder _currentSqlBuilder;
        private string _rawSqlStatement;
        private string[] _selectColumnNamesWithDelimiters;
        private SqlDialect _currentDialect;
        private string _buildingRawJoinQueryStatement;


        [Given(@"I extract the SQL builder for LocalDb and workstation")]
        public void GivenIExtractTheSQLBuilderForLocalDbAndWorkstation()
        {
            PrepareEnvironment<WorkstationDbEntity>(SqlDialect.MsSql);
        }

        [Given(@"I extract the SQL builder for PostgreSql and workstation")]
        public void GivenIExtractTheSQLBuilderForPostgreSqlAndWorkstation()
        {
            PrepareEnvironment<WorkstationDbEntity>(SqlDialect.PostgreSql);
        }

        [Given(@"I extract the SQL builder for MySql and workstation")]
        public void GivenIExtractTheSQLBuilderForMySqlAndWorkstation()
        {
            PrepareEnvironment<WorkstationDbEntity>(SqlDialect.MySql);
        }

        [Given(@"I extract the SQL builder for SqLite and workstation")]
        public void GivenIExtractTheSQLBuilderForSqLiteAndWorkstation()
        {
            PrepareEnvironment<WorkstationDbEntity>(SqlDialect.SqLite);
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
                _selectColumnNamesWithDelimiters.Select(colName => $"{colName}"));
            Assert.That(_rawSqlStatement, Is.EqualTo(expectedSql));
        }

        [When(@"I construct a complex join query for workstation using individual identifier resolvers")]
        public void WhenIConstructAComplexJoinQueryForWorkstationUsingIndividualIdentifierResolvers()
        {
            var parameters = new
                {
                    BuildingName = "Belfry Tower"
                };
            //  _currentSqlBuilder.Format no longer supported
            _buildingRawJoinQueryStatement = Sql.Format<WorkstationDbEntity>(
                $@" SELECT {nameof(WorkstationDbEntity):T}.{nameof(WorkstationDbEntity.WorkstationId):C}, {Sql.Table<BuildingDbEntity>()}.{Sql.Column<BuildingDbEntity>(building => building.BuildingId)}
                    FROM {nameof(WorkstationDbEntity):T}, {Sql.Table<BuildingDbEntity>()}
                    WHERE {nameof(WorkstationDbEntity):T}.{nameof(WorkstationDbEntity.BuildingId):C} = {Sql.Table<BuildingDbEntity>():T}.{Sql.Column<BuildingDbEntity>(nameof(BuildingDbEntity.BuildingId))} AND {Sql.Table<BuildingDbEntity>()}.{Sql.Column<BuildingDbEntity>(nameof(BuildingDbEntity.Name))}={Sql.Parameter(nameof(parameters.BuildingName))}" );
        }

        [When(@"I construct a complex join query for workstation using combined table and column identifier resolvers")]
        public void WhenIConstructAComplexJoinQueryForWorkstationUsingCombinedResolvers()
        {
            var parameters = new
                {
                    BuildingName = "Belfry Tower"
                };
            //  _currentSqlBuilder.Format no longer supported
            _buildingRawJoinQueryStatement = Sql.Format<WorkstationDbEntity>(
                $@" SELECT {nameof(WorkstationDbEntity.WorkstationId):TC}, {Sql.TableAndColumn<BuildingDbEntity>(nameof(BuildingDbEntity.BuildingId))}
                    FROM {nameof(WorkstationDbEntity):T}, {Sql.Table<BuildingDbEntity>()}
                    WHERE {nameof(WorkstationDbEntity.BuildingId):TC} = {Sql.TableAndColumn<BuildingDbEntity>(nameof(BuildingDbEntity.BuildingId))} AND {Sql.Table<BuildingDbEntity>()}.{Sql.Column<BuildingDbEntity>(nameof(BuildingDbEntity.Name))}={Sql.Parameter(nameof(parameters.BuildingName))}");
        }

        [Then(@"I should get a valid join query statement for workstation")]
        public void ThenIShouldGetAValidJoinQueryStatementForWorkstation()
        {
            var parameters = new
                {
                    BuildingName = "Belfry Tower"
                };
            var buildingSqlBuilder = OrmConfiguration.GetSqlBuilder<BuildingDbEntity>();
            var expectedQuery =
                $@" SELECT {_currentSqlBuilder.GetTableName()}.{_currentSqlBuilder.GetColumnName(nameof(WorkstationDbEntity.WorkstationId))}, {buildingSqlBuilder.GetTableName()}.{buildingSqlBuilder.GetColumnName(nameof(BuildingDbEntity.BuildingId))}
                    FROM {_currentSqlBuilder.GetTableName()}, {buildingSqlBuilder.GetTableName()}
                    WHERE {_currentSqlBuilder.GetTableName()}.{_currentSqlBuilder.GetColumnName(nameof(WorkstationDbEntity.BuildingId))} = {buildingSqlBuilder.GetTableName()}.{buildingSqlBuilder.GetColumnName(nameof(BuildingDbEntity.BuildingId))} AND {buildingSqlBuilder.GetTableName()}.{buildingSqlBuilder.GetColumnName(nameof(BuildingDbEntity.Name))}=@{nameof(parameters.BuildingName)}";

            Assert.That(_buildingRawJoinQueryStatement, Is.EqualTo(expectedQuery));
        }

        private void PrepareEnvironment<TEntity>(SqlDialect dialect)
        {
            OrmConfiguration.DefaultDialect = dialect;

            // in real library usage, people will use the ISqlBuilder, but for our tests we're gonna need more than that
            _currentSqlBuilder = OrmConfiguration.GetSqlBuilder<TEntity>() as GenericStatementSqlBuilder;
            _currentDialect = dialect;

            var databaseOptions = OrmConfiguration.Conventions.GetDatabaseOptions(_currentDialect);
            _selectColumnNamesWithDelimiters = _currentSqlBuilder.SelectProperties.Select(propInfo =>
            {
                if (propInfo.DatabaseColumnName != propInfo.PropertyName)
                {
                    return $"{databaseOptions.StartDelimiter}{propInfo.DatabaseColumnName}{databaseOptions.EndDelimiter} AS {databaseOptions.StartDelimiter}{propInfo.PropertyName}{databaseOptions.EndDelimiter}";
                }
                
                return $"{databaseOptions.StartDelimiter}{propInfo.PropertyName}{databaseOptions.EndDelimiter}";
            }).ToArray();
        }
    }
}
