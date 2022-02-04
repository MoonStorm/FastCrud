namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// The full options builder for main queries.
    /// Note that the publicly available builder will use a subset of this functionality and will vary depending on usage.
    /// </summary>
    internal abstract class AggregatedSqlStatementOptionsBuilder<TEntity, TStatementOptionsBuilder> : AggregatedSqlStatementOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AggregatedSqlStatementOptionsBuilder()
        :base(OrmConfiguration.GetEntityDescriptor<TEntity>())
        {
        }

        /// <summary>
        /// Provides the builder used in constructing the options.
        /// </summary>
        protected abstract TStatementOptionsBuilder Builder { get; }

        /// <summary>
        /// Limits the results set by the top number of records returned.
        /// </summary>
        public TStatementOptionsBuilder Top(long? topRecords)
        {
            Requires.Argument(topRecords == null || topRecords > 0, nameof(topRecords), "The top record count must be a positive value");

            this.LimitResults = topRecords;
            return this.Builder;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        public TStatementOptionsBuilder OrderBy(FormattableString? orderByClause)
        {
            this.OrderClause = orderByClause;
            return this.Builder;
        }

        /// <summary>
        /// Skips the initial set of results.
        /// </summary>
        public TStatementOptionsBuilder Skip(long? skipRecordsCount)
        {
            Requires.Argument(skipRecordsCount == null || skipRecordsCount >= 0, nameof(skipRecordsCount), "The number of records to skip must be a positive value");

            this.SkipResults = skipRecordsCount;
            return this.Builder;
        }

        /// <summary>
        /// Causes the result set to be streamed.
        /// </summary>
        public TStatementOptionsBuilder StreamResults(bool streamResults = true)
        {
            this.ForceStreamResults = streamResults;
            return this.Builder;
        }

        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        public TStatementOptionsBuilder Where(FormattableString? whereClause)
        {
            this.WhereClause = whereClause;
            return this.Builder;
        }

        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        public TStatementOptionsBuilder WithParameters(object? parameters)
        {
            this.Parameters = parameters;
            return this.Builder;
        }

        /// <summary>
        /// Enforces a maximum time span on the current command.
        /// </summary>
        public TStatementOptionsBuilder WithTimeout(TimeSpan? commandTimeout)
        {
            this.CommandTimeout = commandTimeout;
            return this.Builder;
        }

        /// <summary>
        /// Attaches the current command to an existing transaction.
        /// </summary>
        public TStatementOptionsBuilder AttachToTransaction(IDbTransaction? transaction)
        {
            this.Transaction = transaction;
            return this.Builder;
        }

        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        public TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TEntity>? entityMapping)
        {
            this.EntityRegistration = entityMapping?.Registration!;
            return this.Builder;
        }

        /// <summary>
        /// Performs an INNER JOIN with a related entity.
        /// The relationship does not need to be registered via mappings when used in this manner,
        ///   but in this case you're required to provide the ON condition.
        /// </summary>
        public TStatementOptionsBuilder InnerJoinWith<TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.OfType(SqlJoinType.InnerJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Performs an INNER JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder InnerJoinWith<TReferencedEntity>(
            Expression<Func<TEntity, IEnumerable<TReferencedEntity>>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.OfType(SqlJoinType.InnerJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Performs an INNER JOIN with a related entity, using a navigation property.
        /// You do not need to specify the ON condition if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder InnerJoinWith<TReferencedEntity>(
            Expression<Func<TEntity, TReferencedEntity>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            Requires.NotNull(navigationProperty, nameof(navigationProperty));
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.UsingNavigationProperty(navigationProperty.GetPropertyDescriptor());
            relationshipOptionsBuilder.OfType(SqlJoinType.InnerJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Performs a LEFT JOIN with a related entity.
        /// The relationship does not need to be registered via mappings when used in this manner,
        ///   but in this case you're required to provide the ON condition.
        /// </summary>
        public TStatementOptionsBuilder LeftJoinWith<TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.OfType(SqlJoinType.LeftOuterJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Performs an LEFT JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder LeftJoinWith<TReferencedEntity>(
            Expression<Func<TEntity, IEnumerable<TReferencedEntity>>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.OfType(SqlJoinType.LeftOuterJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Performs an LEFT JOIN with a related entity, using a navigation property.
        /// You do not need to specify the ON condition if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder LeftJoinWith<TReferencedEntity>(
            Expression<Func<TEntity, TReferencedEntity>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            Requires.NotNull(navigationProperty, nameof(navigationProperty));
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.UsingNavigationProperty(navigationProperty.GetPropertyDescriptor());
            relationshipOptionsBuilder.OfType(SqlJoinType.LeftOuterJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Performs a CROSS JOIN with a related entity.
        /// The relationship does not need to be registered via mappings when used in this manner,
        ///   but in this case you're required to provide the ON condition.
        /// </summary>
        public TStatementOptionsBuilder CrossJoinWith<TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.OfType(SqlJoinType.CrossJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Performs an CROSS JOIN with a related entity, using a navigation property.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder CrossJoinWith<TReferencedEntity>(
            Expression<Func<TEntity, IEnumerable<TReferencedEntity>>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.OfType(SqlJoinType.CrossJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Performs an LEFT JOIN with a related entity, using a navigation property.
        /// You do not need to specify the ON condition if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder CrossJoinWith<TReferencedEntity>(
            Expression<Func<TEntity, TReferencedEntity>> navigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            Requires.NotNull(navigationProperty, nameof(navigationProperty));
            var relationshipOptionsBuilder = new SqlRelationOptionsBuilder<TReferencedEntity>();
            relationshipOptionsBuilder.UsingNavigationProperty(navigationProperty.GetPropertyDescriptor());
            relationshipOptionsBuilder.OfType(SqlJoinType.CrossJoin);
            join?.Invoke(relationshipOptionsBuilder);
            this.ValidateAndAddJoin(relationshipOptionsBuilder);
            return this.Builder;
        }

        /// <summary>
        /// Includes a referred entity into the query. The relationship and the associated mappings must be set up prior to calling this method.
        /// </summary>
        [Obsolete(message: "This method will be removed in a future version. Please use the Join methods instead.", error: false)]
        public TStatementOptionsBuilder Include<TReferencedEntity>(Action<ILegacySqlRelationOptionsBuilder<TReferencedEntity>>? relationshipOptions = null)
        {
            // TODO: restore the functionality of this method for the full release
            throw new NotSupportedException();
            //// set up the relationship options
            //var options = new SqlRelationOptionsBuilder<TReferencedEntity>();
            //relationshipOptions?.Invoke(options);
            //this.RelationshipOptions[typeof(TReferencedEntity)] = options;

            //// set up the factory chain
            //var priorSqlStatementsFactoryChain = this.SqlStatementsFactory;

            //this.SqlStatementsFactory = () =>
            //{
            //    var currentSqlStatementsFactory = priorSqlStatementsFactoryChain();
            //    var nextSqlStatementsFactory = currentSqlStatementsFactory.CombineWith<TReferencedEntity>(OrmConfiguration.GetSqlStatements<TReferencedEntity>(options.EntityMappingOverride));
            //    return nextSqlStatementsFactory;
            //};

            //return this.Builder;
        }

        private void ValidateAndAddJoin(AggregatedRelationalSqlStatementOptions joinOptions)
        {
            var existingJoinWithSameAliasOrTableName = this.RelationshipOptions
                                                           .FirstOrDefault(existingRelationship =>
                                                                               (existingRelationship.ReferencedEntityAlias ?? existingRelationship.EntityDescriptor.CurrentEntityMappingRegistration.TableName)
                                                                               == (joinOptions.ReferencedEntityAlias ?? joinOptions.EntityDescriptor.CurrentEntityMappingRegistration.TableName));
            if (existingJoinWithSameAliasOrTableName != null)
            {
                throw new ArgumentException(
                    $"A unique alias needs to be specified for the JOIN. '{existingJoinWithSameAliasOrTableName.ReferencedEntityAlias ?? existingJoinWithSameAliasOrTableName.EntityRegistration.TableName}' was already used in a previous JOIN.");
            }
        }
    }
}
