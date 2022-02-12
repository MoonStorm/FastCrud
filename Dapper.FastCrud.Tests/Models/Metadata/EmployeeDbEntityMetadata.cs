namespace Dapper.FastCrud.Tests.Models.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Dapper.FastCrud.Tests.Models.CodeFirst;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// The metadata class contains all OR extra db mapping info for existing properties on the main entity.
    /// </summary>
    [Table("Employee")]
    internal class EmployeeDbEntityMetadata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", Order = 1)] // order is used when the composite keys on an entity are used as foreign keys in other entities
        public object UserId { get; } // just a marker, the accessors don't matter since they won't be used

        [Key]
        [Dapper.FastCrud.DatabaseGeneratedDefaultValue]
        [Column(Order = 2)] // can specify the order without the name
        public virtual Guid EmployeeId { get; }

        [Dapper.FastCrud.DatabaseGeneratedDefaultValue]
        public virtual Guid KeyPass { get; }

        [ForeignKey(nameof(EmployeeDbEntity.Workstation))]
        public long? WorkstationId { get; } // the type doesn't matter either

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; set; }

        [ForeignKey(nameof(EmployeeDbEntity.Manager))]
        public int? ManagerUserId { get; }
        [ForeignKey(nameof(EmployeeDbEntity.Manager))]
        public int? ManagerEmployeeId { get; }

        [InverseProperty(nameof(EmployeeDbEntity.Manager))]
        public IEnumerable<EmployeeDbEntity> ManagedEmployees { get; }

        [ForeignKey(nameof(EmployeeDbEntity.Supervisor))]
        public int? SupervisorUserId { get; }
        [ForeignKey(nameof(EmployeeDbEntity.Supervisor))]
        public int? SupervisorEmployeeId { get; }

        [InverseProperty(nameof(EmployeeDbEntity.Supervisor))]
        public IEnumerable<EmployeeDbEntity> SupervisedEmployees { get; }

        [InverseProperty(nameof(BadgeDbEntity.Employee))]
        public BadgeDbEntity? Badge { get; set; }

    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}

