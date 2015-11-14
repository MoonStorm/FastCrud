namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Conditional sql statement options setter. 
    /// </summary>
    public interface IConditionalSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        : IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        where TStatementOptionsSetter : IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        TStatementOptionsSetter Where(FormattableString whereClause);

        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        TStatementOptionsSetter WithParameters(object parameters);
    }
}
