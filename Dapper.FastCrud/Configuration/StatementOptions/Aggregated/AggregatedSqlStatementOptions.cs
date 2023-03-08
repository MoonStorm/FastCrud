namespace Dapper.FastCrud.Configuration.StatementOptions.Aggregated
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Formatters.Contexts;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Aggregates all the options passed on through the exposed extension methods.
    /// </summary>
    internal abstract class AggregatedSqlStatementOptions
    {
        private EntityRegistration? _entityRegistrationOverride;
        
        protected AggregatedSqlStatementOptions(EntityDescriptor entityDescriptor)
        {
            Validate.NotNull(entityDescriptor, nameof(entityDescriptor));

            this.EntityDescriptor = entityDescriptor;
            this.CommandTimeout = OrmConfiguration.DefaultSqlStatementOptions.CommandTimeout;
            this.StatementFormatter = new GenericSqlStatementFormatter();
        }

        /// <summary>
        /// Gets the map of related entity types and their relationships.
        /// </summary>
        public IList<AggregatedSqlJoinOptions> Joins { get; } = new List<AggregatedSqlJoinOptions>();

        /// <summary>
        /// Returns the entity descriptor.
        /// </summary>
        public EntityDescriptor EntityDescriptor { get; }

        /// <summary>
        /// Returns the statement formatter. This is shared with all the related entities as well.
        /// </summary>
        public GenericSqlStatementFormatter StatementFormatter { get; }

        /// <summary>
        /// Gets or sets the main entity alias.
        /// </summary>
        public string? MainEntityAlias { get; set; }

        /// <summary>
        /// Returns the main statement formatter.
        /// </summary>
        public SqlStatementFormatterResolver MainEntityFormatterResolver { get; set; }

        /// <summary>
        /// The transaction to be used by the statement.
        /// </summary>
        public IDbTransaction? Transaction { get; set; }

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
        /// Gets a timeout for the command being executed.
        /// </summary>
        public TimeSpan? CommandTimeout { get; set; }

        /// <summary>
        /// Parameters used by the statement.
        /// </summary>
        public object? Parameters { get; set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString? WhereClause { get; set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString? OrderClause { get; set; }

        /// <summary>
        /// Gets or sets a limit on the number of rows returned.
        /// </summary>
        public long? LimitResults { get; set; }

        /// <summary>
        /// Gets or sets a number of rows to be skipped.
        /// </summary>
        public long? SkipResults { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating the results should be streamed.
        /// </summary>
        public bool ForceStreamResults { get; set; }
    }
}