namespace Dapper.FastCrud.Configuration.StatementOptions.Builders
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Resolvers;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Common options builder for JOINs.
    /// </summary>
    internal abstract class AggregatedRelationalSqlStatementOptionsBuilder<TReferredEntity, TStatementOptionsBuilder> : AggregatedRelationalSqlStatementOptions
    {
        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TReferredEntity> entityMapping)
        {
            this.EntityMappingOverride = entityMapping;
            return this.Builder;
        }

        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        public TStatementOptionsBuilder Where(FormattableString whereClause)
        {
            this.WhereClause = whereClause;
            return this.Builder;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        public TStatementOptionsBuilder OrderBy(FormattableString orderByClause)
        {
            this.OrderClause = orderByClause;
            return this.Builder;
        }

        /// <summary>
        /// A left outer join is desired.
        /// </summary>
        public TStatementOptionsBuilder LeftOuterJoin {
            get
            {
                this.JoinType = SqlJoinType.LeftOuterJoin;
                return this.Builder;
            }
        }

        /// <summary>
        /// An inner join is desired.
        /// </summary>
        public TStatementOptionsBuilder InnerJoin
        {
            get
            {
                this.JoinType = SqlJoinType.InnerJoin;
                return this.Builder;
            }
        }

        protected abstract TStatementOptionsBuilder Builder { get; }
    }
}