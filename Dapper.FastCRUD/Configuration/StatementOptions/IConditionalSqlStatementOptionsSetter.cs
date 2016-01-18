namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;

    /// <summary>
    /// Conditional sql statement options setter. 
    /// </summary>
    public interface IConditionalSqlStatementOptionsOptionsSetter<TEntity, TStatementOptionsSetter>
        :IParameterizedSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        TStatementOptionsSetter Where(FormattableString whereClause);
    }
}
