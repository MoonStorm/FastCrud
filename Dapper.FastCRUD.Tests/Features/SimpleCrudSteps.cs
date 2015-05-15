namespace Dapper.FastCrud.Tests.Features
{
    using TechTalk.SpecFlow;

    [Binding]
    public class SimpleCrudSteps:EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public SimpleCrudSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) single int key entities using Simple Crud")]
        public void WhenIInsertSingleIntKeyEntitiesUsingSimpleCrud(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (; entitiesCount > 0; entitiesCount--)
            {
                var generatedEntity = this.GenerateSingleIntPrimaryKeyEntity(entitiesCount);

                dbConnection.Insert(generatedEntity, commandTimeout: (int?)null);
            }
        }

    }
}
