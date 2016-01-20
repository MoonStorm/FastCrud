namespace Dapper.FastCrud.Mappings
{
    using System.ComponentModel;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Gives information about a relationship between two entities.
    /// </summary>
    internal class EntityRelationship
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public EntityRelationship(
            EntityRelationshipType relationshipType, 
            PropertyDescriptor optionalReferedEntityProperty, 
            params PropertyMapping[] referencingProperties)
        {
            Requires.NotNullOrEmptyOrNullElements(referencingProperties, nameof(referencingProperties));

            this.ReferencingProperties = referencingProperties;
            this.ReferedEntityProperty = optionalReferedEntityProperty;
            this.RelationshipType = relationshipType;
        }

        /// <summary>
        /// The main entity properties through which the relationship is established.
        /// </summary>
        public PropertyMapping[] ReferencingProperties { get; private set; }

        /// <summary>
        /// The property representing the entity the relationship reffers to.
        /// </summary>
        public PropertyDescriptor ReferedEntityProperty { get; private set; } 

        /// <summary>
        /// The type of the relationship.
        /// </summary>
        public EntityRelationshipType RelationshipType { get; private set; }
    }
}
