namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Data.SqlTypes;
    using System.Linq;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CrudSteps: EntityGenerationSteps
    {
        private readonly DatabaseTestContext _testContext;

        public CrudSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        [When(@"I insert (.*) employee entities")]
        public void WhenIInsertEmployeeEntities(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateEmployeeEntity(entityIndex, entitiesCount - entityIndex);

                dbConnection.Insert(generatedEntity, commandTimeout: (TimeSpan?)null);
                // the entity already has the associated id set

                Assert.Greater(generatedEntity.UserId, 1); // the seed starts from 2 in the db to avoid confusion with the number of rows modified
                Assert.AreNotEqual(generatedEntity.KeyPass, Guid.Empty);

                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I query for all the employee entities")]
        public void WhenIQueryForAllTheEmployeeEntities()
        {
            _testContext.QueriedEntities.AddRange(_testContext.DatabaseConnection.Get<Employee>(commandTimeout:(TimeSpan?)null));
        }

        [When(@"I query for a maximum of (.*) employee entities ordered by workstation id skipping (.*) records")]
        public void WhenIQueryForAMaximumOfEmployeeEntitiesInReverseOrderOfWorkstationIdSkippingRecords(int max, int skip)
        {
            _testContext.QueriedEntities.AddRange(
                _testContext.DatabaseConnection.Find<Employee>(
                    whereClause: $"{nameof(Employee.WorkstationId)} IS NOT NULL",
                    orderClause: $"{nameof(Employee.WorkstationId)}",
                    skipRowsCount: skip,
                    limitRowsCount: max));
        }

        [When(@"I query for the inserted employee entities")]
        public void WhenIQueryForTheInsertedEmployeeEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Employee>())
            {
                _testContext.QueriedEntities.Add(_testContext.DatabaseConnection.Get<Employee>(new Employee()
                                                                                                   {
                                                                                                       UserId = insertedEntity.UserId,
                                                                                                       EmployeeId = insertedEntity.EmployeeId
                                                                                                   }));
            }
        }

        [When(@"I update all the inserted employee entities")]
        public void WhenIUpdateAllTheInsertedEmployeeEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Employee>())
            {
                var updatedEntity = new Employee()
                                        {
                                            // all the properties linked to the composite primary key must be used
                                            UserId = insertedEntity.UserId,
                                            EmployeeId = insertedEntity.EmployeeId,
                                            BirthDate = new SqlDateTime(DateTime.Now).Value,
                                            WorkstationId = 10 + insertedEntity.WorkstationId,
                                            FirstName = "Updated " + insertedEntity.FirstName,
                                            LastName = "Updated " + insertedEntity.LastName
                                        };
                _testContext.UpdatedEntities.Add(updatedEntity);
                _testContext.DatabaseConnection.Update(updatedEntity);
            }
        }

        [When(@"I delete all the inserted employee entities")]
        public void WhenIDeleteAllTheInsertedEmployeeEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Employee>())
            {
                _testContext.DatabaseConnection.Delete(insertedEntity, commandTimeout:(TimeSpan?)null);
            }
        }

        [Then(@"I should have (.*) employee entities in the database")]
        public void ThenIShouldHaveEmployeeEntitiesInTheDatabase(int count)
        {
            Assert.AreEqual(count, _testContext.DatabaseConnection.Get<Employee>().Count());
        }
    }
}
