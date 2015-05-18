namespace Dapper.FastCrud.Tests.Features
{
    using System;
    using Dapper.FastCrud.Tests.Models;
    using DeclarativeSql;
    using TechTalk.SpecFlow;
    using DeclarativeSql.Dapper;
    using NUnit.Framework;

    [Binding]
    public class DeclarativeSqlSteps_Invalid : EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public DeclarativeSqlSteps_Invalid(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) benchmark entities using Declarative Sql")]
        public void WhenIInsertSingleIntKeyEntitiesUsingDeclarativeSql(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                // this method will not return the associated identity, which makes the method useless
                //dbConnection.Insert(generatedEntity, setIdentity:false);
            }
        }

    }
}
