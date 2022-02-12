namespace Dapper.FastCrud.Tests.Models.CodeFirst
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Dapper.FastCrud.Tests.Models.Metadata;

    /// <summary>
    /// A code first approach to mapping.
    /// For a clean separation between the database entities and the mappings, see the <see cref="Employee"/> entity.
    /// </summary>
    [Table("Badges")]
    public class BadgeDbEntity
    {
        /// <summary>
        /// Part of the foreign key for <see cref="Employee"/> in a one-to-one relationship.
        /// </summary>
        [Key]
        [Column("Id", Order = 10)] // must match the oder on the Employee side (values don't matter)
        [ForeignKey(nameof(Employee))]
        public int EmployeeUserId { get; set; }

        /// <summary>
        /// Part of the foreign key for <see cref="Employee"/> in a one-to-one relationship.
        /// </summary>
        [Key]
        [Column(Order = 20)] // must match the oder on the Employee side (values don't matter)
        [ForeignKey(nameof(Employee))]
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// The barcode column
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// The parent <see cref="Employee"/> entity.
        /// </summary>
        public EmployeeDbEntity? Employee { get; set; }
    }
}
