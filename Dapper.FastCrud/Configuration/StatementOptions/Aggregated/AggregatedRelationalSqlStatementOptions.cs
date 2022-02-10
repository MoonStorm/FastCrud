namespace Dapper.FastCrud.Configuration.StatementOptions.Aggregated
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters.Contexts;
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
        protected AggregatedRelationalSqlStatementOptions(
            EntityDescriptor referencingEntityDescriptor,
            EntityDescriptor referencedEntityDescriptor
            )
        {
            Requires.NotNull(referencingEntityDescriptor, nameof(referencingEntityDescriptor));
            Requires.NotNull(referencedEntityDescriptor, nameof(referencedEntityDescriptor));

            this.ReferencingEntityDescriptor = referencingEntityDescriptor;
            this.ReferencedEntityDescriptor = referencedEntityDescriptor;
            this.JoinType = SqlJoinType.NotSpecified;
        }

        /// <summary>
        /// Returns the entity descriptor for the referencing entity.
        /// </summary>
        public EntityDescriptor ReferencingEntityDescriptor { get; }

        /// <summary>
        /// Returns the entity descriptor for the referenced entity.
        /// </summary>
        public EntityDescriptor ReferencedEntityDescriptor { get; }

        /// <summary>
        /// When setting this value, you're overriding the default entity used for the entity.
        /// When an override is not set, the default registration is returned.
        /// </summary>
        public EntityRegistration ReferencedEntityRegistration
        {
            set => _entityRegistrationOverride = value; // can be null
            get => _entityRegistrationOverride ?? this.ReferencedEntityDescriptor.CurrentEntityMappingRegistration;
        }

        /// <summary>
        /// The formatter resolver associated with the referenced entity.
        /// </summary>
        public SqlStatementFormatterResolver ReferencedEntityFormatterResolver { get; set; }

        /// <summary>
        /// The formatter resolver associated with the referencing entity.
        /// </summary>
        public SqlStatementFormatterResolver ReferencingEntityFormatterResolver { get; set; }

        /// <summary>
        /// An alias the referencing entity is known by in the current statement.
        /// </summary>
        public string? ReferencingEntityAlias { get; set; }

        /// <summary>
        /// An alias to be used for the referenced entity.
        /// </summary>
        public string? ReferencedEntityAlias { get; set; }

        /// <summary>
        /// An optional navigation property to be set that represents the navigation property on the referenced entity side (either a collection or an entity type property).
        /// Note that if not enough information is provided, the <seealso cref="JoinOnClause"/> becomes mandatory.
        /// </summary>
        public PropertyDescriptor? ReferencedNavigationProperty { get; set; }

        /// <summary>
        /// An optional navigation property to be set that represents the navigation property on the referencing entity side (either a collection or an entity type property).
        /// Note that if not enough information is provided, the <seealso cref="JoinOnClause"/> becomes mandatory.
        /// </summary>
        public PropertyDescriptor? ReferencingNavigationProperty { get; set; }

        /// <summary>
        /// If set to true, it will map the result onto both the navigation property <seealso cref="ReferencedNavigationProperty"/>,
        ///   if one was provided, but also on the corresponding referencing navigation property.
        /// </summary>
        public bool MapResultToNavigationProperties { get; set; }

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