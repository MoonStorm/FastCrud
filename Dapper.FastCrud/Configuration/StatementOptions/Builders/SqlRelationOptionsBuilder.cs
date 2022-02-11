namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated;
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// SQL statement options builder used in JOINs.
    /// </summary>
    public interface ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>
        : ISqlRelationOptionsSetter<TReferencedEntity, ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>
    {
        /// <summary>
        /// Adds more information about the relationship intended to be used in the join.
        /// </summary>
        ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> FromNavigationProperty(Expression<Func<TReferencingEntity, TReferencedEntity?>> referencingEntityNavigationProperty);

        /// <summary>
        /// Adds more information about the relationship intended to be used in the join.
        /// </summary>
        ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> FromNavigationProperty(Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>?>> referencingEntityNavigationProperty);

        /// <summary>
        /// Adds more information about the relationship intended to be used in the join.
        /// </summary>
        ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> ToNavigationProperty(Expression<Func<TReferencedEntity, TReferencingEntity?>> referencedEntityNavigationProperty);

        /// <summary>
        /// Adds more information about the relationship intended to be used in the join.
        /// </summary>
        ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> ToNavigationProperty(Expression<Func<TReferencedEntity, IEnumerable<TReferencingEntity>?>> referencedEntityNavigationProperty);
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

        /// <summary>
        /// Adds more information about the relationship intended to be used in the join.
        /// </summary>
        public ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> FromNavigationProperty(Expression<Func<TReferencingEntity, TReferencedEntity?>> referencingEntityNavigationProperty)
        {
            Requires.NotNull(referencingEntityNavigationProperty, nameof(referencingEntityNavigationProperty));

            return this.UsingReferencingEntityNavigationProperty(referencingEntityNavigationProperty.GetPropertyDescriptor());
        }

        /// <summary>
        /// Adds more information about the relationship intended to be used in the join.
        /// </summary>
        public ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> FromNavigationProperty(Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>?>> referencingEntityNavigationProperty)
        {
            Requires.NotNull(referencingEntityNavigationProperty, nameof(referencingEntityNavigationProperty));

            return this.UsingReferencingEntityNavigationProperty(referencingEntityNavigationProperty.GetPropertyDescriptor());
        }

        /// <summary>
        /// Adds more information about the relationship intended to be used in the join.
        /// </summary>
        public ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> ToNavigationProperty(Expression<Func<TReferencedEntity, TReferencingEntity?>> referencedEntityNavigationProperty)
        {
            Requires.NotNull(referencedEntityNavigationProperty, nameof(referencedEntityNavigationProperty));

            return this.UsingReferencedEntityNavigationProperty(referencedEntityNavigationProperty.GetPropertyDescriptor());
        }

        /// <summary>
        /// Adds more information about the relationship intended to be used in the join.
        /// </summary>
        public ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> ToNavigationProperty(Expression<Func<TReferencedEntity, IEnumerable<TReferencingEntity>?>> referencedEntityNavigationProperty)
        {
            Requires.NotNull(referencedEntityNavigationProperty, nameof(referencedEntityNavigationProperty));

            return this.UsingReferencedEntityNavigationProperty(referencedEntityNavigationProperty.GetPropertyDescriptor());
        }

        protected override ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity> Builder => this;
    }
}