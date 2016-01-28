namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Configuration.StatementOptions.Resolvers;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;

    internal abstract class AggregatedSqlStatementOptionsBuilder<TEntity, TStatementOptionsBuilder>:AggregatedSqlStatementOptions
    {
        /// <summary>
        /// Limits the results set by the top number of records returned.
        /// </summary>
        public TStatementOptionsBuilder Top(long? topRecords)
        {
            Requires.Argument(topRecords == null || topRecords > 0, nameof(topRecords), "The top record count must be a positive value");

            this.LimitResults = topRecords;
            return this.Builder;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        public TStatementOptionsBuilder OrderBy(FormattableString orderByClause)
        {
            this.OrderClause = orderByClause;
            return this.Builder;
        }

        /// <summary>
        /// Skips the initial set of results.
        /// </summary>
        public TStatementOptionsBuilder Skip(long? skipRecordsCount)
        {
            Requires.Argument(skipRecordsCount == null || skipRecordsCount >= 0, nameof(skipRecordsCount), "The number of records to skip must be a positive value");

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
            this.WhereClause = whereClause;
            return this.Builder;
        }

        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        public TStatementOptionsBuilder WithParameters(object parameters)
        {
            this.Parameters = parameters;
            return this.Builder;
        }

        /// <summary>
        /// Enforces a maximum time span on the current command.
        /// </summary>
        public TStatementOptionsBuilder WithTimeout(TimeSpan? commandTimeout)
        {
            this.CommandTimeout = commandTimeout;
            return this.Builder;
        }

        /// <summary>
        /// Attaches the current command to an existing transaction.
        /// </summary>
        public TStatementOptionsBuilder AttachToTransaction(IDbTransaction transaction)
        {
            this.Transaction = transaction;
            return this.Builder;
        }

        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TEntity> entityMapping)
        {
            this.EntityMappingOverride = entityMapping;
            return this.Builder;
        }

        /// <summary>
        /// Includes a referred entity into the query. The relationship must be set up prior to calling this method.
        /// </summary>
        public TStatementOptionsBuilder Include<TReferredEntity>(Func<ISqlRelationOptionsSetter<TEntity, TReferredEntity>, ISqlRelationOptionsSetter<TEntity, TReferredEntity>> options = null)
        {
            throw new NotImplementedException();
        }

        protected abstract TStatementOptionsBuilder Builder { get; }

    }
}
