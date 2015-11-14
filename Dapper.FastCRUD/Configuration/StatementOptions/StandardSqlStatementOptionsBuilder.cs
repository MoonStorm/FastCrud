namespace Dapper.FastCrud.Configuration.StatementOptions
{
    /// <summary>
    /// Standard sql options builder for a statement.
    /// </summary>
    public interface IStandardSqlStatementOptionsBuilder<TEntity>
        :IStandardSqlStatementOptionsSetter<TEntity,IStandardSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Standard sql options builder for a statement.
    /// </summary>
    internal class StandardSqlStatementOptionsBuilder<TEntity>
        : InternalSqlStatementOptions<TEntity, IStandardSqlStatementOptionsBuilder<TEntity>>
        , IStandardSqlStatementOptionsBuilder<TEntity>
    {
        protected override IStandardSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
