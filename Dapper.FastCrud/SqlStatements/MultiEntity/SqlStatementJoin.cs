namespace Dapper.FastCrud.SqlStatements.MultiEntity
{
    using Dapper.FastCrud.Configuration.StatementOptions;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Formatters.Contexts;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// This is mainly used by <seealso cref="GenericSqlStatements{TEntity}"/> class to
    ///   analyze the join options passed in via <seealso cref="AggregatedSqlJoinOptions"/>
    ///   and entities having the information stored in <seealso cref="GenericSqlStatementFormatter"/> for the duration of the request.
    /// The analysis goes all the way to identify even the individual columns used in joining two entities.
    /// </summary>
    internal class SqlStatementJoin
    {
        public SqlStatementJoin(
            AggregatedSqlStatementOptions mainStatementOptions, 
            AggregatedSqlJoinOptions joinStatementOptions)
        {
            Requires.NotNull(mainStatementOptions, nameof(mainStatementOptions));

            this.JoinType = joinStatementOptions.JoinType;
            this.JoinOnClause = joinStatementOptions.JoinOnClause;
            this.JoinExtraWhereClause = joinStatementOptions.ExtraWhereClause;
            this.JoinExtraOrderByClause = joinStatementOptions.ExtraOrderClause;
            this.ReferencedEntityFormatterResolver = joinStatementOptions.ReferencedEntityFormatterResolver;
            this.ResolvedRelationships = this.DiscoverJoinRelationships(mainStatementOptions, joinStatementOptions);
        }

        /// <summary>
        /// Gets the formatter resolver for the referenced entity.
        /// </summary>
        public SqlStatementFormatterResolver ReferencedEntityFormatterResolver { get; }

        /// <summary>
        /// Gets the referenced entity descriptor.
        /// </summary>
        public EntityDescriptor ReferencedEntityDescriptor => this.ReferencedEntityFormatterResolver.EntityDescriptor;

        /// <summary>
        /// Gets the referenced entity registration.
        /// </summary>
        public EntityRegistration ReferencedEntityRegistration => this.ReferencedEntityFormatterResolver.EntityRegistration;

        /// <summary>
        /// Gets the SQL builder attached to the referenced entity.
        /// </summary>
        public GenericStatementSqlBuilder ReferencedEntitySqlBuilder => this.ReferencedEntityFormatterResolver.SqlBuilder;

        /// <summary>
        /// Returns the resolved relationships.
        /// </summary>
        public SqlStatementJoinRelationship[] ResolvedRelationships;

        /// <summary>
        /// Returns true if any of the relationships require result mapping.
        /// </summary>
        public bool RequiresResultMapping => this.ResolvedRelationships.Any(rel => rel.MapResults);

        /// <summary>
        /// The JOIN ON clause. This overrides any relationship that was located through the entity relationship registrations.
        /// </summary>
        public FormattableString? JoinOnClause { get; }

        /// <summary>
        /// The WHERE clause attached to the join, in addition to the main WHERE clause. This is now deprecated.
        /// The where condition is forced to resolve single column formatter specifiers with the associated table or alias.
        /// </summary>
        public FormattableString? JoinExtraWhereClause { get; }

        /// <summary>
        /// The ORDER BY clause attached to the join. This is now deprecated.
        /// The where condition is forced to resolve single column formatter specifiers with the associated table or alias.
        /// </summary>
        public FormattableString? JoinExtraOrderByClause { get; }

        /// <summary>
        /// The type of JOIN requested
        /// </summary>
        public SqlJoinType JoinType { get; }

        private SqlStatementJoinRelationship[] DiscoverJoinRelationships(
            AggregatedSqlStatementOptions statementOptions,
            AggregatedSqlJoinOptions joinOptions)
        {
            var relationshipsToInvestigate = new List<RelationshipToInvestigate>();

            // if we were provided with anything in the JOINed relationships, we'll use that information for investigation
            foreach (var providedJoinRelationship in joinOptions.JoinRelationships)
            {
                var relationshipToInvestigate = new RelationshipToInvestigate(
                    statementOptions.StatementFormatter.LocateResolver(providedJoinRelationship.ReferencingEntityDescriptor.EntityType, providedJoinRelationship.ReferencingEntityAlias),
                    providedJoinRelationship.MapResults ?? joinOptions.MapResults
                );

                relationshipToInvestigate.ReferencingNavigationProperty = providedJoinRelationship.ReferencingNavigationProperty;
                relationshipToInvestigate.ReferencedNavigationProperty = providedJoinRelationship.ReferencedNavigationProperty;

                relationshipsToInvestigate.Add(relationshipToInvestigate);
            }

            if (joinOptions.JoinRelationships.Count == 0 && joinOptions.JoinOnClause == null)
            {
                // or look at all of them prior to this one
                var previousUnusedEntityResolvers = statementOptions.Joins.TakeWhile(join => join != joinOptions)
                                                                    .Select(join => join.ReferencedEntityFormatterResolver)
                                                                    .Concat(new[] { statementOptions.MainEntityFormatterResolver });
                                                                    //.Where(existingResolver => !relationshipsToInvestigate
                                                                    //                            .Select(rel => rel.ReferencingEntityResolver)
                                                                    //                            .Contains(existingResolver)
                foreach (var otherResolver in previousUnusedEntityResolvers)
                {
                    var relationshipToInvestigate = new RelationshipToInvestigate(otherResolver, joinOptions.MapResults);
                    relationshipsToInvestigate.Add(relationshipToInvestigate);
                }
            }

            var analyzedRelationships = new List<SqlStatementJoinRelationship>(relationshipsToInvestigate.Count);
            foreach (var relationshipToInvestigate in relationshipsToInvestigate)
            {
                // now we need to resolve the relationships to extract the matching column properties
                // bear in mind though that we allow extra navigation properties to be added, or use the JOIN ON clause,
                //    in which case we don't have to necessarily succeed at locating relationship registrations

                var referencingRelationshipRegistration = relationshipToInvestigate
                                                                  .ReferencingEntityResolver
                                                                  .EntityRegistration.TryLocateRelationshipThrowWhenMultipleAreFound(
                                                                      joinOptions.ReferencedEntityRegistration.EntityType,
                                                                      referencingNavigationPropertyToFind: relationshipToInvestigate.ReferencingNavigationProperty);

                var referencedRelationshipRegistration = joinOptions
                                                         .ReferencedEntityRegistration
                                                         .TryLocateRelationshipThrowWhenMultipleAreFound(
                                                             relationshipToInvestigate.ReferencingEntityResolver.EntityRegistration.EntityType,
                                                             referencingNavigationPropertyToFind: relationshipToInvestigate.ReferencedNavigationProperty);

                PropertyRegistration[]? referencingEntityColumnProperties = null;
                PropertyRegistration[]? referencedEntityColumnProperties = null;
                PropertyDescriptor? referencingEntityNavigationProperty = null;
                PropertyDescriptor? referencedEntityNavigationProperty = null;

                // we need the referencing relationship
                // if we found the reverse relationship but not the normal one, something's wrong
                if (referencedRelationshipRegistration != null && referencingRelationshipRegistration == null)
                {
                    throw new InvalidOperationException($"Could not locate a matching relationship for '{relationshipToInvestigate.ReferencingEntityResolver.EntityRegistration.EntityType}' (alias '{relationshipToInvestigate.ReferencingEntityResolver.Alias}') -> '{joinOptions.ReferencedEntityRegistration.EntityType}' (alias: '{joinOptions.ReferencedEntityAlias}')");
                }

                // now try to resolve our column properties used in the JOIN 
                // also set up the optional navigation properties
                if (referencingRelationshipRegistration != null)
                {
                    referencingEntityNavigationProperty = relationshipToInvestigate.ReferencingNavigationProperty ?? referencingRelationshipRegistration.ReferencingNavigationProperty;
                    referencedEntityNavigationProperty = relationshipToInvestigate.ReferencedNavigationProperty ?? referencedRelationshipRegistration?.ReferencingNavigationProperty;

                    // now try to resolve the referencing column properties
                    switch (referencingRelationshipRegistration.RelationshipType)
                    {
                        case EntityRelationshipType.ChildToParent:
                            referencingEntityColumnProperties = referencingRelationshipRegistration.ReferencingColumnProperties
                                                                                                   .Select(propName => relationshipToInvestigate.ReferencingEntityResolver.EntityRegistration.GetOrThrowFrozenPropertyRegistrationByPropertyName(propName))
                                                                                                   .ToArray();
                            referencedEntityColumnProperties = joinOptions.ReferencedEntityRegistration.GetAllOrderedFrozenPrimaryKeyRegistrations();
                            break;
                        case EntityRelationshipType.ParentToChildren:
                            referencingEntityColumnProperties = relationshipToInvestigate.ReferencingEntityResolver.EntityRegistration.GetAllOrderedFrozenPrimaryKeyRegistrations();
                            referencedEntityColumnProperties = referencingRelationshipRegistration.ReferencedColumnProperties
                                                                                                  .Select(propName => joinOptions.ReferencedEntityRegistration.GetOrThrowFrozenPropertyRegistrationByPropertyName(propName))
                                                                                                  .ToArray();
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown relationship type {referencingRelationshipRegistration.RelationshipType}");
                    }

                    // again check for imbalances
                    if (referencingEntityColumnProperties.Length != referencedEntityColumnProperties.Length || referencingEntityColumnProperties.Length == 0)
                    {
                        throw new InvalidOperationException($"Invalid number of columns discovered for the relationship '{joinOptions.ReferencedEntityRegistration.EntityType}' (alias: '{joinOptions.ReferencedEntityAlias}') and '{relationshipToInvestigate.ReferencingEntityResolver.EntityRegistration.EntityType}' (alias '{relationshipToInvestigate.ReferencingEntityResolver.Alias}')");
                    }
                }

                // if we do have a relationship or navigation properties to record, do it now
                if (referencingEntityColumnProperties != null || referencingEntityNavigationProperty != null || referencedEntityNavigationProperty != null)
                {
                    var analyzedRelationship = new SqlStatementJoinRelationship(
                        relationshipToInvestigate.ReferencingEntityResolver,
                        referencingEntityNavigationProperty,
                        referencingEntityColumnProperties,
                        joinOptions.ReferencedEntityFormatterResolver,
                        referencedEntityNavigationProperty,
                        referencedEntityColumnProperties,
                        relationshipToInvestigate.MapResults && (referencedEntityNavigationProperty != null || referencingEntityNavigationProperty != null));

                    analyzedRelationships.Add(analyzedRelationship);
                }
            }

            return analyzedRelationships.ToArray();

        }

        private class RelationshipToInvestigate
        {
            public RelationshipToInvestigate(
                SqlStatementFormatterResolver referencingEntityResolver,
                bool mapResults)
            {
                this.MapResults = mapResults;
                this.ReferencingEntityResolver = referencingEntityResolver;
            }

            public bool MapResults { get; }
            public SqlStatementFormatterResolver ReferencingEntityResolver { get; }
            public PropertyDescriptor? ReferencingNavigationProperty { get; set; }
            public PropertyDescriptor? ReferencedNavigationProperty { get; set; }
        }
    }
}
