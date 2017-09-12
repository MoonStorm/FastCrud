namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;

    /// <summary>
    /// Conditional sql statement options setter. 
    /// </summary>
    public interface IConditionalSqlStatementOptionsOptionsSetter<TEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        TStatementOptionsBuilder Where(FormattableString whereClause);
    }
}
