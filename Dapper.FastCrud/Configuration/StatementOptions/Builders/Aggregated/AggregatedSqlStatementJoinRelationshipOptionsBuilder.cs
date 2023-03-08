namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Validations;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// The full options builder for JOINs.
    /// </summary>
    internal abstract class AggregatedSqlStatementJoinRelationshipOptionsBuilder<
            TReferencingEntity,
            TReferencedEntity, 
            TStatementOptionsBuilder> 
        : AggregatedSqlJoinRelationshipOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AggregatedSqlStatementJoinRelationshipOptionsBuilder()
            : base(OrmConfiguration.GetEntityDescriptor<TReferencingEntity>())
        {
        }

        /// <summary>
        /// Provides the builder used in constructing the options.
        /// </summary>
        protected abstract TStatementOptionsBuilder Builder { get; }

        /// <summary>
        /// If <paramref name="condition"/> then <paramref name="then"/> else <paramref name="otherwise"/>.
        /// </summary>
        public TStatementOptionsBuilder When(bool condition, Func<TStatementOptionsBuilder, TStatementOptionsBuilder> then, Func<TStatementOptionsBuilder, TStatementOptionsBuilder>? otherwise = null)
        {
            Validate.NotNull(then, nameof(then));
            if (condition)
            {
                then(this.Builder);
            }
            else if (otherwise != null)
            {
                otherwise(this.Builder);
            }
            return this.Builder;
        }

        /// <summary>
        /// If set to true, the results are set on <see cref="AggregatedSqlJoinRelationshipOptions.ReferencingNavigationProperty"/> and <see cref="AggregatedSqlJoinRelationshipOptions.ReferencedNavigationProperty"/>.
        /// </summary>
        public new TStatementOptionsBuilder MapResults(bool mapResults = true)
        {
            base.MapResults = mapResults;
            return this.Builder;
        }

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        public TStatementOptionsBuilder FromAlias(string? referencingEntityAlias)
        {
            this.ReferencingEntityAlias = referencingEntityAlias;
            return this.Builder;
        }

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        public TStatementOptionsBuilder FromProperty(Expression<Func<TReferencingEntity, TReferencedEntity?>>? referencingNavigationProperty)
        {
            this.ReferencingNavigationProperty = referencingNavigationProperty?.GetPropertyDescriptor();
            return this.Builder;
        }

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        public TStatementOptionsBuilder FromProperty(Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>?>>? referencingNavigationProperty)
        {
            this.ReferencingNavigationProperty = referencingNavigationProperty?.GetPropertyDescriptor();
            return this.Builder;
        }

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        public TStatementOptionsBuilder ToProperty(Expression<Func<TReferencedEntity, TReferencingEntity?>>? referencedNavigationProperty)
        {
            this.ReferencedNavigationProperty = referencedNavigationProperty?.GetPropertyDescriptor();
            return this.Builder;
        }

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        public TStatementOptionsBuilder ToProperty(Expression<Func<TReferencedEntity, IEnumerable<TReferencingEntity>?>>? referencedNavigationProperty)
        {
            this.ReferencedNavigationProperty = referencedNavigationProperty?.GetPropertyDescriptor();
            return this.Builder;
        }
    }
}