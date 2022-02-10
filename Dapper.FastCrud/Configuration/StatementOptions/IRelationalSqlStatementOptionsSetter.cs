namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Builders;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Statement options for entity relationships
    /// </summary>
    public interface IRelationalSqlStatementOptionsSetter<TStatementOptionsBuilder>
    {
        /// <summary>
        /// Includes a referred entity into the query. The relationship must be set up prior to calling this method.
        /// </summary>
        [Obsolete(message:"This method will be removed in a future version. Please use the Join methods instead.", error:false)]
        TStatementOptionsBuilder Include<TReferredEntity> (Action<ILegacySqlRelationOptionsBuilder<TReferredEntity>>? join = null);

        /// <summary>
        /// Performs an INNER JOIN with a related entity.
        /// If the relationship can't be inferred from the entity relationship mappings,
        ///   either use one of the other overloads through which you can provide the navigation properties,
        ///   or pass the JOIN clause manually in <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/>.
        /// </summary>
        TStatementOptionsBuilder InnerJoin<TReferencingEntity, TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs an INNER JOIN with a related entity using navigation properties.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder InnerJoin<TReferencingEntity, TReferencedEntity>(
            Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>>> referencingNavigationProperty,
            Expression<Func<TReferencedEntity, TReferencingEntity>> referencedNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs an INNER JOIN with a related entity using navigation properties.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder InnerJoin<TReferencingEntity, TReferencedEntity>(
            Expression<Func<TReferencingEntity, TReferencedEntity>> referencingNavigationProperty,
            Expression<Func<TReferencedEntity, IEnumerable<TReferencingEntity>>> referencedNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a LEFT OUTER JOIN with a related entity.
        /// If the relationship can't be inferred from the entity relationship mappings,
        ///   either use one of the other overloads through which you can provide the navigation properties,
        ///   or pass the JOIN clause manually in <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/>.
        /// </summary>
        TStatementOptionsBuilder LeftJoin<TReferencingEntity, TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a LEFT OUTER JOIN with a related entity using navigation properties.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder LeftJoin<TReferencingEntity, TReferencedEntity>(
            Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>>> referencingNavigationProperty,
            Expression<Func<TReferencedEntity, TReferencingEntity>> referencedNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs an LEFT OUTER JOIN with a related entity using navigation properties.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder LeftJoin<TReferencingEntity, TReferencedEntity>(
            Expression<Func<TReferencingEntity, TReferencedEntity>> referencingNavigationProperty,
            Expression<Func<TReferencedEntity, IEnumerable<TReferencingEntity>>> referencedNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null);
    }
}
