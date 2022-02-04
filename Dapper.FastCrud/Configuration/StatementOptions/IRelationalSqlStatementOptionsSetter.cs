namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Builders;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Statement options for entity relationships
    /// </summary>
    public interface IRelationalSqlStatementOptionsSetter<TReferencingEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// Includes a referred entity into the query. The relationship must be set up prior to calling this method.
        /// </summary>
        [Obsolete(message:"This method will be removed in a future version. Please use the Join methods instead.", error:false)]
        TStatementOptionsBuilder Include<TReferredEntity> (Action<ILegacySqlRelationOptionsBuilder<TReferredEntity>>? join = null);

        /// <summary>
        /// Performs an INNER JOIN with a related entity.
        /// The relationship does not need to be registered via mappings when used in this manner,
        ///   but in this case you're required to provide the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition.
        /// </summary>
        TStatementOptionsBuilder InnerJoinWith<TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs an INNER JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder InnerJoinWith<TReferencedEntity>(
            Expression<Func<TReferencingEntity, TReferencedEntity>> navigationProperty, 
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs an INNER JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder InnerJoinWith<TReferencedEntity>(
            Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a LEFT JOIN with a related entity.
        /// The relationship does not need to be predefined via mappings when used in this manner,
        ///   but in this case you're required to provide the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition.
        /// </summary>
        TStatementOptionsBuilder LeftJoinWith<TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a LEFT JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder LeftJoinWith<TReferencedEntity>(
            Expression<Func<TReferencingEntity, TReferencedEntity>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a LEFT JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder LeftJoinWith<TReferencedEntity>(
            Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a CROSS JOIN with a related entity.
        /// The relationship does not need to be predefined via mappings when used in this manner,
        ///   but in this case you're required to provide the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition.
        /// </summary>
        TStatementOptionsBuilder CrossJoinWith<TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a CROSS JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder CrossJoinWith<TReferencedEntity>(
            Expression<Func<TReferencingEntity, TReferencedEntity>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a CROSS JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        TStatementOptionsBuilder CrossJoinWith<TReferencedEntity>(
            Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null);
    }
}
