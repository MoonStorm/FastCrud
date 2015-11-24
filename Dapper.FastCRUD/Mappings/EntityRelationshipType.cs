namespace Dapper.FastCrud.Mappings
{
    /// <summary>
    /// The type of the relationship between two entities.
    /// </summary>
    public enum EntityRelationshipType
    {
        /// <summary>
        /// 1-*
        /// </summary>
        OneToMany,

        /// <summary>
        /// *-1
        /// </summary>
        ManyToOne
    }
}
