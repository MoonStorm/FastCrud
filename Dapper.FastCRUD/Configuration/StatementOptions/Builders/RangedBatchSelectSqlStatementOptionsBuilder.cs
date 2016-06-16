namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    public interface IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
        :IRangedConditionalSqlStatementOptionsSetter<TEntity, IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>>,
        IRelationalSqlStatementOptionsSetter<TEntity, IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    internal class RangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
        : AggregatedSqlStatementOptionsBuilder<TEntity, IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>>, 
        IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
    {
        protected override IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity> Builder => this;
    }
}
