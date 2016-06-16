namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    public interface ISelectSqlSqlStatementOptionsBuilder<TEntity>
        : IRelationalSqlStatementOptionsSetter<TEntity, ISelectSqlSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    internal class SelectSqlSqlStatementOptionsBuilder<TEntity>
        : AggregatedSqlStatementOptionsBuilder<TEntity, ISelectSqlSqlStatementOptionsBuilder<TEntity>>
        , ISelectSqlSqlStatementOptionsBuilder<TEntity>
    {
        protected override ISelectSqlSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
