namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;

    /// <summary>
    /// Ranged conditional sql statement options setter. 
    /// </summary>
    public interface IRangedConditionalSqlStatementOptionsSetter<TEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// Limits the results set by the top number of records returned.
        /// </summary>
        TStatementOptionsBuilder Top(long? topRecordsCount);

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        TStatementOptionsBuilder OrderBy(FormattableString orderByClause);

        /// <summary>
        /// Causes the result set to be streamed.
        /// </summary>
        TStatementOptionsBuilder StreamResults();

        /// <summary>
        /// Skips the initial set of results.
        /// </summary>
        TStatementOptionsBuilder Skip(long? skipRecordsCount);
    }
}
