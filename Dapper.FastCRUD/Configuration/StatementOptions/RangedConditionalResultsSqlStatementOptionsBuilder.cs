namespace Dapper.FastCrud.Configuration.StatementOptions
{
    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    public interface IRangedConditionalResultsSqlStatementOptionsBuilder<TEntity>
        :IRangedConditionalResultsSqlStatementSetter<TEntity, IRangedConditionalResultsSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    internal class RangedConditionalResultsSqlStatementOptionsBuilder<TEntity>
        : InternalSqlStatementOptions<TEntity, IRangedConditionalResultsSqlStatementOptionsBuilder<TEntity>>
        , IRangedConditionalResultsSqlStatementOptionsBuilder<TEntity>
    {
        protected override IRangedConditionalResultsSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
