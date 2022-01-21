namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Tests.Common;
    using Dapper.FastCrud.Tests.Contexts;
    using Dapper.FastCrud.Tests.Models;
    using Dapper.FastCrud.Tests.Models.CodeFirst;
    using Dapper.FastCrud.Tests.Models.Metadata;
    using Dapper.FastCrud.Tests.Models.Poco;
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
                var workstation = _testContext.GetInsertedEntitiesOfType<Workstation>().Skip(index).FirstOrDefault();

                // in case we link more employees tan workstations available, use the last one there
                if (workstation == null)
                {
                    workstation = _testContext.GetInsertedEntitiesOfType<Workstation>().Last();
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
                var building = _testContext.GetInsertedEntitiesOfType<Building>().Skip(index).First();
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

        [When(@"I query for one workstation entity combined with the employee entities using (.*) methods")]
        public async Task WhenIQueryForOneWorkstationEntityCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            var workstationId = _testContext.GetInsertedEntitiesOfType<Workstation>()
                                            .Select(workStation => workStation.WorkstationId)
                                            .Single();
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
            
            _testContext.RecordQueriedEntity(queriedEntity);
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

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
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

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
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

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
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

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I query for all the inserted workstation entities using (.*) methods")]
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

        [When(@"I query for all the inserted building entities using (.*) methods")]
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

        [When(@"I query for all the inserted employee entities using (.*) methods")]
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
            var buildingIds = string.Join(",", _testContext.GetInsertedEntitiesOfType<Building>()
                                                           .Select(building => building.BuildingId));

            _testContext.LastCountQueryResult = useAsyncMethods
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
            _testContext.LastCountQueryResult = useAsyncMethods
                ? await _testContext.DatabaseConnection.CountAsync<Workstation>(statement => statement
                                                                                    .Include<Employee>())
                : _testContext.DatabaseConnection.Count<Workstation>(statement => statement.Include<Employee>());
        }

        [When(@"I query for the count of all the employee entities strictly linked to workstation and building entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheEmployeeEntitiesStrictlyLinkedToWorkstationAndBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            _testContext.LastCountQueryResult = useAsyncMethods
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


        [When(@"I batch update all the inserted employee entities using (.*) methods")]
        public async Task WhenIBatchUpdateAMaximumOfEmployeeEntitiesSkippingAndUsingMethods(bool useAsyncMethods)
        {
            var entitiesToUpdate = _testContext.GetInsertedEntitiesOfType<Employee>()
                                               .ToArray();

            var customMapping = OrmConfiguration.GetDefaultEntityMapping<Employee>()
                                                .Clone()
                                                .UpdatePropertiesExcluding(prop =>
                                                                               prop.IsExcludedFromUpdates = true,
                                                                           nameof(Employee.KeyPass),
                                                                           nameof(Employee.BirthDate));

            var updateData = new Employee()
                {
                    KeyPass = Guid.NewGuid(),
                    BirthDate = new DateTime(1951, 09, 30),
                };

            var statementParams = new
                {
                    EntityIds = entitiesToUpdate.Select(originalEntity => originalEntity.UserId).ToArray()
                };
            FormattableString whereCondition;
            switch (OrmConfiguration.DefaultDialect)
            {
                case SqlDialect.PostgreSql:
                    whereCondition = $"{nameof(Employee.UserId):C}=ANY({nameof(statementParams.EntityIds):P})";
                    break;
                case SqlDialect.SqLite:
                    whereCondition = $"{nameof(Employee.UserId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
                default:
                    whereCondition = $"{nameof(Employee.UserId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
            }

            int recordsUpdated;
            if (useAsyncMethods)
            {
                recordsUpdated = await _testContext.DatabaseConnection.BulkUpdateAsync(
                                     updateData,
                                     statement => statement
                                                  .WithEntityMappingOverride(customMapping)
                                                  .Where(whereCondition)
                                                  .WithParameters(statementParams)
                                                  );
            }
            else
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkUpdate(
                    updateData,
                    statement => statement
                                 .WithEntityMappingOverride(customMapping)
                                 .Where(whereCondition)
                                 .WithParameters(statementParams));
            }

            Assert.That(recordsUpdated, Is.EqualTo(entitiesToUpdate.Count()));

            // mimic what we've done on our side
            foreach (var entityToUpdate in entitiesToUpdate)
            {
                var updatedEntity = entityToUpdate.Clone();
                updatedEntity.BirthDate = updateData.BirthDate;
                updatedEntity.KeyPass = updateData.KeyPass;
                _testContext.RecordUpdatedEntity(updatedEntity);
            }
        }

        [When(@"I batch update all the inserted workstation entities using (.*) methods")]
        public async Task WhenIBatchUpdateAMaximumOfWorkstationEntitiesSkippingAndUsingMethods(bool useAsyncMethods)
        {
            var entitiesToUpdate = _testContext.GetInsertedEntitiesOfType<Workstation>()
                                               .ToArray();

            var customMapping = OrmConfiguration.GetDefaultEntityMapping<Workstation>()
                                                .Clone()
                                                .UpdatePropertiesExcluding(prop =>
                                                                               prop.IsExcludedFromUpdates = true,
                                                                           nameof(Workstation.Name));

            var updateData = new Workstation()
                {
                    Name = "Batch updated workstation"
                };

            var statementParams = new
                {
                    EntityIds = entitiesToUpdate.Select(originalEntity => originalEntity.WorkstationId).ToArray()
                };
            FormattableString whereCondition;
            switch (OrmConfiguration.DefaultDialect)
            {
                case SqlDialect.PostgreSql:
                    whereCondition = $"{nameof(Workstation.WorkstationId):C}=ANY({nameof(statementParams.EntityIds):P})";
                    break;
                case SqlDialect.SqLite:
                    whereCondition = $"{nameof(Workstation.WorkstationId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
                default:
                    whereCondition = $"{nameof(Workstation.WorkstationId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
            }

            int recordsUpdated;
            if (useAsyncMethods)
            {
                recordsUpdated = await _testContext.DatabaseConnection.BulkUpdateAsync(updateData,
                                                                                       statement => statement
                                                                                                    .Where(whereCondition)
                                                                                                    .WithParameters(statementParams)
                                                                                                    .WithEntityMappingOverride(customMapping)
                                 );
            }
            else
            {
                recordsUpdated = _testContext.DatabaseConnection.BulkUpdate(updateData,
                                                                            statement => statement
                                                                                         .Where(whereCondition)
                                                                                         .WithParameters(statementParams)
                                                                                         .WithEntityMappingOverride(customMapping)
                );
            }

            Assert.That(recordsUpdated, Is.EqualTo(entitiesToUpdate.Count()));

            // mimic what we've done on our side
            foreach (var entityToUpdate in entitiesToUpdate)
            {
                var updatedEntity = entityToUpdate.Clone();
                updatedEntity.Name = updateData.Name;
                _testContext.RecordUpdatedEntity(updatedEntity);
            }

        }

        [When(@"I batch delete all the inserted workstation entities using (.*) methods")]
        public async Task WhenIBatchDeleteAMaximumOfWorkstationEntitiesSkippingAndUsingMethods(bool useAsyncMethods)
        {
            var entitiesToDelete = _testContext.GetInsertedEntitiesOfType<Workstation>()
                                               .ToArray();

            var statementParams = new
                {
                    EntityIds = entitiesToDelete.Select(originalEntity => originalEntity.WorkstationId).ToArray()
                };
            FormattableString whereCondition;
            switch (OrmConfiguration.DefaultDialect)
            {
                case SqlDialect.PostgreSql:
                    whereCondition = $"{nameof(Workstation.WorkstationId):C}=ANY({nameof(statementParams.EntityIds):P})";
                    break;
                case SqlDialect.SqLite:
                    whereCondition = $"{nameof(Workstation.WorkstationId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
                default:
                    whereCondition = $"{nameof(Workstation.WorkstationId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
            }

            int recordsDeleted;
            if (useAsyncMethods)
            {
                recordsDeleted = await _testContext.DatabaseConnection.BulkDeleteAsync<Workstation>(statement => statement
                                                                                                                 .Where(whereCondition)
                                                                                                                 .WithParameters(statementParams)
                                 );
            }
            else
            {
                recordsDeleted = _testContext.DatabaseConnection.BulkDelete<Workstation>(statement => statement
                                                                                             .Where(whereCondition)
                                                                                             .WithParameters(statementParams)
                );
            }

            Assert.That(recordsDeleted, Is.EqualTo(entitiesToDelete.Count()));
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

        [Then(@"the result of the last query count should be (.*)")]
        public void ThenTheDatabaseCountOfTheQueriedEntitiesShouldBe(int expectedQueryCount)
        {
            Assert.That(_testContext.LastCountQueryResult, Is.EqualTo(expectedQueryCount));
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
            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I partially update all the inserted employee entities")]
        public void WhenIPartiallyUpdateAllTheInsertedEmployeeEntities()
        {
            // prepare a new mapping
            var defaultMapping = OrmConfiguration.GetDefaultEntityMapping<Employee>();
            Assert.IsTrue(defaultMapping.IsFrozen);

            try
            {
                // updates are not possible when the mapping is frozen
                defaultMapping.SetProperty(employee => employee.LastName, propMapping => propMapping.ExcludeFromUpdates());
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            var customMapping = defaultMapping.Clone().UpdatePropertiesExcluding(prop => prop.IsExcludedFromUpdates = true, nameof(Employee.LastName));

            foreach (var insertedEntity in _testContext.GetInsertedEntitiesOfType<Employee>())
            {
                var partialUpdatedEntity = new Employee()
                    {
                        UserId = insertedEntity.UserId,
                        EmployeeId = insertedEntity.EmployeeId,

                        // all the other properties with the exception of the next one will be ignored
                        LastName = "Updated " + insertedEntity.LastName
                    };

                _testContext.DatabaseConnection.Update(partialUpdatedEntity, statement => statement.WithEntityMappingOverride(customMapping));

                // now record it with the copied props
                var updatedEntity = insertedEntity.Clone();
                updatedEntity.LastName = partialUpdatedEntity.LastName;
                updatedEntity.FullName = partialUpdatedEntity.FullName;
                _testContext.RecordUpdatedEntity(updatedEntity);
            }
        }

        private async Task DeleteInsertedEntitiesAsync<TEntity>(int? skipCount = null, int? maxCount = null)
        {
            var entitiesToBeDeleted = _testContext.GetInsertedEntitiesOfType<TEntity>()
                                                  .Skip(skipCount ?? 0)
                                                  .Take(maxCount ?? int.MaxValue)
                                                  .ToArray();

            // we have to escape Specflow's bad synchronization context
            await Task.Run(async () =>
            {
                foreach (var entityToBeDeleted in entitiesToBeDeleted)
                {
                    await _testContext.DatabaseConnection.DeleteAsync(entityToBeDeleted);
                }
            });
        }

        private void DeleteInsertedEntities<TEntity>(int? skipCount = null, int? maxCount = null)
        {
            var entitiesToBeDeleted = _testContext.GetInsertedEntitiesOfType<TEntity>()
                                                  .Skip(skipCount ?? 0)
                                                  .Take(maxCount ?? int.MaxValue)
                                                  .ToArray();
            foreach (var entityToBeDeleted in entitiesToBeDeleted)
            {
                    _testContext.DatabaseConnection.Delete(entityToBeDeleted);
            }
        }

        private async Task UpdateInsertedEntitiesAsync<TEntity>(Func<TEntity, TEntity> updateFunc, int? skipCount = null, int? maxCount = null)
        {
            var entitiesToUpdate = _testContext.GetInsertedEntitiesOfType<TEntity>()
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
                    _testContext.RecordUpdatedEntity(updatedEntity);
                }
            });
        }

        private void UpdateInsertedEntities<TEntity>(Func<TEntity, TEntity> updateFunc,  int? skipCount = null, int? maxCount = null)
        {
            var entitiesToUpdate = _testContext.GetInsertedEntitiesOfType<TEntity>()
                                               .Skip(skipCount ?? 0)
                                               .Take(maxCount ?? int.MaxValue)
                                               .ToArray();
            foreach (var entityToUpdate in entitiesToUpdate)
            {
                var updatedEntity = updateFunc(entityToUpdate);
                _testContext.DatabaseConnection.Update(updatedEntity);
                _testContext.RecordUpdatedEntity(updatedEntity);
            }
        }

        private async Task QueryForInsertedEntitiesAsync<TEntity>(int? skipCount = null, int? maxCount = null)
        {
            var entitiesToQuery = _testContext.GetInsertedEntitiesOfType<TEntity>()
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
                            _testContext.RecordQueriedEntity(queriedEmployeeEntity);
                            break;
                        case Workstation originalWorkstationEntity:
                            var queriedWorkstationEntity = await _testContext.DatabaseConnection.GetAsync(new Workstation
                                {
                                    WorkstationId = originalWorkstationEntity.WorkstationId
                                });
                            _testContext.RecordQueriedEntity(queriedWorkstationEntity);
                            break;
                        case Building originalBuildingEntity:
                            var queriedBuildingEntity = await _testContext.DatabaseConnection.GetAsync(new Building
                                {
                                    BuildingId = originalBuildingEntity.BuildingId
                                });
                            _testContext.RecordQueriedEntity(queriedBuildingEntity);
                            break;
                        default:
                            throw new NotSupportedException($"Don't know what to do with {typeof(TEntity)}");
                    }
                }
            });
        }

        private void QueryForInsertedEntities<TEntity>(int? skipCount = null, int? maxCount = null)
        {
            var entitiesToQuery = _testContext.GetInsertedEntitiesOfType<TEntity>()
                                              .Skip(skipCount ?? 0)
                                              .Take(maxCount ?? int.MaxValue)
                                              .ToArray();
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
                        _testContext.RecordQueriedEntity(queriedEmployeeEntity);
                        break;
                    case Workstation originalWorkstationEntity:
                        var queriedWorkstationEntity = _testContext.DatabaseConnection.Get(new Workstation
                        {
                            WorkstationId = originalWorkstationEntity.WorkstationId
                        });
                        _testContext.RecordQueriedEntity(queriedWorkstationEntity);
                        break;
                    case Building originalBuildingEntity:
                        var queriedBuildingEntity = _testContext.DatabaseConnection.Get(new Building
                        {
                            BuildingId = originalBuildingEntity.BuildingId
                        });
                        _testContext.RecordQueriedEntity(queriedBuildingEntity);
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
                    TEntity entityToInsert = insertFunc(entityIndex);
                    await dbConnection.InsertAsync(entityToInsert);
                    _testContext.RecordInsertedEntity(entityToInsert);
                }

            });
        }

        private void InsertEntity<TEntity>(Func<int, TEntity> insertFunc,  int entityCount = 1)
        {
            var dbConnection = _testContext.DatabaseConnection;
            for(var entityIndex = 0; entityIndex<entityCount; entityIndex++)
            {
                TEntity entityToInsert = insertFunc(entityIndex);
                dbConnection.Insert(entityToInsert);
                _testContext.RecordInsertedEntity(entityToInsert);
            }
        }

        private async Task QueryEntityCountAsync<TEntity>()
        {
            // we have to escape Specflow's bad synchronization context
            _testContext.LastCountQueryResult = await Task.Run(async () =>
            {
                return await _testContext.DatabaseConnection.CountAsync<TEntity>();
            });
        }

        private void QueryEntityCount<TEntity>()
        {
                _testContext.LastCountQueryResult = _testContext.DatabaseConnection.Count<TEntity>();
        }
    }
}
