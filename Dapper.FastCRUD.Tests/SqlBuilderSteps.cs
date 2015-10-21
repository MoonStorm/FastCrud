namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Linq;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class SqlBuilderSteps
    {
        private IStatementSqlBuilder _currentSqlBuilder;
        private string _rawSqlStatement;
        private string[] _selectColumnNames;
        private SqlDialect _currentDialect;


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
            string startDelimiter, endDelimiter;
            OrmConfiguration.Conventions.GetSqlIdentifierDelimiters(_currentDialect, out startDelimiter, out endDelimiter);
            var expectedSql = string.Join(
                ",",
                _selectColumnNames.Select(colName => $"{startDelimiter}{colName}{endDelimiter}"));
            Assert.That(_rawSqlStatement, Is.EqualTo(expectedSql));
        }

        private void PrepareEnvironment<TEntity>(SqlDialect dialect)
        {
            OrmConfiguration.DefaultDialect = dialect;

            // in real library usage, people will use the ISqlBuilder, but for our tests we're gonna need more than that
            _currentSqlBuilder = OrmConfiguration.GetSqlBuilder<TEntity>() as IStatementSqlBuilder;
            _currentDialect = dialect;
            _selectColumnNames = _currentSqlBuilder.SelectProperties.Select(propInfo => propInfo.DatabaseColumnName).ToArray();
        }
    }
}
