namespace Dapper.FastCrud.Configuration.StatementOptions.Builders.Aggregated
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System.Collections.Generic;
    using System.ComponentModel;
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
            // change these lines and you might want to check AggregatedSqlStatementOptionsBuilder.WithEntityMappingOverride as well
            this.MainEntityFormatterResolver = this.StatementFormatter.RegisterResolver(this.EntityDescriptor, this.EntityRegistration, null);
            this.StatementFormatter.SetActiveMainResolver(this.MainEntityFormatterResolver, false);
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
            // change these lines and you might want to check the constructor of AggregatedSqlStatementOptions
            var oldRegistration = this.EntityRegistration;
            this.EntityRegistration = entityMapping?.Registration!;
            this.MainEntityFormatterResolver = this.StatementFormatter.ReplaceRegisteredResolver(this.EntityDescriptor, oldRegistration, null, this.EntityRegistration, null);
            this.StatementFormatter.SetActiveMainResolver(this.MainEntityFormatterResolver, false);
            return this.Builder;
        }

        /// <summary>
        /// Performs an INNER JOIN with a related entity.
        /// If the relationship can't be inferred from the entity relationship mappings,
        ///   either use one of the other overloads through which you can provide the navigation properties,
        ///   or pass the JOIN clause manually in <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/>.
        /// </summary>
        public TStatementOptionsBuilder InnerJoin<TReferencingEntity, TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null)
        {
            this.ValidateAndAddJoin<TReferencingEntity, TReferencedEntity>(
                SqlJoinType.InnerJoin,
                null,
                null,
                join);
            return this.Builder;
        }

        /// <summary>
        /// Performs an INNER JOIN with a related entity using navigation properties.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder InnerJoin<TReferencingEntity, TReferencedEntity>(
            Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>>> referencingNavigationProperty,
            Expression<Func<TReferencedEntity, TReferencingEntity>> referencedNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null)
        {
            Requires.NotNull(referencedNavigationProperty, nameof(referencedNavigationProperty));
            Requires.NotNull(referencingNavigationProperty, nameof(referencingNavigationProperty));

            this.ValidateAndAddJoin<TReferencingEntity, TReferencedEntity>(
                SqlJoinType.InnerJoin,
                referencingNavigationProperty?.GetPropertyDescriptor(),
                referencedNavigationProperty?.GetPropertyDescriptor(),
                join);
            return this.Builder;
        }

        /// <summary>
        /// Performs an INNER JOIN with a related entity using navigation properties.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder InnerJoin<TReferencingEntity, TReferencedEntity>(
            Expression<Func<TReferencingEntity, TReferencedEntity>> referencingNavigationProperty,
            Expression<Func<TReferencedEntity, IEnumerable<TReferencingEntity>>> referencedNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null)
        {
            Requires.NotNull(referencedNavigationProperty, nameof(referencedNavigationProperty));
            Requires.NotNull(referencingNavigationProperty, nameof(referencingNavigationProperty));

            this.ValidateAndAddJoin<TReferencingEntity, TReferencedEntity>(
                SqlJoinType.InnerJoin,
                referencingNavigationProperty?.GetPropertyDescriptor(),
                referencedNavigationProperty?.GetPropertyDescriptor(),
                join);
            return this.Builder;
        }

        /// <summary>
        /// Performs a LEFT OUTER JOIN with a related entity.
        /// If the relationship can't be inferred from the entity relationship mappings,
        ///   either use one of the other overloads through which you can provide the navigation properties,
        ///   or pass the JOIN clause manually in <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/>.
        /// </summary>
        public TStatementOptionsBuilder LeftJoin<TReferencingEntity, TReferencedEntity>(
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null)
        {
            this.ValidateAndAddJoin<TReferencingEntity, TReferencedEntity>(
                SqlJoinType.LeftOuterJoin,
                null,
                null,
                join);
            return this.Builder;
        }

        /// <summary>
        /// Performs a LEFT OUTER JOIN with a related entity using navigation properties.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder LeftJoin<TReferencingEntity, TReferencedEntity>(
            Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>>> referencingNavigationProperty,
            Expression<Func<TReferencedEntity, TReferencingEntity>> referencedNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null)
        {
            Requires.NotNull(referencedNavigationProperty, nameof(referencedNavigationProperty));
            Requires.NotNull(referencingNavigationProperty, nameof(referencingNavigationProperty));

            this.ValidateAndAddJoin<TReferencingEntity, TReferencedEntity>(
                SqlJoinType.LeftOuterJoin,
                referencingNavigationProperty?.GetPropertyDescriptor(),
                referencedNavigationProperty?.GetPropertyDescriptor(),
                join);
            return this.Builder;
        }

        /// <summary>
        /// Performs an LEFT OUTER JOIN with a related entity using navigation properties.
        /// You do not need to specify the <seealso cref="ISqlRelationOptionsSetter{TReferredEntity,TStatementOptionsBuilder}.On"/> condition
        ///   if the relationship was properly registered.
        /// </summary>
        public TStatementOptionsBuilder LeftJoin<TReferencingEntity, TReferencedEntity>(
            Expression<Func<TReferencingEntity, TReferencedEntity>> referencingNavigationProperty,
            Expression<Func<TReferencedEntity, IEnumerable<TReferencingEntity>>> referencedNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? join = null)
        {
            Requires.NotNull(referencedNavigationProperty, nameof(referencedNavigationProperty));
            Requires.NotNull(referencingNavigationProperty, nameof(referencingNavigationProperty));

            this.ValidateAndAddJoin<TReferencingEntity, TReferencedEntity>(
                SqlJoinType.LeftOuterJoin,
                referencingNavigationProperty?.GetPropertyDescriptor(),
                referencedNavigationProperty?.GetPropertyDescriptor(),
                join);
            return this.Builder;
        }

        /// <summary>
        /// Includes a referred entity into the query. The relationship and the associated mappings must be set up prior to calling this method.
        /// </summary>
        [Obsolete(message: "This method will be removed in a future version. Please use the Join methods instead.", error: false)]
        public TStatementOptionsBuilder Include<TReferencedEntity>(Action<ILegacySqlRelationOptionsBuilder<TReferencedEntity>>? join = null)
        {
            // this is an old method that supports a single entity relationship of the type provided

            // first attempt to fetch the legacy options in any way we can (fake the referencing entity)
            var originalOptionsBuilder = new LegacySqlRelationOptionsBuilder<TReferencedEntity>(OrmConfiguration.GetEntityDescriptor<FakeEntity>());
            originalOptionsBuilder.OfType(SqlJoinType.LeftOuterJoin); // default for legacy
            join?.Invoke(originalOptionsBuilder);

            // let's try to find a matching relationship through the joins already provided to us
            var matchedReferencingRelationships = new[]
                                                  {
                                                      new
                                                      {
                                                          EntityDescriptor = this.EntityDescriptor,
                                                          EntityRegistration = this.EntityRegistration
                                                      }
                                                      
                                                  }
                                       .Concat(this.JoinOptions.Select(join =>
                                       {
                                           return new
                                           {
                                               EntityDescriptor = join.ReferencedEntityDescriptor, 
                                               EntityRegistration = join.ReferencedEntityRegistration
                                           };
                                       }))
                                       .Select(info =>
                                       {
                                           return new
                                           {
                                               EntityDescriptor = info.EntityDescriptor,
                                               MatchedRelationship = info.EntityRegistration.TryLocateRelationshipThrowWhenMultipleAreFound(typeof(TReferencedEntity))
                                           };
                                       })
                                       .Where(entityRegistrationRelationship => entityRegistrationRelationship.MatchedRelationship != null)
                                       .ToArray();
            if (matchedReferencingRelationships.Length == 0)
            {
                throw new InvalidOperationException($"Unable to locate any relationship where the referenced entity is '{typeof(TReferencedEntity)}'. It is recommended to use the Join methods instead.");
            }

            if (matchedReferencingRelationships.Length > 1)
            {
                throw new InvalidOperationException($"More than one relationships where found having the referenced entity as '{typeof(TReferencedEntity)}'. It is recommended to use the Join methods instead.");
            }

            var matchedReferencingRelationship = matchedReferencingRelationships[0];

            // recreate the legacy builder but with a proper entity type now
            var finalOptionsBuilder = new LegacySqlRelationOptionsBuilder<TReferencedEntity>(matchedReferencingRelationship.EntityDescriptor);
            finalOptionsBuilder.OfType(originalOptionsBuilder.JoinType);
            finalOptionsBuilder.UsingReferencingEntityNavigationProperty(matchedReferencingRelationship.MatchedRelationship!.ReferencingNavigationProperty);
            finalOptionsBuilder.MapResults(matchedReferencingRelationship.MatchedRelationship!.ReferencingNavigationProperty!=null);
            finalOptionsBuilder.WithEntityMappingOverride(new EntityMapping<TReferencedEntity>(originalOptionsBuilder.ReferencedEntityRegistration));
            finalOptionsBuilder.Where(originalOptionsBuilder.ExtraWhereClause);
            finalOptionsBuilder.OrderBy(originalOptionsBuilder.ExtraOrderClause);

            this.ValidateAndAddJoin(finalOptionsBuilder);
            return this.Builder;
        }

        private void ValidateAndAddJoin<TReferencingEntity, TReferencedEntity>(
            SqlJoinType joinType,
            PropertyDescriptor? referencingEntityNavigationProperty,
            PropertyDescriptor? referencedEntityNavigationProperty,
            Action<ISqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>>? joinOptionsSetter)
        {
            var optionsBuilder = new SqlRelationOptionsBuilder<TReferencingEntity, TReferencedEntity>();
            optionsBuilder.OfType(joinType);
            optionsBuilder.UsingReferencingEntityNavigationProperty(referencingEntityNavigationProperty);
            optionsBuilder.UsingReferencedEntityNavigationProperty(referencedEntityNavigationProperty);
            joinOptionsSetter?.Invoke(optionsBuilder);

            this.ValidateAndAddJoin(optionsBuilder);
        }

        private void ValidateAndAddJoin(AggregatedRelationalSqlStatementOptions joinOptions)
        {
            // for an early warning, perform the validation now
            joinOptions.ReferencedEntityFormatterResolver = this.StatementFormatter.RegisterResolver(
                joinOptions.ReferencedEntityDescriptor, 
                joinOptions.ReferencedEntityRegistration, 
                joinOptions.ReferencedEntityAlias);
            this.JoinOptions.Add(joinOptions);
        }

    }
}
