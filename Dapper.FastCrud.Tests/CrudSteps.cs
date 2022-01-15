namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
        public async Task WhenIInsertBuildingEntities(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<Building>(CreateBuilding, entitiesCount);
            }
            else
            {
                this.InsertEntity<Building>(CreateBuilding, entitiesCount);
            }

            Building CreateBuilding(int index)
            {
                return new Building()
                    {
                        Name = $"Building {Guid.NewGuid().ToString("N")}"
                    };
            }
        }

        [When(@"I insert (\d*) workstation entities using (.*) methods")]
        public async Task WhenIInsertWorkstationEntities(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<Workstation>(CreateWorkstation, entitiesCount);
            }
            else
            {
                this.InsertEntity<Workstation>(CreateWorkstation, entitiesCount);
            }

            Workstation CreateWorkstation(int index)
            {
                return new Workstation()
                    {
                        InventoryIndex = index,
                        Name = $"Workstation {Guid.NewGuid().ToString("N")}",
                        _10PinSlots = index
                    };
            }
        }

        [When(@"I insert (\d*) employee entities using (.*) methods")]
        public async Task WhenIInsertEmployeeEntities(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<Employee>(CreateEmployee, entitiesCount);
            }
            else
            {
                this.InsertEntity<Employee>(CreateEmployee, entitiesCount);
            }

            Employee CreateEmployee(int index)
            {
                return new Employee()
                    {
                        FirstName = $"First Name {Guid.NewGuid().ToString("N")}",
                        LastName = $"Last Name {Guid.NewGuid().ToString("N")}",
                        BirthDate = new DateTime(_rnd.Next(2000, 2010), _rnd.Next(1, 12), _rnd.Next(1, 28), _rnd.Next(0, 23), _rnd.Next(0, 59), _rnd.Next(0, 59)),
                    };
            }
        }

        [When(@"I insert (\d*) employee entities parented to existing workstation entities using (.*) methods")]
        public async Task WhenIInsertEmployeeEntitiesLinkedToWorkstationEntities(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<Employee>(CreateEmployee, entitiesCount);
            }
            else
            {
                this.InsertEntity<Employee>(CreateEmployee, entitiesCount);
            }

            Employee CreateEmployee(int index)
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
            }
        }

        [When(@"I insert (\d*) workstation entities parented to existing building entities using (.*) methods")]
        public async Task WhenIInsertWorkstationEntitiesParentedToExistingBuildingEntitiesUsingSynchronousMethods(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<Workstation>(CreateEntity, entitiesCount);
            }
            else
            {
                this.InsertEntity<Workstation>(CreateEntity, entitiesCount);
            }

            Workstation CreateEntity(int index)
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
            }
        }

        [When(@"I query for all the workstation entities using (.*) methods")]
        public async Task WhenIQueryForAllTheWorkstationEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<Workstation>();
            }
            else
            {
                this.QueryForInsertedEntities<Workstation>();
            }
        }

        [When(@"I query for one workstation entity combined with the employee entities using (.*) methods")]
        public async Task WhenIQueryForOneWorkstationEntityCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            var workstationId = _testContext.LocalInsertedEntities.OfType<Workstation>().Select(workStation => workStation.WorkstationId).Single();
            Workstation queriedEntity;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntity = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.GetAsync<Workstation>(
                               new Workstation()
                                   {
                                       WorkstationId = workstationId
                                   },
                               options => options.Include<Employee>());
                }).ConfigureAwait(false);
            }
            else
            {
                queriedEntity = _testContext.DatabaseConnection.Get<Workstation>(
                    new Workstation()
                        {
                            WorkstationId = workstationId
                        }, options => options.Include<Employee>());

            }
            
            _testContext.QueriedEntities.Add(queriedEntity);
        }

        [When(@"I query for all the workstation entities combined with the employee entities using (.*) methods")]
        public async Task WhenIQueryForAllTheWorkstationEntitiesCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<Workstation> queriedEntities;
            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.FindAsync<Workstation>(options => options.Include<Employee>());
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<Workstation>(options => options.Include<Employee>());
            }

            _testContext.QueriedEntities.AddRange(queriedEntities);
        }

        [When(@"I query for all the employee entities combined with the workstation entities using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesCombinedWithTheWorkstationEntitiesUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<Employee> queriedEntities;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.FindAsync<Employee>(options => options.Include<Workstation>());
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<Employee>(options => options.Include<Workstation>());
            }

            _testContext.QueriedEntities.AddRange(queriedEntities);
        }

        [When(@"I query for all the building entities combined with workstation and employee entities using (.*) methods")]
        public async Task WhenIQueryForAllTheBuildingEntitiesCombinedWithWorkstationAndEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<Building> queriedEntities;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.FindAsync<Building>(options => options
                                                                                                .Include<Workstation>()
                                                                                                .Include<Employee>());
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<Building>(options => options
                                                                                            .Include<Workstation>()
                                                                                            .Include<Employee>());
            }

            _testContext.QueriedEntities.AddRange(queriedEntities);
        }

        [When(@"I query for all the employee entities combined with workstation and building entities using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesCombinedWithWorkstationAndBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<Employee> queriedEntities;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async()=>
                {
                    return await _testContext.DatabaseConnection.FindAsync<Employee>(options => options
                                                                                                .Include<Workstation>()
                                                                                                .Include<Building>());
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<Employee>(options => options
                                                                                            .Include<Workstation>()
                                                                                            .Include<Building>());
            }

            _testContext.QueriedEntities.AddRange(queriedEntities);
        }

        [When(@"I query for all the building entities using (.*) methods")]
        public async Task WhenIQueryForAllTheBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<Building>();
            }
            else
            {
                this.QueryForInsertedEntities<Building>();
            }
        }

        [When(@"I query for all the employee entities using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<Employee>();
            }
            else
            {
                this.QueryForInsertedEntities<Employee>();
            }
        }

        [When(@"I query for the count of all the employee entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryEntityCountAsync<Employee>();
            }
            else
            {
                this.QueryEntityCount<Employee>();
            }
        }

        [When(@"I query for the count of all the building entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryEntityCountAsync<Building>();
            }
            else
            {
                this.QueryEntityCount<Building>();
            }
        }

        [When(@"I query for the count of all the workstation entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheWorkstationEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryEntityCountAsync<Workstation>();
            }
            else
            {
                this.QueryEntityCount<Workstation>();
            }
        }

        [When(@"I query for the count of all the inserted building entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheInsertedBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            FormattableString whereClause = $"charindex(',' + cast({nameof(Building.BuildingId):C} as varchar(10)) + ',', ',' + @BuildingIds + ',') > 0";
            var buildingIds = string.Join(",", _testContext.LocalInsertedEntities.OfType<Building>().Select(building => building.BuildingId));

            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                                          ? await _testContext
                                                .DatabaseConnection
                                                .CountAsync<Building>(statement => statement
                                                    .Where(whereClause)
                                                    .WithParameters(new {BuildingIds = buildingIds}))
                                          : _testContext
                                                .DatabaseConnection
                                                .Count<Building>(statement => statement
                                                    .Where(whereClause)
                                                    .WithParameters(new {BuildingIds = buildingIds}));
        }

        [When(@"I query for the count of all the workstation entities combined with the employee entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheWorkstationEntitiesCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                ? await _testContext.DatabaseConnection.CountAsync<Workstation>(statement => statement
                                                                                    .Include<Employee>())
                : _testContext.DatabaseConnection.Count<Workstation>(statement => statement.Include<Employee>());
        }

        [When(@"I query for the count of all the employee entities strictly linked to workstation and building entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheEmployeeEntitiesStrictlyLinkedToWorkstationAndBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            _testContext.QueriedEntitiesDbCount = useAsyncMethods
                                                      ? await _testContext.DatabaseConnection.CountAsync<Employee>(statement => statement
                                                                                                                                .Include<Workstation>(join => join.InnerJoin())
                                                                                                                                .Include<Building>())
                                                      : _testContext.DatabaseConnection.Count<Employee>(statement => statement
                                                                                                                     .Include<Workstation>(join => join.InnerJoin())
                                                                                                                     .Include<Building>(join => join.InnerJoin()));
        }

        [When(@"I query for the inserted building entities using (.*) methods")]
        public async Task WhenIQueryForTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<Building>();
            }
            else
            {
                this.QueryForInsertedEntities<Building>();
            }
        }

        [When(@"I query for the inserted workstation entities using (.*) methods")]
        public async Task WhenIQueryForTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<Workstation>();
            }
            else
            {
                this.QueryForInsertedEntities<Workstation>();
            }
        }

        [When(@"I query for the inserted employee entities using (.*) methods")]
        public async Task WhenIQueryForTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<Employee>();
            }
            else
            {
                this.QueryForInsertedEntities<Employee>();
            }
        }


        [When(@"I batch update a maximum of (.*) employee entities skipping (.*) and using (.*) methods")]
        public async Task WhenIBatchUpdateAMaximumOfEmployeeEntitiesSkippingAndUsingMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
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
                recordsUpdated = await _testContext.DatabaseConnection.BulkUpdateAsync(updateData, statement => statement.Where(whereCondition));
            }
            else
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkUpdate(updateData, statement=>statement.Where(whereCondition));
            }

            Assert.That(recordsUpdated, Is.EqualTo(entitiesToUpdate.Count()));
        }

        [When(@"I batch update a maximum of (.*) workstation entities skipping (.*) and using (.*) methods")]
        public async Task WhenIBatchUpdateAMaximumOfWorkstationEntitiesSkippingAndUsingMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
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
                recordsUpdated = await _testContext.DatabaseConnection.BulkUpdateAsync(updateData, statement => statement.Where(whereCondition));
            }
            else
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkUpdate(updateData, statement => statement.Where(whereCondition));
            }

            Assert.That(recordsUpdated, Is.EqualTo(entitiesToUpdate.Count()));
        }

        [When(@"I batch delete a maximum of (.*) workstation entities skipping (.*) and using (.*) methods")]
        public async Task WhenIBatchDeleteAMaximumOfWorkstationEntitiesSkippingAndUsingMethods(int? maxCount, int? skipCount, bool useAsyncMethods)
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
                recordsUpdated = await _testContext.DatabaseConnection.BulkDeleteAsync<Workstation>(statement => statement.Where(whereCondition));
            }
            else
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkDelete<Workstation>(statement => statement.Where(whereCondition));
            }

            Assert.That(recordsUpdated, Is.EqualTo(entitiesToDelete.Count()));
        }

        [When(@"I update all the inserted employee entities using (.*) methods")]
        public async Task WhenIUpdateAllTheInsertedEmployeeEntitiesSynchronous(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.UpdateInsertedEntitiesAsync<Employee>(UpdateEmployee);
            }
            else
            {
                this.UpdateInsertedEntities<Employee>(UpdateEmployee);
            }

            Employee UpdateEmployee(Employee originalEmployeeEntity)
            {
                return new Employee()
                    {
                        // all the properties linked to the composite primary key must be used
                        UserId = originalEmployeeEntity.UserId,
                        EmployeeId = originalEmployeeEntity.EmployeeId,
                        BirthDate = new DateTime(2020, 03, 01),
                        WorkstationId = originalEmployeeEntity.WorkstationId,
                        FirstName = "Updated " + originalEmployeeEntity.FirstName,
                        LastName = "Updated " + originalEmployeeEntity.LastName,
                        KeyPass = originalEmployeeEntity.KeyPass,
                    };
            }
        }

        [When(@"I update all the inserted building entities using (.*) methods")]
        public async Task WhenIUpdateAllTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.UpdateInsertedEntitiesAsync<Building>(UpdateBuilding);
            }
            else
            {
                this.UpdateInsertedEntities<Building>(UpdateBuilding);
            }

            Building UpdateBuilding(Building originalBuildingEntity)
            {
                return new Building()
                    {
                        BuildingId = originalBuildingEntity.BuildingId,
                        Name = "Updated " + originalBuildingEntity.Name,
                        Description = "Long text"
                    };
            }
        }

        [When(@"I update all the inserted workstation entities using (.*) methods")]
        public async Task WhenIUpdateAllTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.UpdateInsertedEntitiesAsync<Workstation>(UpdateWorkstation);
            }
            else
            {
                this.UpdateInsertedEntities<Workstation>(UpdateWorkstation);
            }

            Workstation UpdateWorkstation(Workstation originalWorkstationEntity)
            {
                return new Workstation()
                    {
                        WorkstationId = originalWorkstationEntity.WorkstationId,
                        Name = "Updated " + originalWorkstationEntity.Name
                    };
            }
        }

        [When(@"I delete all the inserted workstation entities using (.*) methods")]
        public async Task WhenIDeleteAllTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.DeleteInsertedEntitiesAsync<Workstation>();
            }
            else
            {
                this.DeleteInsertedEntities<Workstation>();
            }
        }

        [When(@"I delete all the inserted employee entities using (.*) methods")]
        public async Task WhenIDeleteAllTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.DeleteInsertedEntitiesAsync<Employee>();
            }
            else
            {
                this.DeleteInsertedEntities<Employee>();
            }
        }

        [When(@"I delete all the inserted building entities using (.*) methods")]
        public async Task WhenIDeleteAllTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.DeleteInsertedEntitiesAsync<Building>();
            }
            else
            {
                this.DeleteInsertedEntities<Building>();
            }
        }

        [Then(@"the database count of the queried entities should be (.*)")]
        public void ThenTheDatabaseCountOfTheQueriedEntitiesShouldBe(int expectedQueryCount)
        {
            Assert.AreEqual(expectedQueryCount, _testContext.QueriedEntitiesDbCount);
        }

        [When(@"I query for a maximum of (.*) workstation entities reverse ordered skipping (.*) records")]
        public void WhenIQueryForAMaximumOfWorkstationEntitiesInReverseOrderOfWorkstationIdSkippingRecords(int? max, int? skip)
        {
            var queriedEntities = _testContext.DatabaseConnection.Find<Workstation>(
                statementOptions =>
                    statementOptions.Where($"{nameof(Workstation.WorkstationId):C} IS NOT NULL")
                                    .OrderBy($"{nameof(Workstation.InventoryIndex):C} DESC")
                                    .Skip(skip)
                                    .Top(max));
            _testContext.QueriedEntities.AddRange(queriedEntities);
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

        private async Task DeleteInsertedEntitiesAsync<TEntity>(int? skipCount = null, int? maxCount = null)
        {
            var entitiesToBeDeleted = _testContext.LocalInsertedEntities
                                                  .OfType<TEntity>()
                                                  .Skip(skipCount ?? 0)
                                                  .Take(maxCount ?? int.MaxValue)
                                                  .ToArray();

            // we have to escape Specflow's bad synchronization context
            await Task.Run(async () =>
            {
                foreach (var entityToBeDeleted in entitiesToBeDeleted)
                {
                    await _testContext.DatabaseConnection.DeleteAsync(entityToBeDeleted);
                    _testContext.LocalInsertedEntities.Remove(entityToBeDeleted);
                }
            });
        }

        private void DeleteInsertedEntities<TEntity>(int? skipCount = null, int? maxCount = null)
        {
            var entitiesToBeDeleted = _testContext.LocalInsertedEntities
                                                  .OfType<TEntity>()
                                                  .Skip(skipCount ?? 0)
                                                  .Take(maxCount ?? int.MaxValue)
                                                  .ToArray();
            foreach (var entityToBeDeleted in entitiesToBeDeleted)
            {
                    _testContext.DatabaseConnection.Delete(entityToBeDeleted);
                    _testContext.LocalInsertedEntities.Remove(entityToBeDeleted);
            }
        }

        private async Task UpdateInsertedEntitiesAsync<TEntity>(Func<TEntity, TEntity> updateFunc, int? skipCount = null, int? maxCount = null)
        {
            var entitiesToUpdate = _testContext.LocalInsertedEntities
                                               .OfType<TEntity>()
                                               .Skip(skipCount ?? 0)
                                               .Take(maxCount ?? int.MaxValue)
                                               .ToArray();

            // we have to escape Specflow's bad synchronization context
            await Task.Run(async () =>
            {
                foreach (var entityToUpdate in entitiesToUpdate)
                {
                    var updatedEntity = updateFunc(entityToUpdate);
                    await _testContext.DatabaseConnection.UpdateAsync(updatedEntity);
                    _testContext.LocalInsertedEntities[_testContext.LocalInsertedEntities.IndexOf(entityToUpdate)] = updatedEntity;
                }
            });
        }

        private void UpdateInsertedEntities<TEntity>(Func<TEntity, TEntity> updateFunc,  int? skipCount = null, int? maxCount = null)
        {
            var entitiesToUpdate = _testContext.LocalInsertedEntities
                                               .OfType<TEntity>()
                                               .Skip(skipCount ?? 0)
                                               .Take(maxCount ?? int.MaxValue)
                                               .ToArray();
            foreach (var entityToUpdate in entitiesToUpdate)
            {
                var updatedEntity = updateFunc(entityToUpdate);
                _testContext.DatabaseConnection.Update((TEntity)updatedEntity);
                _testContext.LocalInsertedEntities[_testContext.LocalInsertedEntities.IndexOf(entityToUpdate)] = updatedEntity;
            }
        }

        private async Task QueryForInsertedEntitiesAsync<TEntity>(int? skipCount = null, int? maxCount = null)
        {
            var entitiesToQuery = _testContext.LocalInsertedEntities
                                              .OfType<TEntity>()
                                              .Skip(skipCount ?? 0)
                                              .Take(maxCount ?? int.MaxValue)
                                              .ToArray();

            // we have to escape Specflow's bad synchronization context
            await Task.Run(async () =>
            {
                foreach (var originalEntity in entitiesToQuery)
                {
                    // this looks a bit stupid, but we want to test the typed queries
                    switch (originalEntity)
                    {
                        case Employee originalEmployeeEntity:
                            var queriedEmployeeEntity = await _testContext.DatabaseConnection.GetAsync(
                                                            new Employee
                                                                {
                                                                    UserId = originalEmployeeEntity.UserId, 
                                                                    EmployeeId = originalEmployeeEntity.EmployeeId
                                                                });
                            _testContext.QueriedEntities.Add(queriedEmployeeEntity);
                            break;
                        case Workstation originalWorkstationEntity:
                            var queriedWorkstationEntity = await _testContext.DatabaseConnection.GetAsync(new Workstation
                                {
                                    WorkstationId = originalWorkstationEntity.WorkstationId
                                });
                            _testContext.QueriedEntities.Add(queriedWorkstationEntity);
                            break;
                        case Building originalBuildingEntity:
                            var queriedBuildingEntity = await _testContext.DatabaseConnection.GetAsync(new Building
                                {
                                    BuildingId = originalBuildingEntity.BuildingId
                                });
                            _testContext.QueriedEntities.Add(queriedBuildingEntity);
                            break;
                        default:
                            throw new NotSupportedException($"Don't know what to do with {typeof(TEntity)}");
                    }
                }
            });
        }

        private void QueryForInsertedEntities<TEntity>(int? skipCount = null, int? maxCount = null)
        {
            var entitiesToQuery = _testContext.LocalInsertedEntities.OfType<TEntity>().Skip(skipCount ?? 0).Take(maxCount ?? int.MaxValue).ToArray();
            foreach (var originalEntity in entitiesToQuery)
            {
                // this looks a bit stupid, but we want to test the typed queries
                switch (originalEntity)
                {
                    case Employee originalEmployeeEntity:
                        var queriedEmployeeEntity = _testContext.DatabaseConnection.Get(
                                                        new Employee
                                                        {
                                                            UserId = originalEmployeeEntity.UserId,
                                                            EmployeeId = originalEmployeeEntity.EmployeeId
                                                        });
                        _testContext.QueriedEntities.Add(queriedEmployeeEntity);
                        break;
                    case Workstation originalWorkstationEntity:
                        var queriedWorkstationEntity = _testContext.DatabaseConnection.Get(new Workstation
                        {
                            WorkstationId = originalWorkstationEntity.WorkstationId
                        });
                        _testContext.QueriedEntities.Add(queriedWorkstationEntity);
                        break;
                    case Building originalBuildingEntity:
                        var queriedBuildingEntity = _testContext.DatabaseConnection.Get(new Building
                        {
                            BuildingId = originalBuildingEntity.BuildingId
                        });
                        _testContext.QueriedEntities.Add(queriedBuildingEntity);
                        break;
                    default:
                        throw new NotSupportedException($"Don't know what to do with {typeof(TEntity)}");
                }
            }
        }

        private async Task InsertEntityAsync<TEntity>(Func<int, TEntity> insertFunc, int entityCount = 1)
        {
            // we have to escape Specflow's bad synchronization context
            await Task.Run(async () =>
            {
                var dbConnection = _testContext.DatabaseConnection;
                for (var entityIndex = 0; entityIndex < entityCount; entityIndex++)
                {
                    object entityToInsert = insertFunc(entityIndex);
                    await dbConnection.InsertAsync<TEntity>((TEntity)entityToInsert);
                    _testContext.LocalInsertedEntities.Add(entityToInsert);
                }

            });
        }

        private void InsertEntity<TEntity>(Func<int, TEntity> insertFunc,  int entityCount = 1)
        {
            var dbConnection = _testContext.DatabaseConnection;
            for(var entityIndex = 0; entityIndex<entityCount; entityIndex++)
            {
                object entityToInsert = insertFunc(entityIndex);
                dbConnection.Insert<TEntity>((TEntity)entityToInsert);
                _testContext.LocalInsertedEntities.Add(entityToInsert);
            }
        }

        private async Task<int> QueryEntityCountAsync<TEntity>()
        {
            // we have to escape Specflow's bad synchronization context
            var recCount = _testContext.QueriedEntitiesDbCount = await Task.Run(async () =>
            {
                return await _testContext.DatabaseConnection.CountAsync<TEntity>();
            });

            return recCount;
        }

        private void QueryEntityCount<TEntity>()
        {
                _testContext.QueriedEntitiesDbCount = _testContext.DatabaseConnection.Count<TEntity>();
        }
    }
}
