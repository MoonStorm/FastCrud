namespace Dapper.FastCrud.Configuration.StatementOptions
{
    /// <summary>
    /// Parameterized SQL statement options setter.
    /// </summary>
    public interface IParameterizedSqlStatementOptionsSetter<TEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        TStatementOptionsBuilder WithParameters(object parameters);
    }
}
