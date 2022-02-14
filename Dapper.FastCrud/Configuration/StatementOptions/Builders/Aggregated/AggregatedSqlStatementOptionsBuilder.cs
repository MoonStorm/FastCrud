namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;
    using System.Linq;

    /// <summary>
    /// The full options builder for main queries.
    /// Note that the publicly available builder will use a subset of this functionality and will vary depending on usage.
    /// </summary>
    internal abstract class AggregatedSqlStatementOptionsBuilder<TEntity, TStatementOptionsBuilder> : AggregatedSqlStatementOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AggregatedSqlStatementOptionsBuilder()
        :base(OrmConfiguration.GetEntityDescriptor<TEntity>())
        {
            // change these lines and you might want to check AggregatedSqlStatementOptionsBuilder.WithEntityMappingOverride as well
            this.MainEntityFormatterResolver = this.StatementFormatter.RegisterResolver(this.EntityDescriptor, this.EntityRegistration, null);
            this.StatementFormatter.SetActiveMainResolver(this.MainEntityFormatterResolver, false);
        }

        /// <summary>
        /// Provides the builder used in constructing the options.
        /// </summary>
        protected abstract TStatementOptionsBuilder Builder { get; }

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
        /// Sets up an alias for the main entity to be used in a relationship.
        /// It is recommended to add aliases to the joined entities as well.
        /// </summary>
        public TStatementOptionsBuilder WithAlias(string? mainEntityAlias)
        {
            var oldAlias = this.MainEntityAlias;
            this.MainEntityAlias = mainEntityAlias;

            this.MainEntityFormatterResolver = this.StatementFormatter.ReplaceRegisteredResolver(
                this.EntityDescriptor, 
                this.EntityRegistration, 
                oldAlias, 
                this.EntityRegistration, 
                this.MainEntityAlias);
            this.StatementFormatter.SetActiveMainResolver(this.MainEntityFormatterResolver, false);
            return this.Builder;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        public TStatementOptionsBuilder OrderBy(FormattableString? orderByClause)
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
        public TStatementOptionsBuilder StreamResults(bool streamResults = true)
        {
            this.ForceStreamResults = streamResults;
            return this.Builder;
        }

        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        public TStatementOptionsBuilder Where(FormattableString? whereClause)
        {
            this.WhereClause = whereClause;
            return this.Builder;
        }

        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        public TStatementOptionsBuilder WithParameters(object? parameters)
        {
            this.Parameters = parameters;
            return this.Builder;
        }

        /// <summary>
        /// Has no effect on the statement builder.
        /// Provided in case you want to include conditional options.
        /// </summary>
        public TStatementOptionsBuilder NoOp()
        {
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
        public TStatementOptionsBuilder AttachToTransaction(IDbTransaction? transaction)
        {
            this.Transaction = transaction;
            return this.Builder;
        }

        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TEntity>? entityMapping)
        {
            // change these lines and you might want to check the constructor of AggregatedSqlStatementOptions
            var oldRegistration = this.EntityRegistration;
            this.EntityRegistration = entityMapping?.Registration!;
            this.MainEntityFormatterResolver = this.StatementFormatter.ReplaceRegisteredResolver(
                this.EntityDescriptor, 
                oldRegistration, 
                this.MainEntityAlias, 
                this.EntityRegistration, 
                this.MainEntityAlias);
            this.StatementFormatter.SetActiveMainResolver(this.MainEntityFormatterResolver, false);
            return this.Builder;
        }


        /// <summary>
        /// Includes a referred entity into the query. The relationship and the associated mappings must be set up prior to calling this method.
        /// </summary>
        public TStatementOptionsBuilder Include<TReferencedEntity>(Action<ISqlJoinOptionsBuilder<TReferencedEntity>>? join = null)
        {
            var joinOptionsBuilder = new SqlJoinOptionsBuilder<TReferencedEntity>();
            joinOptionsBuilder.LeftOuterJoin();
            joinOptionsBuilder.MapResults(true);

            join?.Invoke(joinOptionsBuilder);

            //perform a validation prior to adding the join
            joinOptionsBuilder.ReferencedEntityFormatterResolver = this.StatementFormatter.RegisterResolver(
                joinOptionsBuilder.ReferencedEntityDescriptor,
                joinOptionsBuilder.ReferencedEntityRegistration,
                joinOptionsBuilder.ReferencedEntityAlias);
            this.Joins.Add(joinOptionsBuilder);

            return this.Builder;
        }

    }
}
