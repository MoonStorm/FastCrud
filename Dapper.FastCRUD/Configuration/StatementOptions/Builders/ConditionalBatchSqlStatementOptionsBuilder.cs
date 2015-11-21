namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    /// <summary>
    /// Conditional sql options builder for a statement.
    /// </summary>
    public interface IConditionalBatchSqlStatementOptionsBuilder<TEntity>
        :IConditionalSqlStatementOptionsOptionsSetter<TEntity, IConditionalBatchSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Conditional sql options builder for a statement.
    /// </summary>
    internal class ConditionalBatchBatchSqlStatementOptionsBuilder<TEntity> 
        : InternalSqlStatementOptions<TEntity, IConditionalBatchSqlStatementOptionsBuilder<TEntity>> 
        ,IConditionalBatchSqlStatementOptionsBuilder<TEntity>
    {
        protected override IConditionalBatchSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
