namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Standard sql options setter for a statement.
    /// </summary>
    public interface IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// Enforces a maximum time span on the current command.
        /// </summary>
        TStatementOptionsBuilder WithTimeout(TimeSpan? commandTimeout);

        /// <summary>
        /// Attaches the current command to an existing transaction.
        /// </summary>
        TStatementOptionsBuilder AttachToTransaction(IDbTransaction transaction);

        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TEntity> entityMapping);
    }
}
