namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    public interface ISelectSqlStatementOptionsBuilder<TEntity>
        : IRelationalStatementOptionsSetter<TEntity, ISelectSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Ranged conditional sql options builder for a statement.
    /// </summary>
    internal class SelectSqlStatementOptionsBuilder<TEntity>
        : AggregatedSqlStatementOptionsBuilder<TEntity, ISelectSqlStatementOptionsBuilder<TEntity>>
        , ISelectSqlStatementOptionsBuilder<TEntity>
    {
        protected override ISelectSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
