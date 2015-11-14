namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;

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
}
