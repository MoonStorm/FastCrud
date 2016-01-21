namespace Dapper.FastCrud.Mappings
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Gives information about a relationship between two entities.
    /// </summary>
    internal class EntityMappingForeignKeyRelationship
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public EntityMappingForeignKeyRelationship(
            PropertyDescriptor referencingForeignEntityProperty,
            PropertyMapping[] referencingForeignKeyProperties,
            Type referencedEntityType,
             PropertyDescriptor referencedEntityProperty)
        {
            this.ReferencingForeignKeyProperties = referencingForeignKeyProperties;
            this.ReferencingForeignEntityProperty = referencingForeignEntityProperty;
            this.ReferencedEntityType = referencedEntityType;
            this.ReferencedEntityProperty = referencedEntityProperty;
        }

        /// <summary>
        /// The main entity properties through which the relationship is established.
        /// </summary>
        public PropertyMapping[] ReferencingForeignKeyProperties { get; }

        /// <summary>
        /// The property representing the entity the relationship reffers to. It can be null.
        /// </summary>
        public PropertyDescriptor ReferencingForeignEntityProperty { get; }
        
        /// <summary>
        /// Gets the referenced entity type.
        /// </summary>
        public Type ReferencedEntityType { get; }

        /// <summary>
        /// The property representing the entity the relationship is sourced from. It can be null.
        /// </summary>
        public PropertyDescriptor ReferencedEntityProperty { get; }

    }
}
