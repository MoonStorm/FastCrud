namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Tests.Models;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CrudSteps
    {
        private readonly DatabaseTestContext _testContext;
        private readonly Random _rnd = new Random();

        public CrudSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
            Assert.NotNull(OrmConfiguration.GetDefaultEntityMapping<Employee>());
        }

        [When(@"I insert (.*) building entities using (.*) methods")]
        public void WhenIInsertBuildingEntities(int entitiesCount, bool makeAsyncCalls)
        {
            this.InsertEntity<Building>(makeAsyncCalls,
                                        () => new Building()
                                                  {
                                                      Name = $"Building {Guid.NewGuid().ToString("N")}"
                                                  }, entitiesCount);
        }

        [When(@"I insert (.*) workstation entities using (.*) methods")]
        public void WhenIInsertWorkstationEntities(int entitiesCount, bool makeAsyncCalls)
        {
            this.InsertEntity<Workstation>(makeAsyncCalls, () => new Workstation()
            {
                InventoryIndex = (_testContext.LocalEntities.OfType<Workstation>().LastOrDefault()?.InventoryIndex ?? 0) + 1,
                Name = $"Workstation {Guid.NewGuid().ToString("N")}"
            },
            entitiesCount);
        }

        [When(@"I insert (.*) employee entities using (.*) methods")]
        public void WhenIInsertEmployeeEntities(int entitiesCount, bool makeAsyncCalls)
        {
            this.InsertEntity<Employee>(makeAsyncCalls,
                                        () => new Employee()
                                                  {
                                                      FirstName = $"First Name {Guid.NewGuid().ToString("N")}",
                                                      LastName = $"Last Name {Guid.NewGuid().ToString("N")}",
                                                      BirthDate = new DateTime(_rnd.Next(2000, 2010), _rnd.Next(1, 12), _rnd.Next(1, 28), _rnd.Next(0, 23), _rnd.Next(0, 59), _rnd.Next(0, 59))
                                                  }, entitiesCount);
        }
    
        [When(@"I query for all the workstation entities using (.*) methods")]
        public void WhenIQueryForAllTheWorkstationEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedProperties<Workstation>(useAsyncMethods);
        }

        [When(@"I query for all the building entities using (.*) methods")]
        public void WhenIQueryForAllTheBuildingEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedProperties<Building>(useAsyncMethods);
        }

        [When(@"I query for all the employee entities using (.*) methods")]
        public void WhenIQueryForAllTheEmployeeEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedProperties<Employee>(useAsyncMethods);
        }

        [When(@"I query for the count of all the employee entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheEmployeeEntitiesUsingAsynchronous(bool useAsyncMethods)
        {
            this.QueryEntityCount<Employee>(useAsyncMethods);
        }

        [When(@"I query for the count of all the building entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheBuildingEntitiesUsingAsynchronous(bool useAsyncMethods)
        {
            this.QueryEntityCount<Building>(useAsyncMethods);
        }

        [When(@"I query for the count of all the workstation entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheWorkstationEntitiesUsingAsynchronous(bool useAsyncMethods)
        {
            this.QueryEntityCount<Workstation>(useAsyncMethods);
        }

        [When(@"I query for the count of all the inserted building entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheInsertedBuildingEntitiesUsingAsynchronous(bool useAsyncMethods)
        {
            FormattableString whereClause = $"charindex(',' + cast({nameof(Building.BuildingId):C} as varchar(10)) + ',', ',' + @BuildingIds + ',') > 0";
            var buildingIds = string.Join(",", _testContext.LocalEntities.OfType<Building>().Select(building => building.BuildingId)); 

            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                                          ? _testContext
                                                .DatabaseConnection
                                                .CountAsync<Building>(statement => statement
                                                    .Where(whereClause)
                                                    .WithParameters(new {BuildingIds = buildingIds}))
                                                .GetAwaiter()
                                                .GetResult()
                                          : _testContext
                                                .DatabaseConnection
                                                .Count<Building>(statement => statement
                                                    .Where(whereClause)
                                                    .WithParameters(new {BuildingIds = buildingIds}));

        }

        [When(@"I query for the inserted building entities using (.*) methods")]
        public void WhenIQueryForTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedProperties<Building>(useAsyncMethods);
        }

        [When(@"I query for the inserted workstation entities using (.*) methods")]
        public void WhenIQueryForTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedProperties<Workstation>(useAsyncMethods);
        }

        [When(@"I query for the inserted employee entities using (.*) methods")]
        public void WhenIQueryForTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedProperties<Employee>(useAsyncMethods);
        }


        [When(@"I update all the inserted employee entities using (.*) methods")]
        public void WhenIUpdateAllTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            this.UpdateInsertedEntities<Employee>(useAsyncMethods,
                                                  (originalEmployeeEntity) => new Employee()
                                                                                  {
                                                                                      // all the properties linked to the composite primary key must be used
                                                                                      UserId = originalEmployeeEntity.UserId,
                                                                                      EmployeeId = originalEmployeeEntity.EmployeeId,
                                                                                      BirthDate = new DateTime(2020, 03, 01),
                                                                                      WorkstationId = 10 + originalEmployeeEntity.WorkstationId,
                                                                                      FirstName = "Updated " + originalEmployeeEntity.FirstName,
                                                                                      LastName = "Updated " + originalEmployeeEntity.LastName,
                                                                                      KeyPass = originalEmployeeEntity.KeyPass
                                                                                  });
        }

        [When(@"I batch update a maximum of (.*) employee entities skipping (.*) and using (.*) methods")]
        public void WhenIBatchUpdateAMaximumOfEmployeeEntitiesSkippingAndUsingSynchronousMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
        {
            var updateData = new Employee()
            {
                KeyPass = Guid.NewGuid(),
                BirthDate = new DateTime(1951, 09, 30),
                FirstName = "Rest",
                LastName = "In Peace",
            };

            // mimic the behavior on our side
            var entitiesToUpdate = _testContext.LocalEntities.OfType<Employee>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var originalEntity in entitiesToUpdate)
            {
                _testContext.LocalEntities[_testContext.LocalEntities.IndexOf(originalEntity)] = new Employee()
                                                                                                     {
                                                                                                         UserId = originalEntity.UserId,
                                                                                                         EmployeeId =  originalEntity.EmployeeId,
                                                                                                         KeyPass = updateData.KeyPass,
                                                                                                         BirthDate = updateData.BirthDate,
                                                                                                         FirstName = updateData.FirstName,
                                                                                                         LastName = updateData.LastName,
                                                                                                         FullName = updateData.FirstName+updateData.LastName,
                                                                                                     };
            }

            // update the db
            FormattableString whereCondition = $"{nameof(Employee.UserId):C} in ({string.Join(",", entitiesToUpdate.Select(originalEntity => originalEntity.UserId))})";
            int recordsUpdated;
            if (useAsyncMethods)
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkUpdateAsync(updateData, statement => statement.Where(whereCondition)).GetAwaiter().GetResult();
            }
            else
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkUpdate(updateData, statement=>statement.Where(whereCondition));
            }

            Assert.That(recordsUpdated, Is.EqualTo(entitiesToUpdate.Count()));
        }

        [When(@"I batch update a maximum of (.*) workstation entities skipping (.*) and using (.*) methods")]
        public void WhenIBatchUpdateAMaximumOfWorkstationEntitiesSkippingAndUsingSynchronousMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
        {
            var updateData = new Workstation()
            {
                Name = "Batch updated workstation"
            };

            // mimic the behavior on our side
            var entitiesToUpdate = _testContext.LocalEntities.OfType<Workstation>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var originalEntity in entitiesToUpdate)
            {
                _testContext.LocalEntities[_testContext.LocalEntities.IndexOf(originalEntity)] = new Workstation()
                {
                    WorkstationId =  originalEntity.WorkstationId,
                    Name = updateData.Name
                };
            }

            // update the db
            FormattableString whereCondition = $"{nameof(Workstation.WorkstationId):C} in ({string.Join(",", entitiesToUpdate.Select(originalEntity => originalEntity.WorkstationId))})";
            int recordsUpdated;
            if (useAsyncMethods)
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkUpdateAsync(updateData, statement => statement.Where(whereCondition)).GetAwaiter().GetResult();
            }
            else
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkUpdate(updateData, statement => statement.Where(whereCondition));
            }

            Assert.That(recordsUpdated, Is.EqualTo(entitiesToUpdate.Count()));
        }

        [When(@"I batch delete a maximum of (.*) workstation entities skipping (.*) and using (.*) methods")]
        public void WhenIBatchDeleteAMaximumOfWorkstationEntitiesSkippingAndUsingSynchronousMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
        {
            // mimic the behavior on our side
            var entitiesToDelete = _testContext.LocalEntities.OfType<Workstation>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var entityToDelete in entitiesToDelete)
            {
                _testContext.LocalEntities.Remove(entityToDelete);
            }

            // update the db
            FormattableString whereCondition = $"{nameof(Workstation.WorkstationId):C} in ({string.Join(",", entitiesToDelete.Select(originalEntity => originalEntity.WorkstationId))})";
            int recordsUpdated;
            if (useAsyncMethods)
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkDeleteAsync<Workstation>(statement => statement.Where(whereCondition)).GetAwaiter().GetResult();
            }
            else
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkDelete<Workstation>(statement => statement.Where(whereCondition));
            }

            Assert.That(recordsUpdated, Is.EqualTo(entitiesToDelete.Count()));
        }

        [When(@"I update all the inserted building entities using (.*) methods")]
        public void WhenIUpdateAllTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            this.UpdateInsertedEntities<Building>(useAsyncMethods,
                                                  (originalBuildingEntity) => new Building()
                                                                                  {
                                                                                      BuildingId = originalBuildingEntity.BuildingId,
                                                                                      Name = "Updated " + originalBuildingEntity.Name,
                                                                                      Description = "Long text"
                                                                                  });
        }

        [When(@"I update all the inserted workstation entities using (.*) methods")]
        public void WhenIUpdateAllTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            this.UpdateInsertedEntities<Workstation>(useAsyncMethods,
                                                     (originalWorkstationEntity) => new Workstation()
                                                                                        {
                                                                                            WorkstationId = originalWorkstationEntity.WorkstationId,
                                                                                            Name = "Updated " + originalWorkstationEntity.Name
                                                                                        });
        }

        [When(@"I delete all the inserted employee entities using (.*) methods")]
        public void WhenIDeleteAllTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            this.DeleteInsertedEntities<Employee>(useAsyncMethods);
        }

        [When(@"I delete all the inserted workstation entities using (.*) methods")]
        public void WhenIDeleteAllTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            this.DeleteInsertedEntities<Workstation>(useAsyncMethods);
        }

        [When(@"I delete all the inserted building entities using (.*) methods")]
        public void WhenIDeleteAllTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            this.DeleteInsertedEntities<Building>(useAsyncMethods);
        }

        //[When(@"I batch update a maximum of (.*) employee entities skipping (.*) and using (*.) methods")]
        //public void WhenIBatchUpdateAMaximumOfEmployeeEntitiesSkippingAndUsingMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
        //{
        //    this.UpdateInsertedEntities<Employee>(useAsyncMethods, skipCount, maxCount);
        //}

        //[When(@"I batch update a maximum of (.*) workstation entities skipping (.*) and using (*.) methods")]
        //public void WhenIBatchUpdateAMaximumOfWorksationEntitiesSkippingAndUsingMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
        //{
        //    this.UpdateInsertedEntities<Workstation>(useAsyncMethods, skipCount, maxCount);
        //}

        //[When(@"I batch update a maximum of (.*) building entities skipping (.*) and using (*.) methods")]
        //public void WhenIBatchUpdateAMaximumOfBuildingEntitiesSkippingAndUsingMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
        //{
        //    this.UpdateInsertedEntities<Building>(useAsyncMethods, skipCount, maxCount);
        //}

        [Then(@"the database count of the queried entities should be (.*)")]
        public void ThenTheDatabaseCountOfTheQueriedEntitiesShouldBe(int expectedQueryCount)
        {
            Assert.AreEqual(expectedQueryCount, _testContext.QueriedEntitiesDbCount);
        }

        [When(@"I query for a maximum of (.*) workstation entities reverse ordered skipping (.*) records")]
        public void WhenIQueryForAMaximumOfWorkstationEntitiesInReverseOrderOfWorkstationIdSkippingRecords(int? max, int? skip)
        {
            var sqlBuilder = OrmConfiguration.GetSqlBuilder<Workstation>();
            _testContext.QueriedEntities.AddRange(
                _testContext.DatabaseConnection.Find<Workstation>(
                    statementOptions =>
                    statementOptions.Where($"{nameof(Workstation.WorkstationId):C} IS NOT NULL")
                                    .OrderBy($"{nameof(Workstation.InventoryIndex):C} DESC")
                                    .Skip(skip)
                                    .Top(max)));
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
                defaultMapping.SetProperty(employee => employee.LastName, propMapping => propMapping.ExcludeFromUpdates());
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            var customMapping = defaultMapping.Clone().UpdatePropertiesExcluding(prop => prop.IsExcludedFromUpdates = true, nameof(Employee.LastName), nameof(Employee.FullName));

            for (var entityIndex = 0; entityIndex < _testContext.LocalEntities.Count; entityIndex++)
            {
                var insertedEntity = _testContext.LocalEntities[entityIndex] as Employee;
                if (insertedEntity == null)
                    continue;

                var partialUpdatedEntity = new Employee()
                {
                    UserId = insertedEntity.UserId,
                    EmployeeId = insertedEntity.EmployeeId,
                    BirthDate = new DateTime(2020, 03, 01),
                    WorkstationId = 10 + insertedEntity.WorkstationId,
                    FirstName = "Updated " + insertedEntity.FirstName,

                    // all of the above will be ignored with the exception of the next ones
                    LastName = "Updated " + insertedEntity.LastName
                };

                _testContext.DatabaseConnection.Update(partialUpdatedEntity, statement => statement.WithEntityMappingOverride(customMapping));

                _testContext.LocalEntities[entityIndex] = new Employee()
                {
                    UserId = insertedEntity.UserId,
                    KeyPass = insertedEntity.KeyPass,
                    EmployeeId = insertedEntity.EmployeeId,
                    BirthDate = insertedEntity.BirthDate,
                    WorkstationId = insertedEntity.WorkstationId,
                    FirstName = insertedEntity.FirstName,

                    // all of the above were ignored with the exception of the next ones
                    LastName = partialUpdatedEntity.LastName,
                    FullName = partialUpdatedEntity.FullName
                };
            }
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

        private void DeleteInsertedEntities<TEntity>(bool useAsyncMethods, int? skipCount = null, int? maxCount = null)
            where TEntity:class
        {
            var entitiesToBeDeleted = _testContext.LocalEntities.OfType<TEntity>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue);
            foreach (var insertedEntity in entitiesToBeDeleted)
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
            _testContext.LocalEntities = new List<object>(_testContext.LocalEntities.Except(entitiesToBeDeleted));
        }

        private void UpdateInsertedEntities<TEntity>(bool useAsyncMethods, Func<TEntity, TEntity> updateFunc,  int? skipCount = null, int? maxCount = null)
        {
            var entitiesToUpdate = _testContext.LocalEntities.OfType<TEntity>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var originalEntity in entitiesToUpdate)
            {
                object updatedEntity = updateFunc(originalEntity);

                if (useAsyncMethods)
                {
                    _testContext.DatabaseConnection.UpdateAsync((TEntity)updatedEntity).GetAwaiter().GetResult();
                }
                else
                {
                    _testContext.DatabaseConnection.Update((TEntity)updatedEntity);
                }
                _testContext.LocalEntities[_testContext.LocalEntities.IndexOf(originalEntity)] = updatedEntity;
            }
        }

        private void QueryForInsertedProperties<TEntity>(bool useAsyncMethods, int? skipCount = null, int? maxCount = null)
        {
            var entitiesToQuery = _testContext.LocalEntities.OfType<TEntity>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var originalEntity in entitiesToQuery)
            {
                object queriedEntity = null;

                Employee originalEmployeeEntity;
                Workstation originalWorkstationEntity;
                Building originalBuildingEntity;

                if ((originalEmployeeEntity = originalEntity as Employee) != null)
                {
                    queriedEntity = useAsyncMethods
                    ? _testContext.DatabaseConnection.GetAsync(new Employee { UserId = originalEmployeeEntity.UserId, EmployeeId = originalEmployeeEntity.EmployeeId}).Result
                    : _testContext.DatabaseConnection.Get(new Employee { UserId = originalEmployeeEntity.UserId, EmployeeId = originalEmployeeEntity.EmployeeId});
                }
                else if ((originalWorkstationEntity = originalEntity as Workstation) != null)
                {
                    queriedEntity = useAsyncMethods
                    ? _testContext.DatabaseConnection.GetAsync(new Workstation { WorkstationId = originalWorkstationEntity.WorkstationId }).Result
                    : _testContext.DatabaseConnection.Get(new Workstation { WorkstationId = originalWorkstationEntity.WorkstationId });
                }
                else if ((originalBuildingEntity = originalEntity as Building) != null)
                {
                    queriedEntity = useAsyncMethods
                    ? _testContext.DatabaseConnection.GetAsync(new Building {BuildingId = originalBuildingEntity.BuildingId}).Result
                    : _testContext.DatabaseConnection.Get(new Building { BuildingId = originalBuildingEntity.BuildingId });
                }

                _testContext.QueriedEntities.Add(queriedEntity);
            }
        }

        private void InsertEntity<TEntity>(bool useAsyncMethods, Func<TEntity> insertFunc,  int entityCount = 1)
        {
            var dbConnection = _testContext.DatabaseConnection;
            while (entityCount > 0)
            {
                object entityToInsert = insertFunc();
                if (useAsyncMethods)
                    dbConnection.InsertAsync<TEntity>((TEntity)entityToInsert).GetAwaiter().GetResult();
                else
                    dbConnection.Insert<TEntity>((TEntity)entityToInsert);

                _testContext.LocalEntities.Add(entityToInsert);

                entityCount--;
            }
        }

        private void QueryEntityCount<TEntity>(bool useAsyncMethods)
        {
                _testContext.QueriedEntitiesDbCount = useAsyncMethods
                                                          ? _testContext.DatabaseConnection.CountAsync<TEntity>().GetAwaiter().GetResult()
                                                          : _testContext.DatabaseConnection.Count<TEntity>();
        }
    }
}
