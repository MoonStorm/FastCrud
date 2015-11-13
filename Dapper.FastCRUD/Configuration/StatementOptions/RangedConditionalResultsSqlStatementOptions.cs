namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Sql options for a statement where the results are ordered or restricted.
    /// </summary>
    internal interface IRangedConditionalResultsSqlStatementGetter: IConditionalSqlStatementOptionsGetter
    {
        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        FormattableString OrderClause { get; set; }

        /// <summary>
        /// Gets or sets a limit on the number of rows returned.
        /// </summary>
        long? LimitResults { get; set; }

        /// <summary>
        /// Gets or sets a number of rows to be skipped.
        /// </summary>
        long? SkipResults { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating the results should be streamed.
        /// </summary>
        bool ForceStreamResults { get; set; }
    }

    /// <summary>
    /// Ranged conditional sql statement options setter. 
    /// </summary>
    public interface IRangedConditionalResultsSqlStatementSetter<TEntity, TStatementOptionsSetter>
        : IConditionalSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        where TStatementOptionsSetter : IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Limits the results set by the top number of records returned.
        /// </summary>
        TStatementOptionsSetter Top(long topRecordsCount);

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        TStatementOptionsSetter OrderBy(FormattableString orderByClause);

        /// <summary>
        /// Causes the result set to be streamed.
        /// </summary>
        TStatementOptionsSetter StreamResults();

        /// <summary>
        /// Skips the initial set of results.
        /// </summary>
        TStatementOptionsSetter Skip(long skipRecordsCount);
    }

    /// <summary>
    /// Ranged conditional sql statement options. 
    /// </summary>
    internal class RangedConditionalResultsSqlStatementOptions<TEntity, TStatementOptionsSetter>
        : ConditionalSqlStatementOptions<TEntity, TStatementOptionsSetter>,
        IRangedConditionalResultsSqlStatementGetter,
        IRangedConditionalResultsSqlStatementSetter<TEntity, TStatementOptionsSetter>
        where TStatementOptionsSetter : class, IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
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
        public TStatementOptionsSetter Top(long topRecords)
        {
            Requires.Argument(topRecords>0, nameof(topRecords),"The top record count must be a positive value");

            this.LimitResults = topRecords;
            return this as TStatementOptionsSetter;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        public TStatementOptionsSetter OrderBy(FormattableString orderByClause)
        {
            Requires.NotNull(orderByClause, nameof(orderByClause));

            this.OrderClause = orderByClause;
            return this as TStatementOptionsSetter;
        }

        /// <summary>
        /// Skips the initial set of results.
        /// </summary>
        public TStatementOptionsSetter Skip(long skipRecordsCount)
        {
            Requires.Argument(skipRecordsCount > 0, nameof(skipRecordsCount), "The number of records to skip must be a positive value");

            this.SkipResults = skipRecordsCount;
            return this as TStatementOptionsSetter;
        }

        /// <summary>
        /// Causes the result set to be streamed.
        /// </summary>
        public TStatementOptionsSetter StreamResults()
        {
            this.ForceStreamResults = true;
            return this as TStatementOptionsSetter;
        }
    }

    /// <summary>
    /// Ranged conditional sql statement options builder.
    /// </summary>
    public interface IRangedConditionalResultsSqlStatementBuilder<TEntity> :
        IRangedConditionalResultsSqlStatementSetter<TEntity, IRangedConditionalResultsSqlStatementBuilder<TEntity>>
    {

    }

    /// <summary>
    /// Ranged conditional sql statement options builder. 
    /// </summary>
    internal class RangedConditionalResultsSqlStatementBuilder<TEntity> :
        RangedConditionalResultsSqlStatementOptions<TEntity, IRangedConditionalResultsSqlStatementBuilder<TEntity>>,
        IRangedConditionalResultsSqlStatementBuilder<TEntity>
    {
    }

}
