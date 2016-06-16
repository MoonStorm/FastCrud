namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    public interface ISqlRelationOptionsBuilder<TReferencedEntity>
        : ISqlRelationOptionsSetter<TReferencedEntity, ISqlRelationOptionsBuilder<TReferencedEntity>>
    {
    }

    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    internal class SqlRelationOptionsBuilder<TReferencedEntity> 
        : AggregatedRelationalSqlStatementOptionsBuilder<TReferencedEntity, ISqlRelationOptionsBuilder<TReferencedEntity>>, ISqlRelationOptionsBuilder<TReferencedEntity>
    {
        protected override ISqlRelationOptionsBuilder<TReferencedEntity> Builder => this;
    }
}