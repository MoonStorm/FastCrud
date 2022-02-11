namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Builders;

    /// <summary>
    /// Statement options for entity relationships
    /// </summary>
    public interface IRelationalSqlStatementOptionsSetter<TStatementOptionsBuilder>
    {
        /// <summary>
        /// Sets up an alias for the main entity to be used in a relationship.
        /// It is recommended to add aliases to the joined entities as well.
        /// </summary>
        public TStatementOptionsBuilder WithAlias(string? mainEntityAlias);
        
        /// <summary>
        /// Includes a referred entity into the query. The relationship must be set up prior to calling this method.
        /// </summary>
        [Obsolete(message:"This method will be removed in a future version. Please use the Join methods instead.", error:false)]
        TStatementOptionsBuilder Include<TReferredEntity> (Action<ILegacySqlRelationOptionsBuilder<TReferredEntity>>? join = null);

        /// <summary>
        /// Performs an INNER JOIN between the two entities provided.
        /// If the mapped relationship can't be inferred from the entity relationship mappings, use
        ///   <code>FromNavigationProperty</code>, <code>ToNavigationProperty</code>, <code>FromAlias</code> and <code>ToAlias</code> on the <paramref name="join"/>.
        /// If you don't intend to use a relationship mapping, use <code>On</code> on the <paramref name="join"/>.
        /// </summary>
        /// <param name="join">Optional join options.</param>
        TStatementOptionsBuilder InnerJoin<TReferencingEntity, TReferencedEntity>(Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null);

        /// <summary>
        /// Performs a LEFT OUTER JOIN between the two entities provided.
        /// If the mapped relationship can't be inferred from the entity relationship mappings, use
        ///   <code>FromNavigationProperty</code>, <code>ToNavigationProperty</code>, <code>FromAlias</code> and <code>ToAlias</code> on the <paramref name="join"/>.
        /// If you don't intend to use a relationship mapping, use <code>On</code> on the <paramref name="join"/>.
        /// </summary>
        /// <param name="join">Optional join options.</param>
        public TStatementOptionsBuilder LeftJoin<TReferencingEntity, TReferencedEntity>(Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null);
    }
}
