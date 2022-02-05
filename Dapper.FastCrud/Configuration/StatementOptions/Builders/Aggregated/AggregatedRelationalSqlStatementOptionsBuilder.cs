namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Mappings;
    using System.ComponentModel;

    /// <summary>
    /// The full options builder for JOINs.
    /// </summary>
    internal abstract class AggregatedRelationalSqlStatementOptionsBuilder<TReferredEntity, TStatementOptionsBuilder> 
        : AggregatedRelationalSqlStatementOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AggregatedRelationalSqlStatementOptionsBuilder()
            : base(OrmConfiguration.GetEntityDescriptor<TReferredEntity>())
        {
        }

        /// <summary>
        /// Provides the builder used in constructing the options.
        /// </summary>
        protected abstract TStatementOptionsBuilder Builder { get; }

        /// <summary>
        /// The entity mapping override to be used for the joined entity.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TReferredEntity>? entityMapping)
        {
            this.EntityRegistration = entityMapping?.Registration!;
            return this.Builder;
        }

        /// <summary>
        /// Sets or resets the navigation property to be used. If not set, the <seealso cref="On"/> condition needs to be provided.
        /// </summary>
        public TStatementOptionsBuilder UsingNavigationProperty(PropertyDescriptor? referencingNavigationProperty)
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
        /// Can be set to true or false in order to map or not the results onto the navigation property set through <seealso cref="UsingNavigationProperty"/>.
        /// </summary>
        public TStatementOptionsBuilder MapResults(bool mapResults = true)
        {
            this.MapResultToNavigationProperty = mapResults;
            return this.Builder;
        }

        /// <summary>
        /// Sets up an alias for the entity participating in the JOIN.
        /// Remember to use this alias everywhere in the query.
        /// </summary>
        public TStatementOptionsBuilder WithAlias(string? tableAlias)
        {
            this.ReferencedEntityAlias = tableAlias;
            return this.Builder;
        }

        /// <summary>
        /// Sets up the ON clause on the query. Remember to use the alias for the related entity in case it was set with <seealso cref="WithAlias"/>.
        /// In case the relationship is already known through the mapping, calling this method will override the implicit SQL you'd normally get for the JOIN.
        /// However in this case it is recommended to use the final WHERE clause on the main query.
        /// </summary>
        public TStatementOptionsBuilder On(FormattableString? onClause)
        {
            this.JoinOnClause = onClause;
            return this.Builder;
        }

        /// <summary>
        /// Extra filter for an ON clause in a JOIN.
        /// </summary>
        [Obsolete(message: "Will be removed in a future version.", error: false)]
        public TStatementOptionsBuilder Where(FormattableString? whereClause)
        {
            this.JoinExtraOnClause = whereClause;
            return this.Builder;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the main statement.
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