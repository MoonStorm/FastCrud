## Entity generation using a database first approach (limited to SQL Server)

Entity generation can be performed by installing the NuGet package ``Dapper.FastCrud.ModelGenerator`` and by creating  
your own (``*Config.tt``) files that use the generic template provided in this package. 
Head over to the wiki section on the project website for more details.


## Output Example

```
    /// <summary>
    /// Represents the 'Badges' table.
    /// </summary>
    [Table("Badges")]
    public partial class BadgeEntity
    {
        /// <summary>
        /// Represents the column 'Id'.
        /// </summary>
        [Key]
        [Column(Order = 1)]
        [ForeignKey(nameof(Employee))]
        public virtual int AssetId { get; set; }

        /// <summary>
        /// Represents the column 'EmployeeId'.
        /// </summary>
        [Key]
        [Column(Order = 2)]
        [ForeignKey(nameof(Employee))]
        public virtual Guid EmployeeId { get; set; }

        /// <summary>
        /// Represents the column 'Barcode'.
        /// </summary>
        public virtual string Barcode { get; set; }

        /// <summary>
        /// Represents the navigation property for the child-parent relationship involving <seealso cref="EmployeeEntity"/>
        /// </summary>
        public virtual EmployeeEntity? Employee { get; set; }
    }

```

## Release Notes

### 3.3
- An opt-in flag was added for generating schema decorated entities.
- Several problems involving schemas were fixed 
(e.g. identical table names in separate schemas, relationships, general metadata extraction).
- Parent entity properties are now generated as nullable.
- Added support for ``TimeOnly`` and ``DateOnly``.
- Added warning suppressions for non-nullable properties.
- Customizable entity class modifiers.
- Customizable entity property modifiers.

### 3.0
- [Breaking change] Support for composite primary keys.
- [Breaking change] Added support for self referenced entities.
- [Breaking change] Added support for multiple references to the same target using the InverseProperty attribute.
- [Breaking change] Better handling of columns representing reserved keywords in C#.
- Support for new csproj style projects.
- Fixed a problem preventing it from being used in VS2019 and later.

