namespace Dapper.FastCrud.Tests.Models.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Dapper.FastCrud.Tests.Models.CodeFirst;
    using Dapper.FastCrud.Tests.Models.Poco;
    
    /// <summary>
    /// Metadata is set up in a separate class.
    /// In this case the entities are kept clean, with the exception of a metadata attribute.
    /// For a pure POCO solution, check out the <see cref="BuildingDbEntity"/> entity.
    /// </summary>
    [MetadataType(typeof(EmployeeDbEntityMetadata))]
    public class EmployeeDbEntity
    {
        public int UserId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid KeyPass { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime BirthDate { get; set; }
        public string FullName { get; set; }
        public int RecordIndex { get; set; }
        public long RecordVersion { get; set; }

        public long? WorkstationId { get; set; }
        public WorkstationDbEntity? Workstation { get; set; }

        public int? ManagerUserId { get; set; }
        public Guid? ManagerEmployeeId { get; set; }
        public EmployeeDbEntity? Manager { get; set; }
        public IEnumerable<EmployeeDbEntity>? ManagedEmployees { get; set; }

        public int? SupervisorUserId { get; set; }
        public Guid? SupervisorEmployeeId { get; set; }
        public EmployeeDbEntity? Supervisor { get; set; }
        public IEnumerable<EmployeeDbEntity>? SupervisedEmployees { get; set; }
        public BadgeDbEntity? Badge { get; set; }

    }
}
