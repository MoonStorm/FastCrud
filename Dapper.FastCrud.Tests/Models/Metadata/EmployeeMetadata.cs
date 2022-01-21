namespace Dapper.FastCrud.Tests.Models.Metadata
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The metadata class contains all or extra db mapping info for existing properties on the main entity.
    /// Any properties that do not exist on the main entity will be ignored.
    /// </summary>
    [Table("Employee")]
    internal class EmployeeMetadata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public object UserId { get; } // just a marker

        [Key]
        [Dapper.FastCrud.DatabaseGeneratedDefaultValue]
        public virtual Guid EmployeeId { get; set; }

        [Dapper.FastCrud.DatabaseGeneratedDefaultValue]
        public virtual Guid KeyPass { get; set; }

        [ForeignKey("Workstation")]
        public virtual long? WorkstationId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual string FullName { get; set; }
    }
}

