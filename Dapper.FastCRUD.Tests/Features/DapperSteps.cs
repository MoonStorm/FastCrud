namespace Dapper.FastCrud.Tests.Features
{
    using Dapper.FastCrud.Tests.Models;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class DapperSteps:EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public DapperSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) single int key entities using Dapper")]
        public void WhenIInsertSingleIntKeyEntitiesUsingDapper(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;
            var tableName = _testContext.DatabaseConnection.GetTableName<SingleIntPrimaryKeyEntity>();

            for (; entitiesCount > 0; entitiesCount--)
            {
                var generatedEntity = this.GenerateSingleIntPrimaryKeyEntity(entitiesCount);

                dbConnection.Execute(
                    $"INSERT INTO {tableName} ({nameof(SingleIntPrimaryKeyEntity.FirstName)}, {nameof(SingleIntPrimaryKeyEntity.LastName)}, {nameof(SingleIntPrimaryKeyEntity.DateOfBirth)}) VALUES (@FirstName, @LastName, @BirthDate)",
                    new {
                            FirstName = generatedEntity.FirstName,
                            LastName = generatedEntity.LastName,
                            BirthDate = generatedEntity.DateOfBirth
                        });
            }
        }

        [Then(@"I should have (.*) single int key entities in the database")]
        public void ThenIShouldHaveSingleIntKeyEntitiesInTheDatabase(int entitiesCount)
        {

        }
    }
}
