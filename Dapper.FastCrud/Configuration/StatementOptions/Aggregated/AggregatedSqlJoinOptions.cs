namespace Dapper.FastCrud.Configuration.StatementOptions.Aggregated
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Formatters.Contexts;
    using System;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System.Collections.Generic;

    /// <summary>
    /// Groups together all the statement options related to a joined entity.
    /// </summary>
    internal abstract class AggregatedSqlJoinOptions
    {
        private EntityRegistration? _referencedEntityRegistrationOverride;
        
        /// <summary>
        /// Standard constructor.
        /// </summary>
        protected AggregatedSqlJoinOptions(EntityDescriptor referencedEntityDescriptor)
        {
            Validate.NotNull(referencedEntityDescriptor, nameof(referencedEntityDescriptor));

            this.ReferencedEntityDescriptor = referencedEntityDescriptor;
        }

        /// <summary>
        /// Holds the relationships making up the JOIN.
        /// </summary>
        public IList<AggregatedSqlJoinRelationshipOptions> JoinRelationships { get; } = new List<AggregatedSqlJoinRelationshipOptions>();

        /// <summary>
        /// Returns the entity descriptor for the referenced entity.
        /// </summary>
        public EntityDescriptor ReferencedEntityDescriptor { get; }

        /// <summary>
        /// Returns the statement formatter attached to the JOINed entity.
        /// </summary>
        public SqlStatementFormatterResolver ReferencedEntityFormatterResolver { get; set; }

        /// <summary>
        /// When setting this value, you're overriding the default entity used for the entity.
        /// When an override is not set, the default registration is returned.
        /// </summary>
        public EntityRegistration ReferencedEntityRegistration
        {
            set => _referencedEntityRegistrationOverride = value; // can be null
            get => _referencedEntityRegistrationOverride ?? this.ReferencedEntityDescriptor.CurrentEntityMappingRegistration;
        }

        /// <summary>
        /// An alias to be used for the referenced entity.
        /// </summary>
        public string? ReferencedEntityAlias { get; set; }

        /// <summary>
        /// Gets or sets the SQL join type.
        /// </summary>
        public SqlJoinType JoinType { get; set; }

        /// <summary>
        /// Gets or sets the full ON clause in a JOIN.
        /// When not provided, the clause must be provided from the relationship mapping registrations.
        /// </summary>
        public FormattableString? JoinOnClause { get; set; }

        /// <summary>
        /// If set tot true, mapping relationships will be set on the navigation properties.
        /// This flag can be overriden in a specific relationship by <seealso cref="AggregatedSqlJoinRelationshipOptions.MapResults"/>.
        /// </summary>
        public bool MapResults { get; set; }

        /// <summary>
        /// Gets or sets an extra condition on the ON clause in a JOIN.
        /// For resolving the string, a formatter linked to the JOINed entity must be used.
        /// </summary>
        [Obsolete(message:"This is here strictly for legacy purposes.", error:false)]
        public FormattableString? ExtraWhereClause { get; set; }

        /// <summary>
        /// Gets or sets an extra condition for the ORDER BY clause.
        /// For resolving the string, a formatter linked to the JOINed entity must be used.
        /// </summary>
        [Obsolete(message: "This is here strictly for legacy purposes.", error: false)]
        public FormattableString? ExtraOrderClause { get; set; }
    }
}