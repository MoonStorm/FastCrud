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
            Validate.NotNull(referencedEntity, nameof(referencedEntity));
            Validate.NotNull(referencingColumnProperties, nameof(referencingColumnProperties));
            Validate.NotNull(referencedColumnProperties, nameof(referencedColumnProperties));

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
        /// This property is valid for <see cref="EntityRelationshipType.ChildToParent"/> relationships.
        /// This property is NOT valid for <see cref="EntityRelationshipType.ParentToChildren"/> relationship.
        /// </summary>
        public string[] ReferencingColumnProperties { get; }

        /// <summary>
        /// The property or properties on the referenced entity involved in referencing the current entity.
        /// This property is valid for <see cref="EntityRelationshipType.ParentToChildren"/> relationships.
        /// This property is NOT valid for <see cref="EntityRelationshipType.ChildToParent"/> relationship.
        /// </summary>
        public string[] ReferencedColumnProperties { get; }

        /// <summary>
        /// The property on the current entity referencing the other entity.
        /// </summary>
        public PropertyDescriptor? ReferencingNavigationProperty { get; }
    }
}
