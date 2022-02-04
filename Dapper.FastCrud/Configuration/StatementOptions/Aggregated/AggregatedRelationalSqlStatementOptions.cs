namespace Dapper.FastCrud.Configuration.StatementOptions.Aggregated
{
    using Dapper.FastCrud.EntityDescriptors;
    using System;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System.ComponentModel;

    /// <summary>
    /// Groups together all the statement options related to a joined entity.
    /// </summary>
    internal abstract class AggregatedRelationalSqlStatementOptions
    {
        private EntityRegistration? _entityRegistrationOverride;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        protected AggregatedRelationalSqlStatementOptions(EntityDescriptor entityDescriptor)
        {
            Requires.NotNull(entityDescriptor, nameof(entityDescriptor));

            this.EntityDescriptor = entityDescriptor;
            this.JoinType = SqlJoinType.NotSpecified;
        }

        /// <summary>
        /// Returns the entity descriptor.
        /// </summary>
        public EntityDescriptor EntityDescriptor { get; }

        /// <summary>
        /// When setting this value, you're overriding the default entity used for the entity.
        /// When an override is not set, the default registration is returned.
        /// </summary>
        public EntityRegistration EntityRegistration
        {
            set => _entityRegistrationOverride = value; // can be null
            get => _entityRegistrationOverride ?? this.EntityDescriptor.CurrentEntityMappingRegistration;
        }

        /// <summary>
        /// An alias to be used for the referenced entity.
        /// </summary>
        public string? ReferencedEntityAlias { get; protected set; }

        /// <summary>
        /// An optional navigation property to be set.
        /// If this is not set, the <seealso cref="JoinOnClause"/> becomes mandatory.
        /// </summary>
        public PropertyDescriptor? ReferencingNavigationProperty { get; protected set; }

        /// <summary>
        /// If set to true, it will map the result onto the navigation property <seealso cref="ReferencingNavigationProperty"/>, if one was provided.
        /// </summary>
        public bool MapResultToNavigationProperty { get; protected set; }

        /// <summary>
        /// Gets or sets the SQL join type.
        /// </summary>
        public SqlJoinType JoinType { get; protected set; }

        /// <summary>
        /// Gets or sets the full ON clause in a JOIN.
        /// When not provided, the clause must be provided from the relationship mapping registrations.
        /// </summary>
        public FormattableString? JoinOnClause { get; protected set; }

        /// <summary>
        /// Gets or sets an extra condition on the ON clause in a JOIN.
        /// For resolving the string, a formatter linked to the JOINed entity must be used.
        /// </summary>
        [Obsolete(message:"This is here strictly for legacy purposes.", error:false)]
        public FormattableString? JoinExtraOnClause { get; protected set; }

        /// <summary>
        /// Gets or sets an extra condition for the ORDER BY clause.
        /// For resolving the string, a formatter linked to the JOINed entity must be used.
        /// </summary>
        [Obsolete(message: "This is here strictly for legacy purposes.", error: false)]
        public FormattableString? ExtraOrderClause { get; protected set; }
    }
}