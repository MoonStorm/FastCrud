namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated;

    /// <summary>
    /// Single statement options builder for a single record select.
    /// </summary>
    public interface ISelectSqlSqlStatementOptionsBuilder<TEntity>
        : IRelationalSqlStatementOptionsSetter<TEntity, ISelectSqlSqlStatementOptionsBuilder<TEntity>>, 
        IStandardSqlStatementOptionsSetter<TEntity, ISelectSqlSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Single statement options builder for a single record select.
    /// </summary>
    internal class SelectSqlSqlStatementOptionsBuilder<TEntity>
        : AggregatedSqlStatementOptionsBuilder<TEntity, ISelectSqlSqlStatementOptionsBuilder<TEntity>>
        , ISelectSqlSqlStatementOptionsBuilder<TEntity>
    {
        protected override ISelectSqlSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
