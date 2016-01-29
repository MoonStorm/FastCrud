namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    public interface IRangedBatchSelectSqlStatementOptionsOptionsBuilder<TEntity>
        :IRangedConditionalSqlStatementOptionsSetter<TEntity, IRangedBatchSelectSqlStatementOptionsOptionsBuilder<TEntity>>,
        IRelationalStatementOptionsSetter<TEntity, IRangedBatchSelectSqlStatementOptionsOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    internal class RangedBatchSelectSqlStatementOptionsOptionsBuilder<TEntity>
        : AggregatedSqlStatementOptionsBuilder<TEntity, IRangedBatchSelectSqlStatementOptionsOptionsBuilder<TEntity>>, 
        IRangedBatchSelectSqlStatementOptionsOptionsBuilder<TEntity>
    {
        protected override IRangedBatchSelectSqlStatementOptionsOptionsBuilder<TEntity> Builder => this;
    }
}
