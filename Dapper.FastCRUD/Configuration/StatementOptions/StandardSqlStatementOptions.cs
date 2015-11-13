namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Standard sql options for a statement.
    /// </summary>
    internal interface IStandardSqlStatementOptionsGetter
    {
        /// <summary>
        /// The transaction to be used by the statement.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// The entity mapping override to be used for the main entity.
        /// </summary>
        EntityMapping EntityMappingOverride { get;}

        /// <summary>
        /// Gets a timeout for the command being executed.
        /// </summary>
        TimeSpan? CommandTimeout { get; }
    }

    /// <summary>
    /// Standard sql options setter for a statement.
    /// </summary>
    public interface IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        where TStatementOptionsSetter : IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Enforces a maximum time span on the current command.
        /// </summary>
        TStatementOptionsSetter WithTimeout(TimeSpan commandTimeout);

        /// <summary>
        /// Attaches the current command to an existing transaction.
        /// </summary>
        TStatementOptionsSetter AttachToTransaction(IDbTransaction transaction);

        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        TStatementOptionsSetter WithEntityMappingOverride(EntityMapping<TEntity> entityMapping);
    }

    /// <summary>
    /// Standard sql options for a statement.
    /// </summary>
    internal class StandardSqlStatementOptions<TEntity, TStatementOptionsSetter> 
        : IStandardSqlStatementOptionsGetter, 
        IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        where TStatementOptionsSetter : class, IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        protected StandardSqlStatementOptions()
        {
            this.CommandTimeout = OrmConfiguration.DefaultSqlStatementOptions.CommandTimeout;
        }

        /// <summary>
        /// The transaction to be used by the statement.
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        /// <summary>
        /// The entity mapping override to be used for the main entity.
        /// </summary>
        public EntityMapping EntityMappingOverride { get; private set; }

        /// <summary>
        /// Gets a timeout for the command being executed.
        /// </summary>
        public TimeSpan? CommandTimeout { get; private set; }

        /// <summary>
        /// Enforces a maximum time span on the current command.
        /// </summary>
        public TStatementOptionsSetter WithTimeout(TimeSpan commandTimeout)
        {
            Requires.NotDefault(commandTimeout, nameof(commandTimeout));

            this.CommandTimeout = commandTimeout;
            return this as TStatementOptionsSetter;
        }

        /// <summary>
        /// Attaches the current command to an existing transaction.
        /// </summary>
        public TStatementOptionsSetter AttachToTransaction(IDbTransaction transaction)
        {
            Requires.NotNull(transaction, nameof(transaction));

            this.Transaction = transaction;
            return  this as TStatementOptionsSetter;
        }

        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        public TStatementOptionsSetter WithEntityMappingOverride(EntityMapping<TEntity> entityMapping)
        {
            Requires.NotNull(entityMapping, nameof(entityMapping));

            this.EntityMappingOverride = entityMapping;
            return this as TStatementOptionsSetter;
        }
    }

    /// <summary>
    /// Standard sql options builder.
    /// </summary>
    public interface IStandardSqlStatementOptionsBuilder<TEntity> :
        IStandardSqlStatementOptionsSetter<TEntity, IStandardSqlStatementOptionsBuilder<TEntity>>
    {

    }

    /// <summary>
    /// Standard sql options builder.
    /// </summary>
    internal class StandardSqlStatementOptionsBuilder<TEntity> :
        StandardSqlStatementOptions<TEntity, IStandardSqlStatementOptionsBuilder<TEntity>>,
        IStandardSqlStatementOptionsBuilder<TEntity>
    {        
    }
}
