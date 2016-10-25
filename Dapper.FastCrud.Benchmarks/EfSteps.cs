namespace Dapper.FastCrud.Benchmarks
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Dapper.FastCrud.Tests;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class EfSteps: EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;
        private DbCompiledModel _compiledModel;

        public EfSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
            var dbModelBuilder = new DbModelBuilder();
            dbModelBuilder.Entity<SimpleBenchmarkEntity>();

            var dbModel = dbModelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            _compiledModel = dbModel.Compile();
        }

        [When(@"I insert (.*) benchmark entities using Entity Framework")]
        public void WhenIInsertSingleIntKeyEntitiesUsingEntityFramework(int entitiesCount)
        {
            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);
                using (var dbContext = new EfDbContext(_testContext.DatabaseConnection, _compiledModel, false))
                {
                    dbContext.BenchmarkEntities.Add(generatedEntity);
                    dbContext.SaveChanges();
                }

                Assert.Greater(generatedEntity.Id, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.LocalInsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I select all the benchmark entities using Entity Framework")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingEntityFramework()
        {
            using (var dbContext = new EfDbContext(_testContext.DatabaseConnection, _compiledModel, false))
            {
                _testContext.QueriedEntities.AddRange(dbContext.BenchmarkEntities);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Entity Framework")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingEntityFramework()
        {
            foreach (var entity in _testContext.LocalInsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                using (var dbContext = new EfDbContext(_testContext.DatabaseConnection, _compiledModel, false))
                {
                    dbContext.BenchmarkEntities.Attach(entity);
                    dbContext.BenchmarkEntities.Remove(entity);
                    dbContext.SaveChanges();
                }
            }
        }

        [When(@"I select all the benchmark entities that I previously inserted using Entity Framework")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingEntityFramework()
        {
            foreach (var entity in _testContext.LocalInsertedEntities.OfType<SimpleBenchmarkEntity>())
            {
                using (var dbContext = new EfDbContext(_testContext.DatabaseConnection, _compiledModel, false))
                {
                    _testContext.QueriedEntities.Add(dbContext.BenchmarkEntities.AsNoTracking().Single(queriedEntity => queriedEntity.Id == entity.Id));
                }
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Entity Framework")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingEntityFramework()
        {
            var entityCount = _testContext.LocalInsertedEntities.Count;

            for (var entityIndex = 0; entityIndex < _testContext.LocalInsertedEntities.Count; entityIndex++)
            {
                var oldEntity = _testContext.LocalInsertedEntities[entityIndex] as SimpleBenchmarkEntity;
                var newEntity = new SimpleBenchmarkEntity();
                newEntity.Id = oldEntity.Id;

                using (var dbContext = new EfDbContext(_testContext.DatabaseConnection, _compiledModel, false))
                {
                    dbContext.BenchmarkEntities.Attach(newEntity);
                    this.GenerateSimpleBenchmarkEntity(entityCount++, newEntity);
                    dbContext.SaveChanges();
                }
                _testContext.LocalInsertedEntities[entityIndex] = newEntity;
            }
        }
    }
}
