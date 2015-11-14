namespace Dapper.FastCrud.Configuration.StatementOptions
{
    /// <summary>
    /// Conditional sql options builder for a statement.
    /// </summary>
    public interface IConditionalSqlStatementOptionsBuilder<TEntity>
        :IConditionalSqlStatementOptionsSetter<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Conditional sql options builder for a statement.
    /// </summary>
    internal class ConditionalSqlStatementOptionsBuilder<TEntity> 
        : InternalSqlStatementOptions<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>> 
        ,IConditionalSqlStatementOptionsBuilder<TEntity>
    {
        protected override IConditionalSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
