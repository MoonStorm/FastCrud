namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;

    internal abstract class InternalSqlStatementOptions<TEntity,TStatementOptionsBuilder>
        :ISqlStatementOptionsGetter
        where TStatementOptionsBuilder: class, IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsBuilder>
    {
        protected InternalSqlStatementOptions()
        {
            this.CommandTimeout = OrmConfiguration.DefaultSqlStatementOptions.CommandTimeout;
        }

        /// <summary>
        /// The transaction to be used by the statement.
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        /// <summary>
        /// The entity mapping override to be used for the main entity.
        /// </summary>
        public EntityMapping EntityMappingOverride { get; private set; }

        /// <summary>
        /// Gets a timeout for the command being executed.
        /// </summary>
        public TimeSpan? CommandTimeout { get; private set; }

        /// <summary>
        /// Parameters used by the statement.
        /// </summary>
        public object Parameters { get; private set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString WhereClause { get; private set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString OrderClause { get; set; }

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

        /// <summary>
        /// Limits the results set by the top number of records returned.
        /// </summary>
        public TStatementOptionsBuilder Top(long topRecords)
        {
            Requires.Argument(topRecords > 0, nameof(topRecords), "The top record count must be a positive value");

            this.LimitResults = topRecords;
            return this.Builder;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        public TStatementOptionsBuilder OrderBy(FormattableString orderByClause)
        {
            Requires.NotNull(orderByClause, nameof(orderByClause));

            this.OrderClause = orderByClause;
            return this.Builder;
        }

        /// <summary>
        /// Skips the initial set of results.
        /// </summary>
        public TStatementOptionsBuilder Skip(long skipRecordsCount)
        {
            Requires.Argument(skipRecordsCount > 0, nameof(skipRecordsCount), "The number of records to skip must be a positive value");

            this.SkipResults = skipRecordsCount;
            return this.Builder;
        }

        /// <summary>
        /// Causes the result set to be streamed.
        /// </summary>
        public TStatementOptionsBuilder StreamResults()
        {
            this.ForceStreamResults = true;
            return this.Builder;
        }

        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        public TStatementOptionsBuilder Where(FormattableString whereClause)
        {
            Requires.NotNull(whereClause, nameof(whereClause));

            this.WhereClause = whereClause;
            return this.Builder;
        }

        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        public TStatementOptionsBuilder WithParameters(object parameters)
        {
            Requires.NotNull(parameters, nameof(parameters));

            this.Parameters = parameters;
            return this.Builder;
        }

        /// <summary>
        /// Enforces a maximum time span on the current command.
        /// </summary>
        public TStatementOptionsBuilder WithTimeout(TimeSpan commandTimeout)
        {
            Requires.NotDefault(commandTimeout, nameof(commandTimeout));

            this.CommandTimeout = commandTimeout;
            return this.Builder;
        }

        /// <summary>
        /// Attaches the current command to an existing transaction.
        /// </summary>
        public TStatementOptionsBuilder AttachToTransaction(IDbTransaction transaction)
        {
            Requires.NotNull(transaction, nameof(transaction));

            this.Transaction = transaction;
            return this.Builder;
        }

        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TEntity> entityMapping)
        {
            Requires.NotNull(entityMapping, nameof(entityMapping));

            this.EntityMappingOverride = entityMapping;
            return this.Builder;
        }

        protected abstract TStatementOptionsBuilder Builder { get; }
    }
}
