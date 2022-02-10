namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated;
    using Dapper.FastCrud.EntityDescriptors;
    using System;

    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    [Obsolete(message: "Will be removed in a future version.", error: false)]
    public interface ILegacySqlRelationOptionsBuilder<TReferencedEntity>
        : ILegacySqlRelationOptionsSetter<TReferencedEntity, ILegacySqlRelationOptionsBuilder<TReferencedEntity>>
    {
    }

    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    [Obsolete(message: "Will be removed in a future version.", error: false)]
    internal class LegacySqlRelationOptionsBuilder<TReferencedEntity> 
        : AggregatedRelationalSqlStatementOptionsBuilder<TReferencedEntity, ILegacySqlRelationOptionsBuilder<TReferencedEntity>>, ILegacySqlRelationOptionsBuilder<TReferencedEntity>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LegacySqlRelationOptionsBuilder(EntityDescriptor referencingEntityDescriptor)
            : base(referencingEntityDescriptor)
        {
        }

        protected override ILegacySqlRelationOptionsBuilder<TReferencedEntity> Builder => this;
    }
}