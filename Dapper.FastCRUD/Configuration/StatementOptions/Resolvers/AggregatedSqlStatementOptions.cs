namespace Dapper.FastCrud.Configuration.StatementOptions.Resolvers
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Mappings;

    internal abstract class AggregatedSqlStatementOptions
    {
        protected AggregatedSqlStatementOptions()
        {
            this.CommandTimeout = OrmConfiguration.DefaultSqlStatementOptions.CommandTimeout;
        }

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
