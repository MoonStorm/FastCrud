namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// The full options builder for JOINs.
    /// </summary>
    internal abstract class AggregatedSqlStatementJoinOptionsBuilder<
            TReferencedEntity, 
            TStatementOptionsBuilder> 
        : AggregatedSqlJoinOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AggregatedSqlStatementJoinOptionsBuilder()
            : base(OrmConfiguration.GetEntityDescriptor<TReferencedEntity>())
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
        /// Sets up an alias for the main entity to be used in a relationship.
        /// It is recommended to add aliases to the joined entities as well.
        /// </summary>
        public TStatementOptionsBuilder WithAlias(string? referencedEntityAlias)
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
        /// If set tot true, mapping relationships will be set on the navigation properties.
        /// This flag can be overriden in a specific relationship by <seealso cref="AggregatedSqlJoinRelationshipOptions.MapResults"/>.
        /// </summary>
        public new TStatementOptionsBuilder MapResults(bool mapResults = true)
        {
            base.MapResults = mapResults;
            return this.Builder;
        }

        /// <summary>
        /// Sets the type of the JOIN to a LEFT OUTER JOIN.
        /// </summary>
        public TStatementOptionsBuilder LeftOuterJoin()
        { 
                this.JoinType = SqlJoinType.LeftOuterJoin;
                return this.Builder;
        }

        /// <summary>
        /// Sets the type of the JOIN to an INNER JOIN.
        /// </summary>
        public TStatementOptionsBuilder InnerJoin()
        {
            this.JoinType = SqlJoinType.InnerJoin;
            return this.Builder;
        }

        /// <summary>
        /// Specifies the referencing entity inside a relationships.
        /// </summary>
        public TStatementOptionsBuilder Referencing<TReferencingEntity>(Action<ISqlJoinRelationshipOptionsBuilder<TReferencingEntity, TReferencedEntity>>? relationship = null)
        {
            var relationshipOptionsBuilder = new SqlJoinRelationshipOptionsBuilder<TReferencingEntity, TReferencedEntity>();
            relationship?.Invoke(relationshipOptionsBuilder);
            this.JoinRelationships.Add(relationshipOptionsBuilder);
            return this.Builder;
        }
    }
}