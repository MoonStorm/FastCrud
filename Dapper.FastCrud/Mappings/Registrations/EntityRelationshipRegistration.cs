namespace Dapper.FastCrud.Mappings.Registrations
{
    using Dapper.FastCrud.Validations;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Holds information about the relationship between two entities.
    /// </summary>
    internal class EntityRelationshipRegistration
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityRelationshipRegistration(
            EntityRelationshipType relationshipType, 
            Type referencedEntity,
            string[] referencedColumnProperties,
            string[] referencingColumnProperties, 
            PropertyDescriptor? referencingNavigationProperty)
        {
            Requires.NotNull(referencedEntity, nameof(referencedEntity));
            Requires.NotNull(referencingColumnProperties, nameof(referencingColumnProperties));
            Requires.NotNull(referencedColumnProperties, nameof(referencedColumnProperties));

            this.RelationshipType = relationshipType;
            this.ReferencedEntity = referencedEntity;
            this.ReferencingColumnProperties = referencingColumnProperties;
            this.ReferencedColumnProperties = referencedColumnProperties;
            this.ReferencingNavigationProperty = referencingNavigationProperty;
        }

        /// <summary>
        /// Type of relationship.
        /// </summary>
        public EntityRelationshipType RelationshipType  { get; }

        /// <summary>
        /// The referenced entity.
        /// </summary>
        public Type ReferencedEntity { get; }

        /// <summary>
        /// The property or properties on the current entity involved in referencing the other entity.
        /// </summary>
        public string[] ReferencingColumnProperties { get; }

        /// <summary>
        /// The property or properties on the referenced entity involved in referencing the current entity.
        /// </summary>
        public string[] ReferencedColumnProperties { get; }

        /// <summary>
        /// The property on the current entity referencing the other entity.
        /// </summary>
        public PropertyDescriptor? ReferencingNavigationProperty { get; }
    }
}
