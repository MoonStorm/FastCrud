namespace Dapper.FastCrud.Mappings
{
    /// <summary>
    /// Gives information about a relationship between two entities.
    /// </summary>
    public abstract class EntityRelationship
    {
        /// <summary>
        /// The main entity's property through which the relationship is established.
        /// </summary>
        internal PropertyMapping[] SourceRelationshipProperties { get; private set; }

        /// <summary>
        /// The related entity's property through which the relationship is established.
        /// </summary>
        internal PropertyMapping[] TargetRelationshipProperties { get; private set; }

        /// <summary>
        /// The type of the relationship.
        /// </summary>
        internal EntityRelationshipType RelationshipType { get; private set; }
    }
}
