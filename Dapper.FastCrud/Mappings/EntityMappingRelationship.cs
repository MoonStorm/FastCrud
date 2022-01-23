namespace Dapper.FastCrud.Mappings
{
    using Dapper.FastCrud.Mappings.Registrations;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Gives information about a relationship between two entities.
    /// </summary>
    internal class EntityMappingRelationship
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public EntityMappingRelationship(
            Type referencedEntityType,
            PropertyRegistration[] referencingKeyProperties,
            PropertyDescriptor referencingEntityProperty = null)
        {
            this.ReferencingKeyProperties = referencingKeyProperties;
            this.ReferencingEntityProperty = referencingEntityProperty;
            this.ReferencedEntityType = referencedEntityType;
        }

        /// <summary>
        /// The main entity properties through which the relationship is established.
        /// </summary>
        public PropertyRegistration[] ReferencingKeyProperties { get; }

        /// <summary>
        /// The property representing the entity the relationship reffers to. It can be null.
        /// </summary>
        public PropertyDescriptor ReferencingEntityProperty { get; }
        
        /// <summary>
        /// Gets the referenced entity type.
        /// </summary>
        public Type ReferencedEntityType { get; }
    }
}
