namespace Dapper.FastCrud.Benchmarks.Targets.DapperExtensions
{
    using global::Dapper.FastCrud.Benchmarks.Models;
    using global::Dapper.FastCrud.Tests.Contexts;
    using global::DapperExtensions;
    using global::DapperExtensions.Mapper;
    using global::DapperExtensions.Sql;
    using NUnit.Framework;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using TechTalk.SpecFlow;

    [Binding]
    public class DapperExtensionsSteps : EntityGenerationSteps
    {
        private DatabaseTestContext _testContext;

        public DapperExtensionsSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [BeforeScenario]
        public static void TestSetup()
        {
            // clear caches by reinitializing
            var config = new DapperExtensionsConfiguration(typeof(PluralizedAutoClassMapper<>), (IList<Assembly>)new List<Assembly>(), (ISqlDialect)new SqlServerDialect());
            DapperExtensions.Configure(config);
            
            //// UPDATE: in 1.7 we have a blocking bug:
            //// https://github.com/tmsmith/Dapper-Extensions/issues/276
            //// Reverted to 1.6.3
            //DapperExtensions.DefaultMapper = typeof(global::DapperExtensions.Mapper.PluralizedAutoClassMapper<>);
        }


        [When(@"I insert (.*) benchmark entities using Dapper Extensions")]
        public void WhenIInsertSingleIntKeyEntitiesUsingDapperExtensions(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateSimpleBenchmarkEntity(entityIndex);

                generatedEntity.Id = DapperExtensions.Insert(dbConnection, generatedEntity);

                Assert.That(generatedEntity.Id, Is.GreaterThan(1)); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                _testContext.RecordInsertedEntity(generatedEntity);
            }
        }

        [When(@"I select all the benchmark entities using Dapper Extensions (\d+) times")]
        public void WhenISelectAllTheSingleIntKeyEntitiesUsingDapperExtensions(int opCount)
        {
            var dbConnection = _testContext.DatabaseConnection;
            while (--opCount >= 0)
            {
                foreach (var queriedEntity in DapperExtensions.GetList<SimpleBenchmarkEntity>(dbConnection))
                {
                    if (opCount == 0)
                    {
                        _testContext.RecordQueriedEntity(queriedEntity);
                    }
                }
            }
        }

        [When(@"I select all the benchmark entities that I previously inserted using Dapper Extensions")]
        public void WhenISelectAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;
            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                _testContext.RecordQueriedEntity(DapperExtensions.Get<SimpleBenchmarkEntity>(dbConnection, new {entity.Id}));
            }
        }

        [When(@"I update all the benchmark entities that I previously inserted using Dapper Extensions")]
        public void WhenIUpdateAllTheSingleIntKeyEntitiesThatIPreviouslyInsertedUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;

            var entityIndex = 0;
            foreach (var oldInsertedEntity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                var newEntity = this.GenerateSimpleBenchmarkEntity(entityIndex++);
                newEntity.Id = oldInsertedEntity.Id;
                DapperExtensions.Update(dbConnection, newEntity);
                _testContext.RecordUpdatedEntity(newEntity);
            }
        }

        [When(@"I delete all the inserted benchmark entities using Dapper Extensions")]
        public void WhenIDeleteAllTheInsertedSingleIntKeyEntitiesUsingDapperExtensions()
        {
            var dbConnection = _testContext.DatabaseConnection;

            foreach (var entity in _testContext.GetInsertedEntitiesOfType<SimpleBenchmarkEntity>())
            {
                DapperExtensions.Delete(dbConnection, entity);
            }
        }

    }
}
