namespace Dapper.FastCrud.Mappings
{
    using System;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Holds details about a relationship
    /// </summary>
    public class PropertyMappingForeignKeyRelationship
    {
        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="referencedEntityType">The entity type referenced in the foreign key relationship.</param>
        public PropertyMappingForeignKeyRelationship(Type referencedEntityType)
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
        public PropertyMappingForeignKeyRelationship(string referencingPropertyName, Type referencedEntityType)
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
