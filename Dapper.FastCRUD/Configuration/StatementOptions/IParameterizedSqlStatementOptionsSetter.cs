namespace Dapper.FastCrud.Configuration.StatementOptions
{
    /// <summary>
    /// Parameterized SQL statement options setter.
    /// </summary>
    public interface IParameterizedSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        :IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        TStatementOptionsSetter WithParameters(object parameters);
    }
}
