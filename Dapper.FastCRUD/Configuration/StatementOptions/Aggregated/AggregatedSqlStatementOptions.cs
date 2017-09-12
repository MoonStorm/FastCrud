namespace Dapper.FastCrud.Configuration.StatementOptions.Aggregated
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.SqlStatements;

    /// <summary>
    /// Aggregates all the options passed on through the exposed extension methods.
    /// </summary>
    internal abstract class AggregatedSqlStatementOptions<TEntity>
    {
        protected AggregatedSqlStatementOptions()
        {
            this.CommandTimeout = OrmConfiguration.DefaultSqlStatementOptions.CommandTimeout;
            this.RelationshipOptions = new Dictionary<Type, AggregatedRelationalSqlStatementOptions>();
            this.SqlStatementsFactoryChain = () => OrmConfiguration.GetSqlStatements<TEntity>(this.EntityMappingOverride);
        }

        /// <summary>
        /// Gets the map of related entity types and their relationships.
        /// </summary>
        public Dictionary<Type, AggregatedRelationalSqlStatementOptions> RelationshipOptions { get; }

        /// <summary>
        /// The factory used to create the sql statement factories.
        /// </summary>
        public Func<ISqlStatements<TEntity>> SqlStatementsFactoryChain { get; protected set; }

        /// <summary>
        /// The transaction to be used by the statement.
        /// </summary>
        public IDbTransaction Transaction { get; protected set; }

        /// <summary>
        /// The entity mapping override to be used for the main entity.
        /// </summary>
        public EntityMapping EntityMappingOverride { get; protected set; }

        /// <summary>
        /// Gets a timeout for the command being executed.
        /// </summary>
        public TimeSpan? CommandTimeout { get; protected set; }

        /// <summary>
        /// Parameters used by the statement.
        /// </summary>
        public object Parameters { get; protected set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString WhereClause { get; protected set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString OrderClause { get; protected set; }

        /// <summary>
        /// Gets or sets a limit on the number of rows returned.
        /// </summary>
        public long? LimitResults { get; protected set; }

        /// <summary>
        /// Gets or sets a number of rows to be skipped.
        /// </summary>
        public long? SkipResults { get; protected set; }

        /// <summary>
        /// Gets or sets a flag indicating the results should be streamed.
        /// </summary>
        public bool ForceStreamResults { get; protected set; }
    }
}