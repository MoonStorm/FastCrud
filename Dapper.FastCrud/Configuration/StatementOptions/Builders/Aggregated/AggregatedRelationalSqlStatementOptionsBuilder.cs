namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;
    using System.ComponentModel;

    /// <summary>
    /// The full options builder for JOINs.
    /// </summary>
    internal abstract class AggregatedRelationalSqlStatementOptionsBuilder<
            TReferencedEntity, 
            TStatementOptionsBuilder> 
        : AggregatedRelationalSqlStatementOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AggregatedRelationalSqlStatementOptionsBuilder(EntityDescriptor referencingEntityDescriptor)
            : base(referencingEntityDescriptor, OrmConfiguration.GetEntityDescriptor<TReferencedEntity>())
        {
        }

        /// <summary>
        /// Provides the builder used in constructing the options.
        /// </summary>
        protected abstract TStatementOptionsBuilder Builder { get; }

        /// <summary>
        /// Has no effect on the join builder.
        /// Provided in case you want to include conditional options.
        /// </summary>
        public TStatementOptionsBuilder NoOp()
        {
            return this.Builder;
        }

        /// <summary>
        /// The entity mapping override to be used for the joined entity.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TReferencedEntity>? entityMapping)
        {
            this.ReferencedEntityRegistration = entityMapping?.Registration!;
            return this.Builder;
        }

        /// <summary>
        /// Sets or resets the navigation property to be used on the referenced entity.
        /// Note that if not enough information is provided, the <seealso cref="On"/> becomes mandatory.
        /// </summary>
        public TStatementOptionsBuilder UsingReferencedEntityNavigationProperty(PropertyDescriptor? referencedNavigationProperty)
        {
            this.ReferencedNavigationProperty = referencedNavigationProperty;
            return this.Builder;
        }

        /// <summary>
        /// Sets or resets the navigation property to be used on the referencing entity.
        /// Note that if not enough information is provided, the <seealso cref="On"/> becomes mandatory.
        /// </summary>
        public TStatementOptionsBuilder UsingReferencingEntityNavigationProperty(PropertyDescriptor? referencingNavigationProperty)
        {
            this.ReferencingNavigationProperty = referencingNavigationProperty;
            return this.Builder;
        }

        /// <summary>
        /// Sets the SQL join type.
        /// </summary>
        public TStatementOptionsBuilder OfType(SqlJoinType joinType)
        {
            this.JoinType = joinType;
            return this.Builder;
        }

        /// <summary>
        /// Can be set to true or false in order to map or not the results onto the navigation property set through <seealso cref="UsingReferencedEntityNavigationProperty"/>.
        /// </summary>
        public TStatementOptionsBuilder MapResults(bool mapResults = true)
        {
            this.MapResultToNavigationProperties = mapResults;
            return this.Builder;
        }

        /// <summary>
        /// Provides the alias the referencing entity is known as  throughout the statement.
        /// </summary>
        public TStatementOptionsBuilder FromAlias(string? referencingEntityAlias)
        {
            this.ReferencingEntityAlias = referencingEntityAlias;
            return this.Builder;
        }

        /// <summary>
        /// Sets up an alias for the new referenced entity participating in the JOIN.
        /// Remember to use this alias everywhere in the query.
        /// </summary>
        public TStatementOptionsBuilder ToAlias(string? referencedEntityAlias)
        {
            this.ReferencedEntityAlias = referencedEntityAlias;
            return this.Builder;
        }

        /// <summary>
        /// Sets up the ON clause on the query. Remember to use the alias for the related entity in case it was set with <seealso cref="ToAlias"/>.
        /// In case the relationship is already known through the mapping, calling this method will override the implicit SQL you'd normally get for the JOIN.
        /// However in this case it is recommended to use the final WHERE clause on the main query.
        /// </summary>
        public TStatementOptionsBuilder On(FormattableString? onClause)
        {
            this.JoinOnClause = onClause;
            return this.Builder;
        }

        /// <summary>
        /// Extra conditions to be used for the joined entity. These will be added to the main WHERE clause.
        /// The formatter used to resolve the formattable string defaults to the JOINed entity, hence all the single columns become fully qualified.
        /// </summary>
        [Obsolete(message: "Will be removed in a future version.", error: false)]
        public TStatementOptionsBuilder Where(FormattableString? whereClause)
        {
            this.ExtraWhereClause = whereClause;
            return this.Builder;
        }

        /// <summary>
        /// Adds an extra ORDER BY clause to the main statement.
        /// The formatter used to resolve the formattable string defaults to the JOINed entity, hence all the single columns become fully qualified.
        /// </summary>
        [Obsolete(message: "Will be removed in a future version.", error: false)]
        public TStatementOptionsBuilder OrderBy(FormattableString? orderByClause)
        {
            this.ExtraOrderClause = orderByClause;
            return this.Builder;
        }

        /// <summary>
        /// Sets the type of the JOIN to a LEFT OUTER JOIN.
        /// </summary>
        [Obsolete(message: "Will be removed in a future version.", error: false)]
        public TStatementOptionsBuilder LeftOuterJoin()
        { 
                this.JoinType = SqlJoinType.LeftOuterJoin;
                return this.Builder;
        }

        /// <summary>
        /// Sets the type of the JOIN to an INNER JOIN.
        /// </summary>
        [Obsolete(message: "Will be removed in a future version.", error: false)]
        public TStatementOptionsBuilder InnerJoin()
        {
            this.JoinType = SqlJoinType.InnerJoin;
            return this.Builder;
        }
    }
}