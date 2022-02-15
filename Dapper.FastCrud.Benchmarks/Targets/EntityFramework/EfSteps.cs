namespace Dapper.FastCrud.Benchmarks.Targets.EntityFramework
{
    using global::Dapper.FastCrud.Benchmarks.Models;
    using global::Dapper.FastCrud.Tests.Contexts;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using System.Linq;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class EfSteps: EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;
        private EfDbContext _dbContext;

        public EfSteps(DatabaseTestContext testContext, EfDbContext dbContext)
        {
            _testContext = testContext;
            _dbContext = dbContext;
        }

        [AfterScenario()]
        public void Cleanup()
        {
            if (_dbContext.IsValueCreated)
            {
                _dbContext.Value.Dispose();
            }
        }

        [When(@"I insert (.*) benchmark entities using Entity Framework")]
        public void WhenIInsertSingleIntKeyEntitiesUsingEntityFramework(int entitiesCount)
        {
            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                _dbContext.Value.BenchmarkEntities.Add(generatedEntity);
                _dbContext.Value.SaveChanges();

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.RecordInsertedEntity(generatedEntity);
            }
        }

        [When(@"I select all the benchmark entities using Entity Framework")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingEntityFramework()
        {
            foreach (var queriedEntity in _dbContext.Value.BenchmarkEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Entity Framework")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingEntityFramework()
        {
            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                _dbContext.Value.BenchmarkEntities.Attach(entity);
                _dbContext.Value.BenchmarkEntities.Remove(entity);
                _dbContext.Value.SaveChanges();
            }
        }

        [When(@"I select all the benchmark entities that I previously inserted using Entity Framework")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingEntityFramework()
        {
            var entityIndex = 0;
            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                _testContext.RecordQueriedEntity(_dbContext.Value.BenchmarkEntities.AsNoTracking().Single(queriedEntity => queriedEntity.Id == entity.Id));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Entity Framework")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingEntityFramework()
        {
            var entityIndex = 0;
            foreach (var oldEntity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {

                var newEntity = new SimpleBenchmarkEntity()
                {
                    Id = oldEntity.Id, 
                    FirstName = oldEntity.FirstName, 
                    LastName = oldEntity.LastName, 
                    DateOfBirth = oldEntity.DateOfBirth
                };

                _dbContext.Value.BenchmarkEntities.Attach(newEntity);

                // now update it
                this.GenerateSimpleBenchmarkEntity(entityIndex++, newEntity);
                _dbContext.Value.SaveChanges();

                _testContext.RecordUpdatedEntity(newEntity);
            }
        }
    }
}
