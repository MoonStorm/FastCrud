namespace Dapper.FastCrud.Mappings
{
    using System;

    [Flags]
    public enum PropertyMappingOptions
    {
        /// <summary>
        /// Identifies a regular property. 
        /// The property will be used in operations such as <c>Insert</c> and <c>Update</c>.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Key properties are used to identify the attached record in the database. 
        /// You are not restricted in having only one key property, as many databases accept composite keys.
        /// </summary>
        KeyProperty = 0x01,

        /// <summary>
        /// This is a database generated property. Database generated key properties will be excluded from <c>Insert</c>, and their values will be updated on the entity once the insertion has completed. 
        /// </summary>
        //DatabaseGeneratedProperty = 0x02,

        /// <summary>
        /// Useful for partial updates, a property marked with this option will be excluded from any <c>Update</c> operations.
        /// </summary>
        ExcludedFromUpdates = 0x04,

        /// <summary>
        /// Useful for partial updates and identity columns, a property marked with this option will be excluded from any <c>Insert</c> operations.
        /// </summary>
        ExcludedFromInserts = 0x08,

        /// <summary>
        /// The value is not going to be re-read from the database on INSERTs.
        /// </summary>
        RefreshPropertyOnInserts = 0x10,

        /// <summary>
        /// The value is not going to be re-read from the database on UPDATEs.
        /// </summary>
        RefreshPropertyOnUpdates = 0x20,
    }
}
