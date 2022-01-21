namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated;

    /// <summary>
    /// Single statement options builder for a single record select.
    /// </summary>
    public interface ISelectSqlStatementOptionsBuilder<TEntity>
        : IRelationalSqlStatementOptionsSetter<TEntity, ISelectSqlStatementOptionsBuilder<TEntity>>, 
        IStandardSqlStatementOptionsSetter<TEntity, ISelectSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Single statement options builder for a single record select.
    /// </summary>
    internal class SelectSqlStatementOptionsBuilder<TEntity>
        : AggregatedSqlStatementOptionsBuilder<TEntity, ISelectSqlStatementOptionsBuilder<TEntity>>
        , ISelectSqlStatementOptionsBuilder<TEntity>
    {
        protected override ISelectSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
