namespace Dapper.FastCrud.Tests.Models.CodeFirst
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Dapper.FastCrud.Tests.Models.Metadata;
    using Dapper.FastCrud.Tests.Models.Poco;

    /// <summary>
    /// A code first approach to mapping.
    /// For a clean separation between the database entities and the mappings, see the <see cref="EmployeeDbEntity"/> entity.
    /// </summary>
    [Table("Workstations")]
    public partial class WorkstationDbEntity
    {
        /// <summary>
        /// WorkstationId Column
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long WorkstationId { get; set; }

        /// <summary>
        /// Name Column
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// AccessLevel Column
        /// </summary>
        [Dapper.FastCrud.DatabaseGeneratedDefaultValue]
        public virtual int AccessLevel { get; set; }

        /// <summary>
        /// InventoryIndex Column
        /// </summary>
        public virtual int InventoryIndex { get; set; }

        [ForeignKey(nameof(Building))]
        public int? BuildingId { get; set; }

        /// <summary>
        /// Parent entity, referenced by <see cref="BuildingId"/>.
        /// </summary>
        public BuildingDbEntity Building { get; set; }

        /// <summary>
        /// Children entities.
        /// </summary>
        public virtual IEnumerable<EmployeeDbEntity> Employees { get; set; }
    }
}
