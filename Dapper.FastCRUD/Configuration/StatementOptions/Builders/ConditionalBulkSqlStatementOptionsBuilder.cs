namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    /// <summary>
    /// Conditional sql options builder for a statement.
    /// </summary>
    public interface IConditionalBulkSqlStatementOptionsBuilder<TEntity>
        :IConditionalSqlStatementOptionsOptionsSetter<TEntity, IConditionalBulkSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Conditional sql options builder for a statement.
    /// </summary>
    internal class ConditionalBulkSqlStatementOptionsBuilder<TEntity> 
        : InternalSqlStatementOptions<TEntity, IConditionalBulkSqlStatementOptionsBuilder<TEntity>> 
        ,IConditionalBulkSqlStatementOptionsBuilder<TEntity>
    {
        protected override IConditionalBulkSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
