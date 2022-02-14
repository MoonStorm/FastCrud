namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Tests.Common;
    using Dapper.FastCrud.Tests.Contexts;
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
        private volatile int _recordIndex = 0;

        public CrudSteps(DatabaseTestContext testContext)
        {
            _testContext = testContext;
            Assert.NotNull(OrmConfiguration.GetDefaultEntityMapping<EmployeeDbEntity>());
        }

        [When(@"I insert (\d*) building entities using (.*) methods")]
        public async Task WhenIInsertBuildingEntities(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<BuildingDbEntity>(CreateBuilding, entitiesCount);
            }
            else
            {
                this.InsertEntity<BuildingDbEntity>(CreateBuilding, entitiesCount);
            }

            BuildingDbEntity CreateBuilding(int index)
            {
                return new BuildingDbEntity()
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
                await this.InsertEntityAsync<WorkstationDbEntity>(CreateWorkstation, entitiesCount);
            }
            else
            {
                this.InsertEntity<WorkstationDbEntity>(CreateWorkstation, entitiesCount);
            }

            WorkstationDbEntity CreateWorkstation(int index)
            {
                return new WorkstationDbEntity()
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
                await this.InsertEntityAsync<EmployeeDbEntity>(CreateEmployee, entitiesCount);
            }
            else
            {
                this.InsertEntity<EmployeeDbEntity>(CreateEmployee, entitiesCount);
            }

            EmployeeDbEntity CreateEmployee(int index)
            {
                return new EmployeeDbEntity()
                    {
                        FirstName = $"First Name {Guid.NewGuid().ToString("N")}",
                        LastName = $"Last Name {Guid.NewGuid().ToString("N")}",
                        RecordIndex = ++_recordIndex,
                        BirthDate = new DateTime(_rnd.Next(2000, 2010), _rnd.Next(1, 12), _rnd.Next(1, 28), _rnd.Next(0, 23), _rnd.Next(0, 59), _rnd.Next(0, 59)),
                    };
            }
        }

        [When(@"I insert (\d*) employee entities parented to existing workstation entities using (.*) methods")]
        public async Task WhenIInsertEmployeeEntitiesLinkedToWorkstationEntities(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<EmployeeDbEntity>(CreateEmployee, entitiesCount);
            }
            else
            {
                this.InsertEntity<EmployeeDbEntity>(CreateEmployee, entitiesCount);
            }

            EmployeeDbEntity CreateEmployee(int index)
            {
                var workstation = _testContext.GetInsertedEntitiesOfType<WorkstationDbEntity>().Skip(index).FirstOrDefault();

                // in case we link more employees than workstations available, use the last one there
                if (workstation == null)
                {
                    workstation = _testContext.GetInsertedEntitiesOfType<WorkstationDbEntity>().Last();
                }

                var employee = new EmployeeDbEntity()
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
                    workstation.Employees = new List<EmployeeDbEntity>();
                }

                ((IList<EmployeeDbEntity>)workstation.Employees).Add(employee);

                return employee;
            }
        }

        [When(@"I insert (.*) employee entities as children of promoted manager and supervisor employee entities using (.*) methods")]
        public async Task WhenIInsertEmployeeEntitiesAsChildrenOfPromotedManagerAndSupervisorEmployeeEntitiesUsingMethods(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<EmployeeDbEntity>(CreateEmployee, entitiesCount);
            }
            else
            {
                this.InsertEntity<EmployeeDbEntity>(CreateEmployee, entitiesCount);
            }

            EmployeeDbEntity CreateEmployee(int index)
            {
                var existingEmployees = _testContext.GetInsertedEntitiesOfType<EmployeeDbEntity>().ToArray();
                var manager = existingEmployees[_rnd.Next(0, existingEmployees.Length - 1)];
                var supervisor = existingEmployees[_rnd.Next(0, existingEmployees.Length - 1)];

                var employee = new EmployeeDbEntity()
                    {
                        FirstName = $"First Name {Guid.NewGuid().ToString("N")}",
                        LastName = $"Last Name {Guid.NewGuid().ToString("N")}",
                        RecordIndex = ++_recordIndex,
                        BirthDate = new DateTime(_rnd.Next(2000, 2010), _rnd.Next(1, 12), _rnd.Next(1, 28), _rnd.Next(0, 23), _rnd.Next(0, 59), _rnd.Next(0, 59)),

                        // assign a self referenced ids for the join tests
                        ManagerUserId = manager.UserId,
                        ManagerEmployeeId = manager.EmployeeId,
                        SupervisorUserId = supervisor.UserId,
                        SupervisorEmployeeId = supervisor.EmployeeId,

                        // these are not going to get inserted in the db, but we need them for local comparisons
                        Manager = manager,
                        Supervisor = supervisor
                    };

                // these collections are not going to get inserted in the db either, but we need them for local comparisons
                if (manager.ManagedEmployees == null)
                {
                    manager.ManagedEmployees = new List<EmployeeDbEntity>();
                }
                ((List<EmployeeDbEntity>)manager.ManagedEmployees).Add(employee);

                if (supervisor.SupervisedEmployees == null)
                {
                    supervisor.SupervisedEmployees = new List<EmployeeDbEntity>();
                }
                ((List<EmployeeDbEntity>)supervisor.SupervisedEmployees).Add(employee);

                return employee;
            }
        }

        [When(@"I assign unique badges to the last inserted (.*) employee entities using (.*) methods")]
        public async Task WhenIAssignUniqueBadgesToTheLastInsertedEmployeeEntitiesUsingMethods(int badgeCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<BadgeDbEntity>(CreateBadge, badgeCount);
            }
            else
            {
                this.InsertEntity<BadgeDbEntity>(CreateBadge, badgeCount);
            }

            BadgeDbEntity CreateBadge(int index)
            {
                var employee = _testContext.GetInsertedEntitiesOfType<EmployeeDbEntity>().Reverse().Skip(index).Take(1).Single();

                var badge = new BadgeDbEntity()
                {
                    EmployeeUserId = employee.UserId,
                    EmployeeId = employee.EmployeeId,
                    Barcode = Guid.NewGuid().ToString("N"),
                    Employee = employee
                };

                employee.Badge = badge;

                return badge;
            }
        }


        [When(@"I insert (\d*) workstation entities parented to existing building entities using (.*) methods")]
        public async Task WhenIInsertWorkstationEntitiesParentedToExistingBuildingEntitiesUsingSynchronousMethods(int entitiesCount, bool makeAsyncCalls)
        {
            if (makeAsyncCalls)
            {
                await this.InsertEntityAsync<WorkstationDbEntity>(CreateEntity, entitiesCount);
            }
            else
            {
                this.InsertEntity<WorkstationDbEntity>(CreateEntity, entitiesCount);
            }

            WorkstationDbEntity CreateEntity(int index)
            {
                var building = _testContext.GetInsertedEntitiesOfType<BuildingDbEntity>().Skip(index).First();
                var workstation = new WorkstationDbEntity()
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
                    building.Workstations = new List<WorkstationDbEntity>();
                }

                ((IList<WorkstationDbEntity>)building.Workstations).Add(workstation);

                return workstation;
            }
        }

        [When(@"I query for one workstation entity combined with the employee entities using (.*) methods")]
        public async Task WhenIQueryForOneWorkstationEntityCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            var workstationId = _testContext.GetInsertedEntitiesOfType<WorkstationDbEntity>()
                                            .Select(workStation => workStation.WorkstationId)
                                            .Single();
            WorkstationDbEntity queriedEntity;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntity = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.GetAsync<WorkstationDbEntity>(
                               new WorkstationDbEntity()
                                   {
                                       WorkstationId = workstationId
                                   },
                               options => options.Include<EmployeeDbEntity>());
                }).ConfigureAwait(false);
            }
            else
            {
                queriedEntity = _testContext.DatabaseConnection.Get<WorkstationDbEntity>(
                    new WorkstationDbEntity()
                        {
                            WorkstationId = workstationId
                        }, options => options.Include<EmployeeDbEntity>());

            }
            
            _testContext.RecordQueriedEntity(queriedEntity);
        }

        [When(@"I query for all the workstation entities combined with the employee entities using (.*) methods")]
        public async Task WhenIQueryForAllTheWorkstationEntitiesCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<WorkstationDbEntity> queriedEntities;
            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.FindAsync<WorkstationDbEntity>(options => options.Include<EmployeeDbEntity>());
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<WorkstationDbEntity>(options => options.Include<EmployeeDbEntity>());
            }

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I query for all the employee entities combined with the workstation entities using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesCombinedWithTheWorkstationEntitiesUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<EmployeeDbEntity> queriedEntities;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.FindAsync<EmployeeDbEntity>(options => options.Include<WorkstationDbEntity>());
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<EmployeeDbEntity>(options => options.Include<WorkstationDbEntity>());
            }

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I query for all the employee entities combined with the assigned badge entities using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesCombinedWithTheAssignedBadgeEntitiesUsingSynchronousMethods(bool useAsyncMethods)
        {
            IEnumerable<EmployeeDbEntity> queriedEntities;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.FindAsync<EmployeeDbEntity>(options => options.Include<BadgeDbEntity>());
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<EmployeeDbEntity>(options => options.Include<BadgeDbEntity>(join => join.LeftOuterJoin().MapResults()));
            }

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }


        [When(@"I query for all the employee entities combined with the workstation entities when no relationships or navigation properties are set up using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesCombinedWithTheWorkstationEntitiesWhenNoRelationshipsOrNavigationPropertiesAreSetUpUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<EmployeeDbEntity> queriedEntities;
            var workstationMappingNoRelationships = OrmConfiguration.GetDefaultEntityMapping<WorkstationDbEntity>()
                                                                    .Clone()
                                                                    .RemoveAllRelationships();
            var employeeMappingNoRelationships = OrmConfiguration.GetDefaultEntityMapping<EmployeeDbEntity>()
                                                                 .Clone()
                                                                 .RemoveAllRelationships();


            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.FindAsync<EmployeeDbEntity>(options => options
                                                                                                .WithAlias("em")
                                                                                                .WithEntityMappingOverride(employeeMappingNoRelationships)
                                                                                                .Include<WorkstationDbEntity>(join => join.WithEntityMappingOverride(workstationMappingNoRelationships)
                                                                                                                                          .MapResults(true)
                                                                                                                                          .WithAlias("ws")
                                                                                                                                          .Referencing<EmployeeDbEntity>(rel => 
                                                                                                                                              rel.FromAlias("em")
                                                                                                                                                 .FromProperty(employee => employee.Workstation)
                                                                                                                                                 .ToProperty(workstation => workstation.Employees))
                                                                                                                                          .On($"{nameof(EmployeeDbEntity.WorkstationId):of em} = {nameof(WorkstationDbEntity.WorkstationId):of ws}")
                                                                                                                                          ));
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<EmployeeDbEntity>(options => options
                                                                                          .WithAlias("em")
                                                                                          .WithEntityMappingOverride(employeeMappingNoRelationships)
                                                                                          .Include<WorkstationDbEntity>(join => join.WithEntityMappingOverride(workstationMappingNoRelationships)
                                                                                                                                    .MapResults(true)
                                                                                                                                    .WithAlias("ws")
                                                                                                                                    .Referencing<EmployeeDbEntity>(rel =>
                                                                                                                                                                       rel.FromAlias("em")
                                                                                                                                                                           .FromProperty(employee => employee.Workstation)
                                                                                                                                                                           .ToProperty(workstation => workstation.Employees))
                                                                                                                                    .On($"{nameof(EmployeeDbEntity.WorkstationId):of em} = {nameof(WorkstationDbEntity.WorkstationId):of ws}")
                                                                                          ));
            }

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }


        [When(@"I query for all the building entities combined with workstation and employee entities using (.*) methods")]
        public async Task WhenIQueryForAllTheBuildingEntitiesCombinedWithWorkstationAndEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<BuildingDbEntity> queriedEntities;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection.FindAsync<BuildingDbEntity>(options => options
                                                                                                .Include<WorkstationDbEntity>()
                                                                                                .Include<EmployeeDbEntity>());
                });
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<BuildingDbEntity>(options => options
                                                                                            .Include<WorkstationDbEntity>()
                                                                                            .Include<EmployeeDbEntity>());
            }

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I query for all the employee entities combined with workstation and building entities using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesCombinedWithWorkstationAndBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            IEnumerable<EmployeeDbEntity> queriedEntities;

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                                      {
                                          return await _testContext.DatabaseConnection
                                                                   .FindAsync<EmployeeDbEntity>(options => options
                                                                                                   .WithAlias("e")
                                                                                                   .Include<WorkstationDbEntity>()
                                                                                                   .Include<BuildingDbEntity>()
                                                                                                   );
                                      }
                                  );
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<EmployeeDbEntity>(options => options
                                                                                                    .Include<WorkstationDbEntity>()
                                                                                                    .Include<BuildingDbEntity>()
                );
            }

            foreach (var queriedEntity in queriedEntities)
            {
                _testContext.RecordQueriedEntity(queriedEntity);
            }
        }

        [When(@"I query for all the employee entities combined with themselves as managers and supervisors using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesCombinedWithThemselvesAsManagersAndSupervisorsUsingMethods(bool useAsyncMethods)
        {
            await this.WhenIQueryForTopEmployeeEntitiesCombinedWithThemselvesAsManagersAndSupervisorsSkippingEntitiesUsingMethods(null, useAsyncMethods);
        }

        [When(@"I query for the last (\d*) inserted employee entities combined with themselves as managers and supervisors using (.*) methods")]
        public async Task WhenIQueryForTopEmployeeEntitiesCombinedWithThemselvesAsManagersAndSupervisorsSkippingEntitiesUsingMethods(int? lastInsertedRecordCount, bool useAsyncMethods)
        {
            IEnumerable<EmployeeDbEntity> queriedEntities;

            var queryParams = new
                {
                    MinRecordIndex = lastInsertedRecordCount.HasValue
                                    ? _testContext.GetInsertedEntitiesOfType<EmployeeDbEntity>().TakeLast(lastInsertedRecordCount.Value).First().RecordIndex
                                    : 0
                };

            if (useAsyncMethods)
            {
                // we have to escape Specflow's bad synchronization context
                queriedEntities = await Task.Run(async () =>
                {
                    return await _testContext.DatabaseConnection
                                             .FindAsync<EmployeeDbEntity>(options => options
                                                                                     .Include<EmployeeDbEntity>(join => join
                                                                                                                        .WithAlias("manager")
                                                                                                                        .Referencing<EmployeeDbEntity>(rel => rel
                                                                                                                                                           .FromProperty(employee => employee.Manager)
                                                                                                                                                           .ToProperty(manager => manager.ManagedEmployees)))
                                                                                     .Include<EmployeeDbEntity>(join => join
                                                                                                                    .WithAlias("supervisor")
                                                                                                                    .Referencing<EmployeeDbEntity>(rel => rel
                                                                                                                                                          .FromProperty(employee => employee.Supervisor)
                                                                                                                                                          .ToProperty(employee => employee.SupervisedEmployees)))
                                                                             .Where(lastInsertedRecordCount.HasValue
                                                                                        ? (FormattableString?)$"{nameof(EmployeeDbEntity.RecordIndex):TC}>={nameof(queryParams.MinRecordIndex):P}"
                                                                                        : (FormattableString?)null)
                                                                             .OrderBy($"{nameof(EmployeeDbEntity.UserId):TC} ASC")
                                                                             .WithParameters(queryParams)

                                             );
                }
                                  );
            }
            else
            {
                queriedEntities = _testContext.DatabaseConnection.Find<EmployeeDbEntity>(options => options
                                                                                            .WithAlias("employee")
                                                                                            .Include<EmployeeDbEntity>(join => join
                                                                                                                               .WithAlias("manager")
                                                                                                                               .Referencing<EmployeeDbEntity>(rel => rel
                                                                                                                                                                     .FromAlias("employee")
                                                                                                                                                                     .FromProperty(employee => employee.Manager)
                                                                                                                                                                     .ToProperty(manager => manager.ManagedEmployees)))
                                                                                            .Include<EmployeeDbEntity>(join => join
                                                                                                                               .WithAlias("supervisor")
                                                                                                                               .Referencing<EmployeeDbEntity>(rel => rel
                                                                                                                                                                     .FromAlias("employee")
                                                                                                                                                                     .FromProperty(employee => employee.Supervisor)
                                                                                                                                                                     .ToProperty(employee => employee.SupervisedEmployees)))
                                                                                            .Where(lastInsertedRecordCount.HasValue
                                                                                                       ? (FormattableString?)$"{nameof(EmployeeDbEntity.RecordIndex):of employee}>={nameof(queryParams.MinRecordIndex):P}"
                                                                                                       : (FormattableString?)null)
                                                                                            .OrderBy($"{nameof(EmployeeDbEntity.UserId):of employee} ASC")
                                                                                            .WithParameters(queryParams)
                );
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
                await this.QueryForInsertedEntitiesAsync<WorkstationDbEntity>();
            }
            else
            {
                this.QueryForInsertedEntities<WorkstationDbEntity>();
            }
        }

        [When(@"I query for all the inserted building entities using (.*) methods")]
        public async Task WhenIQueryForAllTheBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<BuildingDbEntity>();
            }
            else
            {
                this.QueryForInsertedEntities<BuildingDbEntity>();
            }
        }

        [When(@"I query for all the inserted employee entities using (.*) methods")]
        public async Task WhenIQueryForAllTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<EmployeeDbEntity>();
            }
            else
            {
                this.QueryForInsertedEntities<EmployeeDbEntity>();
            }
        }

        [When(@"I query for the count of all the employee entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryEntityCountAsync<EmployeeDbEntity>();
            }
            else
            {
                this.QueryEntityCount<EmployeeDbEntity>();
            }
        }

        [When(@"I query for the count of all the building entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryEntityCountAsync<BuildingDbEntity>();
            }
            else
            {
                this.QueryEntityCount<BuildingDbEntity>();
            }
        }

        [When(@"I query for the count of all the workstation entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheWorkstationEntitiesUsingMethods(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryEntityCountAsync<WorkstationDbEntity>();
            }
            else
            {
                this.QueryEntityCount<WorkstationDbEntity>();
            }
        }

        [When(@"I query for the count of all the inserted building entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheInsertedBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            FormattableString whereClause = $"charindex(cast({nameof(BuildingDbEntity.BuildingId):of b} as varchar(10)), @BuildingIds) > 4";

            var buildingIds = "ids:" + string.Join(",", _testContext.GetInsertedEntitiesOfType<BuildingDbEntity>()
                                                           .Select(building => building.BuildingId));

            _testContext.LastCountQueryResult = useAsyncMethods
                                                    ? await _testContext
                                                            .DatabaseConnection
                                                            .CountAsync<BuildingDbEntity>(statement => statement
                                                                                               .WithAlias("b")
                                                                                               .Where(whereClause)
                                                                                               .WithParameters(new { BuildingIds = buildingIds }))
                                                    : _testContext
                                                      .DatabaseConnection
                                                      .Count<BuildingDbEntity>(statement => statement
                                                                                    .WithAlias("b")
                                                                                    .Where(whereClause)
                                                                                    .WithParameters(new { BuildingIds = buildingIds }));
        }

        [When(@"I query for the count of all the workstation entities combined with the employee entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheWorkstationEntitiesCombinedWithTheEmployeeEntitiesUsingMethods(bool useAsyncMethods)
        {
            _testContext.LastCountQueryResult = useAsyncMethods
                                                    ? await _testContext.DatabaseConnection.CountAsync<WorkstationDbEntity>(statement => statement
                                                                                                                                // old way
                                                                                                                                .Include<EmployeeDbEntity>(join => join.InnerJoin()))
                                                    : _testContext.DatabaseConnection.Count<WorkstationDbEntity>(statement => statement
                                                                                                                              .WithAlias("ws")
                                                                                                                              .Include<EmployeeDbEntity>(join => join.InnerJoin()));
        }

        [When(@"I query for the count of all the workstation entities combined with the employee entities when no relationships or navigation properties are set up using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheWorkstationEntitiesCombinedWithTheEmployeeEntitiesWhenNoRelationshipsOrNavigationPropertiesAreSetUpUsingSynchronousMethods(bool useAsyncMethods)
        {
            var workstationMappingNoRelationships = OrmConfiguration.GetDefaultEntityMapping<WorkstationDbEntity>()
                                                                    .Clone()
                                                                    .RemoveAllRelationships();
            var employeeMappingNoRelationships = OrmConfiguration.GetDefaultEntityMapping<EmployeeDbEntity>()
                                                                    .Clone()
                                                                    .RemoveAllRelationships();

            if (useAsyncMethods)
            {
                _testContext.LastCountQueryResult = await _testContext.DatabaseConnection
                                                                      .CountAsync<WorkstationDbEntity>(statement => 
                                                                                                   statement
                                                                                                           .WithAlias("ws")
                                                                                                           .WithEntityMappingOverride(workstationMappingNoRelationships)
                                                                                                           .Include<EmployeeDbEntity>(join => join
                                                                                                                                              .InnerJoin()
                                                                                                                                              .WithAlias("em")
                                                                                                                                              .WithEntityMappingOverride(employeeMappingNoRelationships)
                                                                                                                                              .On($"{nameof(EmployeeDbEntity.WorkstationId):of em} = {nameof(WorkstationDbEntity.WorkstationId):of ws}")));
            }
            else
            {
                _testContext.LastCountQueryResult = _testContext.DatabaseConnection.Count<WorkstationDbEntity>(statement =>
                                                                                                                statement
                                                                                                                    .WithAlias("ws")
                                                                                                                    .Include<EmployeeDbEntity>(join => join
                                                                                                                                                       .MapResults(false)
                                                                                                                                                       .InnerJoin()
                                                                                                                                                       .WithAlias("em")
                                                                                                                                                       .WithEntityMappingOverride(employeeMappingNoRelationships)
                                                                                                                                                       .On($"{nameof(EmployeeDbEntity.WorkstationId):of em} = {nameof(WorkstationDbEntity.WorkstationId):of ws}")));
            }
        }


        [When(@"I query for the count of all the employee entities strictly linked to workstation and building entities using (.*) methods")]
        public async Task WhenIQueryForTheCountOfAllTheEmployeeEntitiesStrictlyLinkedToWorkstationAndBuildingEntitiesUsingMethods(bool useAsyncMethods)
        {
            _testContext.LastCountQueryResult = useAsyncMethods
                                                      ? await _testContext.DatabaseConnection.CountAsync<EmployeeDbEntity>(statement => statement
                                                                                                                                .Include<WorkstationDbEntity>(join => join.InnerJoin())
                                                                                                                                .Include<BuildingDbEntity>())
                                                      : _testContext.DatabaseConnection.Count<EmployeeDbEntity>(statement => statement
                                                                                                                     .Include<WorkstationDbEntity>(join => join.InnerJoin())
                                                                                                                     .Include<BuildingDbEntity>(join => join.InnerJoin()));
        }

        [When(@"I query for the inserted building entities using (.*) methods")]
        public async Task WhenIQueryForTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<BuildingDbEntity>();
            }
            else
            {
                this.QueryForInsertedEntities<BuildingDbEntity>();
            }
        }

        [When(@"I query for the inserted workstation entities using (.*) methods")]
        public async Task WhenIQueryForTheInsertedWorkstationEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<WorkstationDbEntity>();
            }
            else
            {
                this.QueryForInsertedEntities<WorkstationDbEntity>();
            }
        }

        [When(@"I query for the inserted employee entities using (.*) methods")]
        public async Task WhenIQueryForTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.QueryForInsertedEntitiesAsync<EmployeeDbEntity>();
            }
            else
            {
                this.QueryForInsertedEntities<EmployeeDbEntity>();
            }
        }


        [When(@"I batch update all the inserted employee entities using (.*) methods")]
        public async Task WhenIBatchUpdateAMaximumOfEmployeeEntitiesSkippingAndUsingMethods(bool useAsyncMethods)
        {
            var entitiesToUpdate = _testContext.GetInsertedEntitiesOfType<EmployeeDbEntity>()
                                               .ToArray();

            var customMapping = OrmConfiguration.GetDefaultEntityMapping<EmployeeDbEntity>()
                                                .Clone()
                                                .UpdatePropertiesExcluding(prop =>
                                                                               prop.IsExcludedFromUpdates = true,
                                                                           nameof(EmployeeDbEntity.KeyPass),
                                                                           nameof(EmployeeDbEntity.BirthDate));

            var updateData = new EmployeeDbEntity()
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
                    whereCondition = $"{nameof(EmployeeDbEntity.UserId):C}=ANY({nameof(statementParams.EntityIds):P})";
                    break;
                case SqlDialect.SqLite:
                    whereCondition = $"{nameof(EmployeeDbEntity.UserId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
                default:
                    whereCondition = $@"
                        /* comment test */
                        {nameof(EmployeeDbEntity.UserId):C} IN {nameof(statementParams.EntityIds):P}
                    ";
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
            var entitiesToUpdate = _testContext.GetInsertedEntitiesOfType<WorkstationDbEntity>()
                                               .ToArray();

            var customMapping = OrmConfiguration.GetDefaultEntityMapping<WorkstationDbEntity>()
                                                .Clone()
                                                .UpdatePropertiesExcluding(prop =>
                                                                               prop.IsExcludedFromUpdates = true,
                                                                           nameof(WorkstationDbEntity.Name));

            var updateData = new WorkstationDbEntity()
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
                    whereCondition = $"{nameof(WorkstationDbEntity.WorkstationId):C}=ANY({nameof(statementParams.EntityIds):P})";
                    break;
                case SqlDialect.SqLite:
                    whereCondition = $"{nameof(WorkstationDbEntity.WorkstationId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
                default:
                    whereCondition = $"{nameof(WorkstationDbEntity.WorkstationId):C} IN {nameof(statementParams.EntityIds):P}";
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
            var entitiesToDelete = _testContext.GetInsertedEntitiesOfType<WorkstationDbEntity>()
                                               .ToArray();

            var statementParams = new
                {
                    EntityIds = entitiesToDelete.Select(originalEntity => originalEntity.WorkstationId).ToArray()
                };
            FormattableString whereCondition;
            switch (OrmConfiguration.DefaultDialect)
            {
                case SqlDialect.PostgreSql:
                    whereCondition = $"{nameof(WorkstationDbEntity.WorkstationId):C}=ANY({nameof(statementParams.EntityIds):P})";
                    break;
                case SqlDialect.SqLite:
                    whereCondition = $"{nameof(WorkstationDbEntity.WorkstationId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
                default:
                    whereCondition = $"{nameof(WorkstationDbEntity.WorkstationId):C} IN {nameof(statementParams.EntityIds):P}";
                    break;
            }

            int recordsDeleted;
            if (useAsyncMethods)
            {
                recordsDeleted = await _testContext.DatabaseConnection.BulkDeleteAsync<WorkstationDbEntity>(statement => statement
                                                                                                                 .Where(whereCondition)
                                                                                                                 .WithParameters(statementParams)
                                 );
            }
            else
            {
                recordsDeleted = _testContext.DatabaseConnection.BulkDelete<WorkstationDbEntity>(statement => statement
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
                await this.UpdateInsertedEntitiesAsync<EmployeeDbEntity>(UpdateEmployee);
            }
            else
            {
                this.UpdateInsertedEntities<EmployeeDbEntity>(UpdateEmployee);
            }

            EmployeeDbEntity UpdateEmployee(EmployeeDbEntity originalEmployeeEntity)
            {
                return new EmployeeDbEntity()
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
                await this.UpdateInsertedEntitiesAsync<BuildingDbEntity>(UpdateBuilding);
            }
            else
            {
                this.UpdateInsertedEntities<BuildingDbEntity>(UpdateBuilding);
            }

            BuildingDbEntity UpdateBuilding(BuildingDbEntity originalBuildingEntity)
            {
                return new BuildingDbEntity()
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
                await this.UpdateInsertedEntitiesAsync<WorkstationDbEntity>(UpdateWorkstation);
            }
            else
            {
                this.UpdateInsertedEntities<WorkstationDbEntity>(UpdateWorkstation);
            }

            WorkstationDbEntity UpdateWorkstation(WorkstationDbEntity originalWorkstationEntity)
            {
                return new WorkstationDbEntity()
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
                await this.DeleteInsertedEntitiesAsync<WorkstationDbEntity>();
            }
            else
            {
                this.DeleteInsertedEntities<WorkstationDbEntity>();
            }
        }

        [When(@"I delete all the inserted employee entities using (.*) methods")]
        public async Task WhenIDeleteAllTheInsertedEmployeeEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.DeleteInsertedEntitiesAsync<EmployeeDbEntity>();
            }
            else
            {
                this.DeleteInsertedEntities<EmployeeDbEntity>();
            }
        }

        [When(@"I delete all the inserted building entities using (.*) methods")]
        public async Task WhenIDeleteAllTheInsertedBuildingEntities(bool useAsyncMethods)
        {
            if (useAsyncMethods)
            {
                await this.DeleteInsertedEntitiesAsync<BuildingDbEntity>();
            }
            else
            {
                this.DeleteInsertedEntities<BuildingDbEntity>();
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
            var queriedEntities = _testContext.DatabaseConnection.Find<WorkstationDbEntity>(
                statementOptions =>
                    statementOptions.Where($"{nameof(WorkstationDbEntity.WorkstationId):C} IS NOT NULL")
                                    .OrderBy($"{nameof(WorkstationDbEntity.InventoryIndex):C} DESC")
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
            var defaultMapping = OrmConfiguration.GetDefaultEntityMapping<EmployeeDbEntity>();
            Assert.IsTrue(defaultMapping.Registration.IsFrozen);

            try
            {
                // updates are not possible when the mapping is frozen
                defaultMapping.SetProperty(employee => employee.LastName, propMapping => propMapping.ExcludeFromUpdates());
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }

            var customMapping = defaultMapping.Clone().UpdatePropertiesExcluding(prop => prop.IsExcludedFromUpdates = true, nameof(EmployeeDbEntity.LastName));

            foreach (var insertedEntity in _testContext.GetInsertedEntitiesOfType<EmployeeDbEntity>())
            {
                var partialUpdatedEntity = new EmployeeDbEntity()
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
                        case EmployeeDbEntity originalEmployeeEntity:
                            var queriedEmployeeEntity = await _testContext.DatabaseConnection.GetAsync(
                                                            new EmployeeDbEntity
                                                                {
                                                                    UserId = originalEmployeeEntity.UserId, 
                                                                    EmployeeId = originalEmployeeEntity.EmployeeId
                                                                });
                            _testContext.RecordQueriedEntity(queriedEmployeeEntity);
                            break;
                        case WorkstationDbEntity originalWorkstationEntity:
                            var queriedWorkstationEntity = await _testContext.DatabaseConnection.GetAsync(new WorkstationDbEntity
                                {
                                    WorkstationId = originalWorkstationEntity.WorkstationId
                                });
                            _testContext.RecordQueriedEntity(queriedWorkstationEntity);
                            break;
                        case BuildingDbEntity originalBuildingEntity:
                            var queriedBuildingEntity = await _testContext.DatabaseConnection.GetAsync(new BuildingDbEntity
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
                    case EmployeeDbEntity originalEmployeeEntity:
                        var queriedEmployeeEntity = _testContext.DatabaseConnection.Get(
                                                        new EmployeeDbEntity
                                                        {
                                                            UserId = originalEmployeeEntity.UserId,
                                                            EmployeeId = originalEmployeeEntity.EmployeeId
                                                        });
                        _testContext.RecordQueriedEntity(queriedEmployeeEntity);
                        break;
                    case WorkstationDbEntity originalWorkstationEntity:
                        var queriedWorkstationEntity = _testContext.DatabaseConnection.Get(new WorkstationDbEntity
                        {
                            WorkstationId = originalWorkstationEntity.WorkstationId
                        });
                        _testContext.RecordQueriedEntity(queriedWorkstationEntity);
                        break;
                    case BuildingDbEntity originalBuildingEntity:
                        var queriedBuildingEntity = _testContext.DatabaseConnection.Get(new BuildingDbEntity
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
