namespace Dapper.FastCrud.Mappings
{
    using System;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Holds details about a relationship
    /// </summary>
    public class PropertyMappingRelationship
    {
        /// <summary>
        /// Constructor. BLOCKED FOR THE TIME BEING
        /// </summary>
        /// <param name="referencedEntityType">The entity type referenced in the foreign key relationship.</param>
        public PropertyMappingRelationship(Type referencedEntityType)
        {
            Requires.NotNull(referencedEntityType, nameof(referencedEntityType));

            this.ReferencedEntityType = referencedEntityType;
        }

        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="referencingPropertyName">
        /// The property of type <paramref name="referencedEntityType"/> 
        ///  that would hold the foreign entity on SELECTs, when the statement was instructed to.
        /// </param>
        /// <param name="referencedEntityType">The entity type referenced in the foreign key relationship.</param>
        public PropertyMappingRelationship(Type referencedEntityType, string referencingPropertyName)
            :this(referencedEntityType)
        {
            Requires.NotNullOrWhiteSpace(referencingPropertyName, nameof(referencingPropertyName));

            this.ReferencingPropertyName = referencingPropertyName;
        }

        /// <summary>
        /// Gets the referenced entity type.
        /// </summary>
        public Type ReferencedEntityType { get; }

        /// <summary>
        /// Gets the referencing property name. It might return null if not provided.
        /// </summary>
        public string ReferencingPropertyName { get; }
    }
}
