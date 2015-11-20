namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Linq;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CrudSteps : EntityGenerationSteps
    {
        private readonly DatabaseTestContext _testContext;

        public CrudSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
            Assert.NotNull(OrmConfiguration.GetDefaultEntityMapping<Employee>());
        }

        [When(@"I insert (.*) building entities using (.*) methods")]
        public void WhenIInsertBuildingEntities(int entitiesCount, bool makeAsyncCalls)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateBuildingEntity();

                if (makeAsyncCalls)
                {
                    dbConnection.InsertAsync(generatedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    dbConnection.Insert(generatedEntity);
                }

                // the entity already has the associated id set

                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I insert (.*) workstation entities using (.*) methods")]
        public void WhenIInsertWorkstationEntities(int entitiesCount, bool makeAsyncCalls)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateWorkstationEntity();

                if (makeAsyncCalls)
                {
                    dbConnection.InsertAsync(generatedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    dbConnection.Insert(generatedEntity);
                }
                // the entity already has the associated id set

                Assert.Greater(generatedEntity.WorkstationId, 0);
                Assert.Greater(generatedEntity.AccessLevel, 0);

                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I insert (.*) employee entities using (.*) methods")]
        public void WhenIInsertEmployeeEntities(int entitiesCount, bool makeAsyncCalls)
        {
            var dbConnection = _testContext.DatabaseConnection;

            for (var entityIndex = 1; entityIndex <= entitiesCount; entityIndex++)
            {
                var generatedEntity = this.GenerateEmployeeEntity();

                if (makeAsyncCalls)
                {
                    dbConnection.InsertAsync(generatedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    dbConnection.Insert(generatedEntity);
                }
                // the entity already has the associated id set

                Assert.Greater(generatedEntity.UserId, 0);
                Assert.AreNotEqual(generatedEntity.KeyPass, Guid.Empty);

                _testContext.InsertedEntities.Add(generatedEntity);
            }
        }

        [When(@"I query for all the workstation entities using (.*) methods")]
        public void WhenIQueryForAllTheWorkstationEntities(bool useAsyncMethods)
        {
            var entities = useAsyncMethods 
                ? _testContext.DatabaseConnection.GetAsync<Workstation>(commandTimeout: (TimeSpan?)null).Result
                : _testContext.DatabaseConnection.Get<Workstation>(commandTimeout: (TimeSpan?)null);
            _testContext.QueriedEntities.AddRange(entities);
        }

        [When(@"I query for all the building entities using (.*) methods")]
        public void WhenIQueryForAllTheBuildingEntities(bool useAsyncMethods)
        {
            var entities = useAsyncMethods
                ? _testContext.DatabaseConnection.GetAsync<Building>(commandTimeout: (TimeSpan?)null).Result
                : _testContext.DatabaseConnection.Get<Building>(commandTimeout: (TimeSpan?)null);
            _testContext.QueriedEntities.AddRange(entities);
        }

        [When(@"I query for all the employee entities using (.*) methods")]
        public void WhenIQueryForAllTheEmployeeEntities(bool useAsyncMethods)
        {
            var entities = useAsyncMethods
                ? _testContext.DatabaseConnection.GetAsync<Employee>(commandTimeout: (TimeSpan?)null).Result
                : _testContext.DatabaseConnection.Get<Employee>(commandTimeout: (TimeSpan?)null);
            _testContext.QueriedEntities.AddRange(entities);
        }

        [When(@"I query for the count of all the employee entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheEmployeeEntitiesUsingAsynchronous(bool useAsyncMethods)
        {
            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                                                      ? _testContext.DatabaseConnection.CountAsync<Employee>().Result
                                                      : _testContext.DatabaseConnection.Count<Employee>();
        }

        [When(@"I query for the count of all the building entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheBuildingEntitiesUsingAsynchronous(bool useAsyncMethods)
        {
            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                                                      ? _testContext.DatabaseConnection.CountAsync<Building>().Result
                                                      : _testContext.DatabaseConnection.Count<Building>();
        }

        [When(@"I query for the count of all the workstation entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheWorkstationEntitiesUsingAsynchronous(bool useAsyncMethods)
        {
            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                                                      ? _testContext.DatabaseConnection.CountAsync<Workstation>().Result
                                                      : _testContext.DatabaseConnection.Count<Workstation>();
        }

        [When(@"I query for a maximum of (.*) workstation entities reverse ordered skipping (.*) records")]
        public void WhenIQueryForAMaximumOfWorkstationEntitiesInReverseOrderOfWorkstationIdSkippingRecords(int? max, int? skip)
        {
            var sqlBuilder = OrmConfiguration.GetSqlBuilder<Workstation>();
            _testContext.QueriedEntities.AddRange(
                _testContext.DatabaseConnection.Find<Workstation>(
                    whereClause: $"{nameof(Workstation.WorkstationId):C} IS NOT NULL",
                    orderClause: $"{nameof(Workstation.InventoryIndex):C} DESC",
                    skipRowsCount: skip,
                    limitRowsCount: max));
        }

        //[When(@"I query for a maximum of (.*) employee entities ordered by workstation id skipping (.*) records")]
        //public void WhenIQueryForAMaximumOfEmployeeEntitiesInReverseOrderOfWorkstationIdSkippingRecords(int max, int skip)
        //{
        //    _testContext.QueriedEntities.AddRange(
        //        _testContext.DatabaseConnection.Find<Employee>(
        //            whereClause: $"{nameof(Employee.WorkstationId)} IS NOT NULL",
        //            orderClause: $"{nameof(Employee.WorkstationId)} DESC",
        //            skipRowsCount: skip,
        //            limitRowsCount: max));
        //}

        [When(@"I query for the inserted building entities using (.*) methods")]
        public void WhenIQueryForTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Building>())
            {
                var entityQuery = new Building() { BuildingId = insertedEntity.BuildingId };                
                _testContext.QueriedEntities.Add(useAsyncMethods
                    ? _testContext.DatabaseConnection.GetAsync<Building>(entityQuery).Result
                    : _testContext.DatabaseConnection.Get<Building>(entityQuery));
            }
        }

        [When(@"I query for the inserted workstation entities using (.*) methods")]
        public void WhenIQueryForTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Workstation>())
            {
                var entityQuery = new Workstation() { WorkstationId = insertedEntity.WorkstationId };
                _testContext.QueriedEntities.Add(useAsyncMethods
                    ? _testContext.DatabaseConnection.GetAsync<Workstation>(entityQuery).Result
                    :_testContext.DatabaseConnection.Get<Workstation>(entityQuery));
            }
        }

        [When(@"I query for the inserted employee entities using (.*) methods")]
        public void WhenIQueryForTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Employee>())
            {
                var entityQuery = new Employee() { UserId = insertedEntity.UserId, EmployeeId = insertedEntity.EmployeeId };
                _testContext.QueriedEntities.Add(useAsyncMethods
                    ?_testContext.DatabaseConnection.GetAsync<Employee>(entityQuery).Result
                    :_testContext.DatabaseConnection.Get<Employee>(entityQuery));
            }
        }

        [When(@"I partially update all the inserted employee entities")]
        public void WhenIPartiallyUpdateAllTheInsertedEmployeeEntities()
        {
            // prepare a new mapping
            var defaultMapping = OrmConfiguration.GetDefaultEntityMapping<Employee>();
            Assert.IsTrue(defaultMapping.IsFrozen);

            var lastNamePropMapping = defaultMapping.GetProperty(employee => employee.LastName);
            try
            {
                // updates are not possible when the mapping is frozen
                defaultMapping.SetProperty(nameof(Employee.LastName), PropertyMappingOptions.ExcludedFromUpdates);
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            var customMapping = defaultMapping.Clone().UpdatePropertiesExcluding(prop => prop.IsExcludedFromUpdates = true, nameof(Employee.LastName));

            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Employee>())
            {
                var partialUpdatedEntity = new Employee()
                {
                    UserId = insertedEntity.UserId,
                    EmployeeId = insertedEntity.EmployeeId,
                    BirthDate = new DateTime(2020, 03, 01),
                    WorkstationId = 10 + insertedEntity.WorkstationId,
                    FirstName = "Updated " + insertedEntity.FirstName,

                    // all of the above should be excluded with the exception of this one
                    LastName = "Updated " + insertedEntity.LastName
                };
                _testContext.UpdatedEntities.Add(new Employee()
                {
                    UserId = insertedEntity.UserId,
                    KeyPass = insertedEntity.KeyPass,
                    EmployeeId = insertedEntity.EmployeeId,
                    BirthDate = insertedEntity.BirthDate,
                    WorkstationId = insertedEntity.WorkstationId,
                    FirstName = insertedEntity.FirstName,

                    // all of the above should be excluded with the difference of this one
                    LastName = "Updated " + insertedEntity.LastName
                });
                _testContext.DatabaseConnection.Update(partialUpdatedEntity, null, entityMappingOverride: customMapping);
            }

        }

        [When(@"I update all the inserted employee entities using (.*) methods")]
        public void WhenIUpdateAllTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Employee>())
            {
                var updatedEntity = new Employee()
                {
                    // all the properties linked to the composite primary key must be used
                    UserId = insertedEntity.UserId,
                    EmployeeId = insertedEntity.EmployeeId,
                    BirthDate = new DateTime(2020, 03, 01),
                    WorkstationId = 10 + insertedEntity.WorkstationId,
                    FirstName = "Updated " + insertedEntity.FirstName,
                    LastName = "Updated " + insertedEntity.LastName
                };
                _testContext.UpdatedEntities.Add(updatedEntity);
                if (useAsyncMethods)
                {
                    _testContext.DatabaseConnection.UpdateAsync(updatedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    _testContext.DatabaseConnection.Update(updatedEntity);
                }
            }
        }

        [When(@"I update all the inserted building entities using (.*) methods")]
        public void WhenIUpdateAllTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Building>())
            {
                var updatedEntity = new Building()
                {
                    BuildingId = insertedEntity.BuildingId,
                    Name = "Updated " + insertedEntity.Name
                };
                _testContext.UpdatedEntities.Add(updatedEntity);
                if (useAsyncMethods)
                {
                    _testContext.DatabaseConnection.UpdateAsync(updatedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    _testContext.DatabaseConnection.Update(updatedEntity);
                }
            }
        }

        [When(@"I update all the inserted workstation entities using (.*) methods")]
        public void WhenIUpdateAllTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Workstation>())
            {
                var updatedEntity = new Workstation()
                {
                    WorkstationId = insertedEntity.WorkstationId,
                    Name = "Updated " + insertedEntity.Name
                };
                _testContext.UpdatedEntities.Add(updatedEntity);
                if (useAsyncMethods)
                {
                    _testContext.DatabaseConnection.UpdateAsync(updatedEntity).GetAwaiter().GetResult();

                }
                else
                {
                    _testContext.DatabaseConnection.Update(updatedEntity);
                }
            }
        }

        [When(@"I delete all the inserted employee entities using (.*) methods")]
        public void WhenIDeleteAllTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Employee>())
            {
                if (useAsyncMethods)
                {
                    _testContext.DatabaseConnection.DeleteAsync(insertedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    _testContext.DatabaseConnection.Delete(insertedEntity);
                }
            }
        }

        [When(@"I delete all the inserted workstation entities using (.*) methods")]
        public void WhenIDeleteAllTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Workstation>())
            {
                if (useAsyncMethods)
                {
                    _testContext.DatabaseConnection.DeleteAsync(insertedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    _testContext.DatabaseConnection.Delete(insertedEntity);
                }
            }
        }

        [When(@"I delete all the inserted building entities using (.*) methods")]
        public void WhenIDeleteAllTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            foreach (var insertedEntity in _testContext.InsertedEntities.OfType<Building>())
            {
                if (useAsyncMethods)
                {
                    _testContext.DatabaseConnection.DeleteAsync(insertedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    _testContext.DatabaseConnection.Delete(insertedEntity);
                }
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

        [Then(@"the database count of the queried entities should be (.*)")]
        public void ThenTheDatabaseCountOfTheQueriedEntitiesShouldBe(int expectedQueryCount)
        {
            Assert.AreEqual(expectedQueryCount, _testContext.QueriedEntitiesDbCount);
        }

        [StepArgumentTransformation]
        public bool FlagConversion(string input)
        {
            switch (input)
            {
                case "synchronous":
                    return false;
                case "asynchronous":
                    return true;
                default:
                    throw new ArgumentException($"Unknown flag {input}", nameof(input));
            }
        }

        [StepArgumentTransformation()]
        public int? RowCountTransform(string rowCount)
        {
            if (rowCount == "NULL")
                return null;
            return int.Parse(rowCount);
        }
    }
}
