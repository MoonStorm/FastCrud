namespace Dapper.FastCrud.Configuration.StatementOptions.Aggregated
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Validations;
    using System.ComponentModel;

    /// <summary>
    /// Defines a relationship inside a JOIN.
    /// </summary>
    internal abstract class AggregatedSqlJoinRelationshipOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AggregatedSqlJoinRelationshipOptions(EntityDescriptor referencingEntityDescriptor)
        {
            Validate.NotNull(referencingEntityDescriptor, nameof(referencingEntityDescriptor));

            this.ReferencingEntityDescriptor = referencingEntityDescriptor;
        }

        /// <summary>
        /// The referencing entity descriptor.
        /// </summary>
        public EntityDescriptor ReferencingEntityDescriptor { get; set; }

        /// <summary>
        /// The referencing entity alias.
        /// </summary>
        public string? ReferencingEntityAlias { get; set; }

        /// <summary>
        /// The navigation property on the referencing entity.
        /// </summary>
        public PropertyDescriptor? ReferencingNavigationProperty { get; set;  }

        /// <summary>
        /// The navigation property on the referenced property.
        /// </summary>
        public PropertyDescriptor? ReferencedNavigationProperty { get; set;  }

        /// <summary>
        /// If set to true, the results are set on <see cref="ReferencingNavigationProperty"/> and <see cref="ReferencedNavigationProperty"/>.
        /// </summary>
        public bool? MapResults { get; set; }
    }
}
