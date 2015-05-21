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

        [When(@"I insert (.*) building entities")]
        public void WhenIInsertBuildingEntities(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateBuildingEntity();

                dbConnection.Insert(generatedEntity, commandTimeout: (TimeSpan?)null);
                // the entity already has the associated id set

                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I insert (.*) workstation entities")]
        public void WhenIInsertWorkstationEntities(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateWorkstationEntity();

                dbConnection.Insert(generatedEntity, commandTimeout: (TimeSpan?)null);
                // the entity already has the associated id set

                Assert.Greater(generatedEntity.WorkstationId, 0);
                Assert.Greater(generatedEntity.AccessLevel, 0);

                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I insert (.*) employee entities")]
        public void WhenIInsertEmployeeEntities(int entitiesCount)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateEmployeeEntity();

                dbConnection.Insert(generatedEntity, commandTimeout: (TimeSpan?)null);
                // the entity already has the associated id set

                Assert.Greater(generatedEntity.UserId, 0);
                Assert.AreNotEqual(generatedEntity.KeyPass, Guid.Empty);

                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I query for all the workstation entities")]
        public void WhenIQueryForAllTheWorkstationEntities()
        {
            _testContext.QueriedEntities.AddRange(_testContext.DatabaseConnection.Get<Workstation>(commandTimeout: (TimeSpan?)null));
        }

        [When(@"I query for all the building entities")]
        public void WhenIQueryForAllTheBuildingEntities()
        {
            _testContext.QueriedEntities.AddRange(_testContext.DatabaseConnection.Get<Building>(commandTimeout: (TimeSpan?)null));
        }

        [When(@"I query for all the employee entities")]
        public void WhenIQueryForAllTheEmployeeEntities()
        {
            _testContext.QueriedEntities.AddRange(_testContext.DatabaseConnection.Get<Employee>(commandTimeout:(TimeSpan?)null));
        }

        [When(@"I query for a maximum of (.*) workstation entities ordered by workstation id skipping (.*) records")]
        public void WhenIQueryForAMaximumOfWorkstationEntitiesInReverseOrderOfWorkstationIdSkippingRecords(int max, int skip)
        {
            _testContext.QueriedEntities.AddRange(
                _testContext.DatabaseConnection.Find<Workstation>(
                    whereClause: $"{nameof(Workstation.WorkstationId)} IS NOT NULL",
                    orderClause: $"{nameof(Workstation.WorkstationId)} DESC",
                    skipRowsCount: skip,
                    limitRowsCount: max));
        }

        [When(@"I query for a maximum of (.*) employee entities ordered by workstation id skipping (.*) records")]
        public void WhenIQueryForAMaximumOfEmployeeEntitiesInReverseOrderOfWorkstationIdSkippingRecords(int max, int skip)
        {
            _testContext.QueriedEntities.AddRange(
                _testContext.DatabaseConnection.Find<Employee>(
                    whereClause: $"{nameof(Employee.WorkstationId)} IS NOT NULL",
                    orderClause: $"{nameof(Employee.WorkstationId)} DESC",
                    skipRowsCount: skip,
                    limitRowsCount: max));
        }

        [When(@"I query for the inserted building entities")]
        public void WhenIQueryForTheInsertedBuildingEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Building>())
            {
                _testContext.QueriedEntities.Add(_testContext.DatabaseConnection.Get<Building>(new Building()
                {
                    BuildingId = insertedEntity.BuildingId
                }));
            }
        }

        [When(@"I query for the inserted workstation entities")]
        public void WhenIQueryForTheInsertedWorkstationEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Workstation>())
            {
                _testContext.QueriedEntities.Add(_testContext.DatabaseConnection.Get<Workstation>(new Workstation()
                {
                    WorkstationId = insertedEntity.WorkstationId
                }));
            }
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
                                            BirthDate = new DateTime(2020,03,01),
                                            WorkstationId = 10 + insertedEntity.WorkstationId,
                                            FirstName = "Updated " + insertedEntity.FirstName,
                                            LastName = "Updated " + insertedEntity.LastName
                                        };
                _testContext.UpdatedEntities.Add(updatedEntity);
                _testContext.DatabaseConnection.Update(updatedEntity);
            }
        }

        [When(@"I update all the inserted building entities")]
        public void WhenIUpdateAllTheInsertedBuildingEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Building>())
            {
                var updatedEntity = new Building()
                {
                    BuildingId = insertedEntity.BuildingId,
                    Name = "Updated " + insertedEntity.Name
                };
                _testContext.UpdatedEntities.Add(updatedEntity);
                _testContext.DatabaseConnection.Update(updatedEntity);
            }
        }

        [When(@"I update all the inserted workstation entities")]
        public void WhenIUpdateAllTheInsertedWorkstationEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Workstation>())
            {
                var updatedEntity = new Workstation()
                {
                    WorkstationId = insertedEntity.WorkstationId,
                    Name = "Updated " + insertedEntity.Name
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

        [When(@"I delete all the inserted workstation entities")]
        public void WhenIDeleteAllTheInsertedWorkstationEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Workstation>())
            {
                _testContext.DatabaseConnection.Delete(insertedEntity, commandTimeout: (TimeSpan?)null);
            }
        }

        [When(@"I delete all the inserted building entities")]
        public void WhenIDeleteAllTheInsertedBuildingEntities()
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Building>())
            {
                _testContext.DatabaseConnection.Delete(insertedEntity, commandTimeout: (TimeSpan?)null);
            }
        }

        [Then(@"I should have (.*) employee entities in the database")]
        public void ThenIShouldHaveEmployeeEntitiesInTheDatabase(int count)
        {
            Assert.AreEqual(count, _testContext.DatabaseConnection.Get<Employee>().Count());
        }

        [Then(@"I should have (.*) workstation entities in the database")]
        public void ThenIShouldHaveWorkstationEntitiesInTheDatabase(int count)
        {
            Assert.AreEqual(count, _testContext.DatabaseConnection.Get<Workstation>().Count());
        }

        [Then(@"I should have (.*) building entities in the database")]
        public void ThenIShouldHaveBuildingEntitiesInTheDatabase(int count)
        {
            Assert.AreEqual(count, _testContext.DatabaseConnection.Get<Building>().Count());
        }

    }
}
