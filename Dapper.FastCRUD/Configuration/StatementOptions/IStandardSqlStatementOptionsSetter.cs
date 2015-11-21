namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Standard sql options setter for a statement.
    /// </summary>
    public interface IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Enforces a maximum time span on the current command.
        /// </summary>
        TStatementOptionsSetter WithTimeout(TimeSpan? commandTimeout);

        /// <summary>
        /// Attaches the current command to an existing transaction.
        /// </summary>
        TStatementOptionsSetter AttachToTransaction(IDbTransaction transaction);

        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        TStatementOptionsSetter WithEntityMappingOverride(EntityMapping<TEntity> entityMapping);
    }
}
