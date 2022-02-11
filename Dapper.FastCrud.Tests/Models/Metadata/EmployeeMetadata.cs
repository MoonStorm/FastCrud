namespace Dapper.FastCrud.Tests.Models.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The metadata class contains all OR extra db mapping info for existing properties on the main entity.
    /// </summary>
    [Table("Employee")]
    internal class EmployeeMetadata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public object UserId { get; } // just a marker, the accessors don't matter since they won't be used

        [Key]
        [Dapper.FastCrud.DatabaseGeneratedDefaultValue]
        public virtual Guid EmployeeId { get; }

        [Dapper.FastCrud.DatabaseGeneratedDefaultValue]
        public virtual Guid KeyPass { get; }

        [ForeignKey(nameof(Employee.Workstation))]
        public long? WorkstationId { get; } // the type doesn't matter either

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; set; }

        [ForeignKey(nameof(Employee.Manager))]
        public int? ManagerUserId { get; }
        [ForeignKey(nameof(Employee.Manager))]
        public int? ManagerEmployeeId { get; }

        [InverseProperty(nameof(Employee.Manager))]
        public IEnumerable<Employee> ManagedEmployees { get; }

        [ForeignKey(nameof(Employee.Supervisor))]
        public int? SupervisorUserId { get; }
        [ForeignKey(nameof(Employee.Supervisor))]
        public int? SupervisorEmployeeId { get; }

        [InverseProperty(nameof(Employee.Supervisor))]
        public IEnumerable<Employee> SupervisedEmployees { get; }
    }
}

