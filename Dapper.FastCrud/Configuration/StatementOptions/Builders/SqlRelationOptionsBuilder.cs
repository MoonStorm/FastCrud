namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated;

    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    public interface ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>
        : ISqlRelationOptionsSetter<TReferencedEntity, ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>
    {
    }

    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    internal class SqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> 
        : AggregatedRelationalSqlStatementOptionsBuilder<TReferencedEntity, ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>, 
          ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlRelationOptionsBuilder()
            : base(OrmConfiguration.GetEntityDescriptor<TReferencingEntity>())
        {
        }

        protected override ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> Builder => this;
    }
}