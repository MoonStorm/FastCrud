namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        [When(@"I insert (\d*) building entities using (.*) methods")]
        public void WhenIInsertBuildingEntities(int entitiesCount, bool makeAsyncCalls)
        {
            this.InsertEntity<Building>(makeAsyncCalls, index => new Building()
                                                  {
                                                      Name = $"Building {Guid.NewGuid().ToString("N")}"
                                                  }, entitiesCount);
        }

        [When(@"I insert (\d*) workstation entities using (.*) methods")]
        public void WhenIInsertWorkstationEntities(int entitiesCount, bool makeAsyncCalls)
        {
            this.InsertEntity<Workstation>(makeAsyncCalls, index => new Workstation() {
                InventoryIndex = index,
                Name = $"Workstation {Guid.NewGuid().ToString("N")}",
                _10PinSlots = index
            },
            entitiesCount);
        }

        [When(@"I insert (\d*) employee entities using (.*) methods")]
        public void WhenIInsertEmployeeEntities(int entitiesCount, bool makeAsyncCalls)
        {
            this.InsertEntity<Employee>(makeAsyncCalls, index => new Employee()
                                                  {
                                                      FirstName = $"First Name {Guid.NewGuid().ToString("N")}",
                                                      LastName = $"Last Name {Guid.NewGuid().ToString("N")}",
                                                      BirthDate = new DateTime(_rnd.Next(2000, 2010), _rnd.Next(1, 12), _rnd.Next(1, 28), _rnd.Next(0, 23), _rnd.Next(0, 59), _rnd.Next(0, 59)),
            }, entitiesCount);
        }

        [When(@"I insert (\d*) employee entities parented to existing workstation entities using (.*) methods")]
        public void WhenIInsertEmployeeEntitiesLinkedToWorkstationEntities(int entitiesCount, bool makeAsyncCalls)
        {
            this.InsertEntity<Employee>(makeAsyncCalls, index =>
            {
                var workstation = _testContext.LocalInsertedEntities.OfType<Workstation>().Skip(index).FirstOrDefault();

                // in case we link more employees tan workstations available, use the last one there
                if (workstation == null)
                {
                    workstation = _testContext.LocalInsertedEntities.OfType<Workstation>().Last();
                }

                var employee = new Employee()
                {
                    FirstName = $"First Name {Guid.NewGuid().ToString("N")}",
                    LastName = $"Last Name {Guid.NewGuid().ToString("N")}",
                    BirthDate = new DateTime(_rnd.Next(2000, 2010), _rnd.Next(1, 12), _rnd.Next(1, 28), _rnd.Next(0, 23), _rnd.Next(0, 59), _rnd.Next(0, 59)),

                    // assign a workstation id for the join tests
                    WorkstationId = workstation.WorkstationId,

                    // this is not going to get inserted in the db, but we need it for local comparisons
                    Workstation = workstation
                };

                if (workstation.Employees == null)
                {
                    workstation.Employees = new List<Employee>();
                }

                ((IList<Employee>)workstation.Employees).Add(employee);

                return employee;
            }, entitiesCount);
        }

        [When(@"I insert (\d*) workstation entities parented to existing building entities using (.*) methods")]
        public void WhenIInsertWorkstationEntitiesParentedToExistingBuildingEntitiesUsingSynchronousMethods(int entitiesCount, bool makeAsyncCalls)
        {
            this.InsertEntity<Workstation>(makeAsyncCalls, index =>
            {
                var building = _testContext.LocalInsertedEntities.OfType<Building>().Skip(index).First();
                var workstation = new Workstation()
                    {
                        InventoryIndex = index,
                        Name = $"Workstation {Guid.NewGuid().ToString("N")}",

                        // assign a building id for the join tests
                        BuildingId = building.BuildingId,

                        // this is not going to get inserted in the db, but we need it for local comparisons
                        Building = building
                    };

                if (building.Workstations == null)
                {
                    building.Workstations = new List<Workstation>();
                }

                ((IList<Workstation>)building.Workstations).Add(workstation);

                return workstation;
            }, entitiesCount);
        }

        [When(@"I query for all the workstation entities using (.*) methods")]
        public void WhenIQueryForAllTheWorkstationEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedEntities<Workstation>(useAsyncMethods);
        }

        [When(@"I query for one workstation entity combined with the employee entities using (.*) methods")]
        public void WhenIQueryForOneWorkstationEntityCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            var workstationId = _testContext.LocalInsertedEntities.OfType<Workstation>().Select(workStation => workStation.WorkstationId).Single();
            var queriedEntity = useAsyncMethods
                    ? _testContext.DatabaseConnection.GetAsync<Workstation>(
                        new Workstation() {WorkstationId  = workstationId}, options => options.Include<Employee>()).GetAwaiter().GetResult()
                    : _testContext.DatabaseConnection.Get<Workstation>(
                        new Workstation() { WorkstationId = workstationId} ,options => options.Include<Employee>());

            _testContext.QueriedEntities.Add(queriedEntity);
        }

        [When(@"I query for all the workstation entities combined with the employee entities using (.*) methods")]
        public void WhenIQueryForAllTheWorkstationEntitiesCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            var queriedEntityes = useAsyncMethods
                    ? _testContext.DatabaseConnection.FindAsync<Workstation>(options => options.Include<Employee>()).Result
                    : _testContext.DatabaseConnection.Find<Workstation>(options => options.Include<Employee>());

            _testContext.QueriedEntities.AddRange(queriedEntityes);
        }

        [When(@"I query for all the employee entities combined with the workstation entities using (.*) methods")]
        public void WhenIQueryForAllTheEmployeeEntitiesCombinedWithTheWorkstationEntitiesUsingMethods(bool useAsyncMethods)
        {
            var queriedEntityes = useAsyncMethods
                    ? _testContext.DatabaseConnection.FindAsync<Employee>(options => options.Include<Workstation>()).Result
                    : _testContext.DatabaseConnection.Find<Employee>(options => options.Include<Workstation>());

            _testContext.QueriedEntities.AddRange(queriedEntityes);
        }

        [When(@"I query for all the building entities combined with workstation and employee entities using (.*) methods")]
        public void WhenIQueryForAllTheBuildingEntitiesCombinedWithWorkstationAndEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            var queriedEntityes = useAsyncMethods
                    ? _testContext.DatabaseConnection.FindAsync<Building>(options => options.Include<Workstation>().Include<Employee>()).Result
                    : _testContext.DatabaseConnection.Find<Building>(options => options.Include<Workstation>().Include<Employee>());

            _testContext.QueriedEntities.AddRange(queriedEntityes);
        }

        [When(@"I query for all the employee entities combined with workstation and building entities using (.*) methods")]
        public void WhenIQueryForAllTheEmployeeEntitiesCombinedWithWorkstationAndBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            var queriedEntityes = useAsyncMethods
                    ? _testContext.DatabaseConnection.FindAsync<Employee>(options => options.Include<Workstation>().Include<Building>()).Result
                    : _testContext.DatabaseConnection.Find<Employee>(options => options.Include<Workstation>().Include<Building>());

            _testContext.QueriedEntities.AddRange(queriedEntityes);
        }

        [When(@"I query for all the building entities using (.*) methods")]
        public void WhenIQueryForAllTheBuildingEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedEntities<Building>(useAsyncMethods);
        }

        [When(@"I query for all the employee entities using (.*) methods")]
        public void WhenIQueryForAllTheEmployeeEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedEntities<Employee>(useAsyncMethods);
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
            var buildingIds = string.Join(",", _testContext.LocalInsertedEntities.OfType<Building>().Select(building => building.BuildingId));

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

        [When(@"I query for the count of all the workstation entities combined with the employee entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheWorkstationEntitiesCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                ? _testContext.DatabaseConnection.CountAsync<Workstation>(statement => statement.Include<Employee>()).GetAwaiter().GetResult()
                : _testContext.DatabaseConnection.Count<Workstation>(statement => statement.Include<Employee>());
        }

        [When(@"I query for the count of all the employee entities strictly linked to workstation and building entities using (.*) methods")]
        public void WhenIQueryForTheCountOfAllTheEmployeeEntitiesStrictlyLinkedToWorkstationAndBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                ? _testContext.DatabaseConnection.CountAsync<Employee>(statement => statement.Include<Workstation>(join => join.InnerJoin()).Include<Building>()).GetAwaiter().GetResult()
                : _testContext.DatabaseConnection.Count<Employee>(statement => statement.Include<Workstation>(join=> join.InnerJoin()).Include<Building>(join => join.InnerJoin()));
        }

        [When(@"I query for the inserted building entities using (.*) methods")]
        public void WhenIQueryForTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedEntities<Building>(useAsyncMethods);
        }

        [When(@"I query for the inserted workstation entities using (.*) methods")]
        public void WhenIQueryForTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedEntities<Workstation>(useAsyncMethods);
        }

        [When(@"I query for the inserted employee entities using (.*) methods")]
        public void WhenIQueryForTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            this.QueryForInsertedEntities<Employee>(useAsyncMethods);
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
                                                                                      WorkstationId = originalEmployeeEntity.WorkstationId,
                                                                                      FirstName = "Updated " + originalEmployeeEntity.FirstName,
                                                                                      LastName = "Updated " + originalEmployeeEntity.LastName,
                                                                                      KeyPass = originalEmployeeEntity.KeyPass,
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
            var entitiesToUpdate = _testContext.LocalInsertedEntities.OfType<Employee>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var originalEntity in entitiesToUpdate)
            {
                _testContext.LocalInsertedEntities[_testContext.LocalInsertedEntities.IndexOf(originalEntity)] = new Employee()
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
            var entitiesToUpdate = _testContext.LocalInsertedEntities.OfType<Workstation>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var originalEntity in entitiesToUpdate)
            {
                _testContext.LocalInsertedEntities[_testContext.LocalInsertedEntities.IndexOf(originalEntity)] = new Workstation()
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
            var entitiesToDelete = _testContext.LocalInsertedEntities.OfType<Workstation>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var entityToDelete in entitiesToDelete)
            {
                _testContext.LocalInsertedEntities.Remove(entityToDelete);
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

        [Then(@"the database count of the queried entities should be (.*)")]
        public void ThenTheDatabaseCountOfTheQueriedEntitiesShouldBe(int expectedQueryCount)
        {
            Assert.AreEqual(expectedQueryCount, _testContext.QueriedEntitiesDbCount);
        }

        [When(@"I query for a maximum of (.*) workstation entities reverse ordered skipping (.*) records")]
        public void WhenIQueryForAMaximumOfWorkstationEntitiesInReverseOrderOfWorkstationIdSkippingRecords(int? max, int? skip)
        {
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

            for (var entityIndex = 0; entityIndex < _testContext.LocalInsertedEntities.Count; entityIndex++)
            {
                var insertedEntity = _testContext.LocalInsertedEntities[entityIndex] as Employee;
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

                _testContext.LocalInsertedEntities[entityIndex] = new Employee()
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
            var entitiesToBeDeleted = _testContext.LocalInsertedEntities.OfType<TEntity>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue);
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
            _testContext.LocalInsertedEntities = new List<object>(_testContext.LocalInsertedEntities.Except(entitiesToBeDeleted));
        }

        private void UpdateInsertedEntities<TEntity>(bool useAsyncMethods, Func<TEntity, TEntity> updateFunc,  int? skipCount = null, int? maxCount = null)
        {
            var entitiesToUpdate = _testContext.LocalInsertedEntities.OfType<TEntity>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
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
                _testContext.LocalInsertedEntities[_testContext.LocalInsertedEntities.IndexOf(originalEntity)] = updatedEntity;
            }
        }

        private void QueryForInsertedEntities<TEntity>(bool useAsyncMethods, int? skipCount = null, int? maxCount = null)
        {
            var entitiesToQuery = _testContext.LocalInsertedEntities.OfType<TEntity>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
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

        private void InsertEntity<TEntity>(bool useAsyncMethods, Func<int, TEntity> insertFunc,  int entityCount = 1)
        {
            var dbConnection = _testContext.DatabaseConnection;
            for(var entityIndex = 0; entityIndex<entityCount; entityIndex++)
            {
                object entityToInsert = insertFunc(entityIndex);
                if (useAsyncMethods)
                    dbConnection.InsertAsync<TEntity>((TEntity)entityToInsert).GetAwaiter().GetResult();
                else
                    dbConnection.Insert<TEntity>((TEntity)entityToInsert);

                _testContext.LocalInsertedEntities.Add(entityToInsert);
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
