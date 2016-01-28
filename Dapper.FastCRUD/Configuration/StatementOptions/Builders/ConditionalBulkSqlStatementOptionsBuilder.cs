namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using Dapper.FastCrud.Configuration.StatementOptions.Resolvers;

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
        : AggregatedSqlStatementOptionsBuilder<TEntity, IConditionalBulkSqlStatementOptionsBuilder<TEntity>> 
        ,IConditionalBulkSqlStatementOptionsBuilder<TEntity>
    {
        protected override IConditionalBulkSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
