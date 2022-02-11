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
    /// For a pure POCO solution, check out the <see cref="Building"/> entity.
    /// </summary>
    [MetadataType(typeof(EmployeeMetadata))]
    public class Employee
    {
        public int UserId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid KeyPass { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime BirthDate { get; set; }
        public string FullName { get; set; }
        public int RecordIndex { get; set; }
        
        public long? WorkstationId { get; set; }
        public Workstation? Workstation { get; set; }

        public int? ManagerUserId { get; set; }
        public Guid? ManagerEmployeeId { get; set; }
        public Employee? Manager { get; set; }
        public IEnumerable<Employee>? ManagedEmployees { get; set; }

        public int? SupervisorUserId { get; set; }
        public Guid? SupervisorEmployeeId { get; set; }
        public Employee? Supervisor { get; set; }
        public IEnumerable<Employee>? SupervisedEmployees { get; set; }

    }
}
