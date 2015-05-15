namespace Dapper.FastCrud.Tests.Features
{
    using System;
    using TechTalk.SpecFlow;

    [Binding]
    public class FastCrudTests : EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public FastCrudTests(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) single int key entities using Fast Crud")]
        public void WhenIInsertSingleIntKeyEntitiesUsingSimpleCrud(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (; entitiesCount > 0; entitiesCount--)
            {
                var generatedEntity = this.GenerateSingleIntPrimaryKeyEntity(entitiesCount);

                dbConnection.Insert(generatedEntity, commandTimeout: (TimeSpan?)null);
            }
        }

    }
}
