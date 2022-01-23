namespace Dapper.FastCrud.Mappings
{
    using System;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Holds details about a child-parent relationship
    /// </summary>
    public class PropertyMappingRelationship
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        internal PropertyMappingRelationship(Type referencedEntityType, 
                                             string? referencingParentEntityPropertyName, 
                                             string? referencingChildrenCollectionPropertyName)
        {
            Requires.NotNull(referencedEntityType, nameof(referencedEntityType));

            ReferencedEntityType = referencedEntityType;
            ReferencingParentEntityPropertyName = referencingParentEntityPropertyName;
            ReferencingChildrenCollectionPropertyName = referencingChildrenCollectionPropertyName;
        }

        /// <summary>
        /// Gets the referenced parent entity type.
        /// </summary>
        public Type ReferencedEntityType { get; }

        /// <summary>
        /// Gets the name of the property on the current entity holding the parent entity.
        /// This can be optionally provided via ForeignKey attribute in the child entity or via fluent mapping.
        /// </summary>
        public string? ReferencingParentEntityPropertyName { get; }

        /// <summary>
        /// Gets the name of the property on the referenced entity holding the children entities.
        /// This can be optionally provided via InverseProperty attribute in the parent entity or via fluent mapping.
        /// </summary>
        public string? ReferencingChildrenCollectionPropertyName { get; }

    }
}
