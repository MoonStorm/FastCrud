namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated;

    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    public interface ISqlJoinOptionsBuilder<TReferencedEntity>
        : ISqJoinOptionsSetter<TReferencedEntity, ISqlJoinOptionsBuilder<TReferencedEntity>>
    {
    }

    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    internal class SqlJoinOptionsBuilder<TReferencedEntity> 
        : AggregatedSqlStatementJoinOptionsBuilder<TReferencedEntity, ISqlJoinOptionsBuilder<TReferencedEntity>>, 
          ISqlJoinOptionsBuilder<TReferencedEntity>
    {
        protected override ISqlJoinOptionsBuilder<TReferencedEntity> Builder => this;
    }
}