namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated;

    /// <summary>
    /// Statement sql options builder for a conditional statement.
    /// </summary>
    public interface IConditionalSqlStatementOptionsBuilder<TEntity>
        : IStandardSqlStatementOptionsSetter<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>>,
        IConditionalSqlStatementOptionsOptionsSetter<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>>,
        IRelationalSqlStatementOptionsSetter<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>>,
        IParameterizedSqlStatementOptionsSetter<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>>
    {
    }

    /// <summary>
    /// Statement sql options builder for a conditional statement.
    /// </summary>
    internal class ConditionalSqlStatementOptionsBuilder<TEntity>
        : AggregatedSqlStatementOptionsBuilder<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>>
        , IConditionalSqlStatementOptionsBuilder<TEntity>
    {
        protected override IConditionalSqlStatementOptionsBuilder<TEntity> Builder => this;
    }
}
