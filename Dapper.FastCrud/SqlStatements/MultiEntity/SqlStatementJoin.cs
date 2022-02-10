namespace Dapper.FastCrud.SqlStatements.MultiEntity
{
    using Dapper.FastCrud.Configuration.StatementOptions;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Formatters.Contexts;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// This is mainly used by <seealso cref="GenericSqlStatements{TEntity}"/> class to
    ///   analyze the join options passed in via <seealso cref="AggregatedRelationalSqlStatementOptions"/>
    ///   and entities having the information stored in <seealso cref="GenericSqlStatementFormatter"/> for the duration of the request.
    /// The analysis goes all the way to identify even the individual columns used in joining two entities.
    /// </summary>
    internal class SqlStatementJoin
    {
        private readonly Lazy<GenericStatementSqlBuilder> _referencedEntitySqlBuilder;
        private readonly Lazy<GenericStatementSqlBuilder> _referencingEntitySqlBuilder;

        /// <summary>
        /// Default constructor
        /// </summary>
        private SqlStatementJoin(
            SqlJoinType joinType,
            bool requiresResultMapping,
            FormattableString? joinExtraWhereClause, // this is deprecated
            FormattableString? joinExtraOrderByClause, // this is deprecated
            FormattableString? joinOnClause,

            EntityRelationshipType? relationshipType,

            EntityDescriptor referencingEntityDescriptor,
            EntityRegistration? referencingEntityRegistration,
            SqlStatementFormatterResolver? referencingEntityResolver,
            PropertyDescriptor? referencingNavigationProperty,
            bool referencingNavigationPropertyIsCollection,
            PropertyRegistration[]? referencingColumnProperties,

            EntityDescriptor referencedEntityDescriptor,
            EntityRegistration referencedEntityRegistration,
            SqlStatementFormatterResolver referencedEntityResolver,
            PropertyDescriptor? referencedNavigationProperty,
            bool referencedNavigationPropertyIsCollection,
            PropertyRegistration[]? referencedColumnProperties)
        {
            Requires.NotNull(referencingEntityDescriptor, nameof(referencingEntityDescriptor));
            Requires.NotNull(referencedEntityDescriptor, nameof(referencedEntityDescriptor));
            Requires.NotNull(referencedEntityRegistration, nameof(referencedEntityRegistration));
            Requires.NotNull(referencedEntityResolver, nameof(referencedEntityResolver));

            this.JoinType = joinType;
            this.RequiresResultMapping = requiresResultMapping;
            this.JoinExtraWhereClause = joinExtraWhereClause;
            this.JoinExtraOrderByClause = joinExtraOrderByClause;
            this.JoinOnClause = joinOnClause;
            this.RelationshipType = relationshipType;

            this.ReferencingEntityRegistration = referencingEntityRegistration;
            this.ReferencingEntityFormatterResolver = referencingEntityResolver;
            this.ReferencingNavigationProperty = referencingNavigationProperty;
            this.ReferencingNavigationPropertyIsCollection = referencingNavigationPropertyIsCollection;
            this.ReferencingColumnProperties = referencingColumnProperties;

            this.ReferencedEntityRegistration = referencedEntityRegistration;
            this.ReferencedEntityFormatterResolver = referencedEntityResolver;
            this.ReferencedNavigationProperty = referencedNavigationProperty;
            this.ReferencedNavigationPropertyIsCollection = referencedNavigationPropertyIsCollection;
            this.ReferencedColumnProperties = referencedColumnProperties;

            _referencingEntitySqlBuilder = new Lazy<GenericStatementSqlBuilder>(() => referencingEntityDescriptor.GetSqlBuilder(this.ReferencingEntityRegistration), LazyThreadSafetyMode.PublicationOnly);
            _referencedEntitySqlBuilder = new Lazy<GenericStatementSqlBuilder>(() => referencedEntityDescriptor.GetSqlBuilder(this.ReferencedEntityRegistration), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// This property holds the referencing entity registration.
        /// </summary>
        public EntityRegistration? ReferencingEntityRegistration { get; }

        /// <summary>
        /// This property holds the referenced entity registration.
        /// </summary>
        public EntityRegistration ReferencedEntityRegistration { get; }

        /// <summary>
        /// If a matching relationship has been located from the entity registrations,
        /// this property holds the referencing formatter resolver.
        /// This had to be previously registered in <seealso cref="SqlStatementFormatterResolverMap"/>
        /// and can be used to set the active resolver in <seealso cref="GenericSqlStatementFormatter.SetActiveMainResolver"/>.
        /// </summary>
        public SqlStatementFormatterResolver? ReferencingEntityFormatterResolver { get; }

        /// <summary>
        /// If a matching relationship has been located from the entity registrations,
        /// this property holds the referenced formatter resolver.
        /// This had to be previously registered in <seealso cref="SqlStatementFormatterResolverMap"/>
        /// and can be used to set the active resolver in <seealso cref="GenericSqlStatementFormatter.SetActiveMainResolver"/>.
        /// </summary>
        public SqlStatementFormatterResolver ReferencedEntityFormatterResolver { get; }

        /// <summary>
        /// If a matching referencing-&lt;referenced relationship has been located from the entity registrations,
        /// this holds the navigation property the relationship was set with.
        /// Note that this is an optional feature when defining a relationship.
        /// </summary>
        public PropertyDescriptor? ReferencingNavigationProperty { get; }

        /// <summary>
        /// Returns true if the navigation property on the referencing property is a collection.
        /// </summary>
        public bool ReferencingNavigationPropertyIsCollection { get; }

        /// <summary>
        /// If a matching referenced-&lt;referencing relationship has been located from the entity registrations,
        /// this holds the navigation property the relationship was set with.
        /// Note that this is an optional feature when defining a relationship.
        /// </summary>
        public PropertyDescriptor? ReferencedNavigationProperty { get; }

        /// <summary>
        /// Returns true if the navigation property on the referenced property is a collection.
        /// </summary>
        public bool ReferencedNavigationPropertyIsCollection { get; }

        /// <summary>
        /// If a matching relationship has been located from the entity registrations,
        /// this holds the properties representing the columns on the referencing entity that are used to JOIN the two entities.
        /// </summary>
        public PropertyRegistration[]? ReferencingColumnProperties { get; }

        /// <summary>
        /// If a matching relationship has been located from the entity registrations,
        /// this holds the properties representing the columns on the referenced entity that are used to JOIN the two entities.
        /// </summary>
        public PropertyRegistration[]? ReferencedColumnProperties { get; }

        /// <summary>
        /// If a matching relationship has been located from the entity registrations,
        /// this property holds the type of relationship found between the two entity types.
        /// </summary>
        public EntityRelationshipType? RelationshipType { get; }

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
        /// When this is set, the results should be populated in <seealso cref="ReferencingNavigationProperty"/> and <seealso cref="ReferencedNavigationProperty"/>.
        /// </summary>
        public bool RequiresResultMapping { get; }

        /// <summary>
        /// The type of JOIN requested
        /// </summary>
        public SqlJoinType JoinType { get; }

        /// <summary>
        /// The SQL builder attached to the referencing entity.
        /// </summary>
        public GenericStatementSqlBuilder ReferencingEntitySqlBuilder => _referencingEntitySqlBuilder.Value;

        /// <summary>
        /// The SQL builder attached to the referenced entity.
        /// </summary>
        public GenericStatementSqlBuilder ReferencedEntitySqlBuilder => _referencedEntitySqlBuilder.Value;

        /// <summary>
        /// Attempts to find information about a JOIN.
        /// </summary>
        public static SqlStatementJoin From(
            AggregatedSqlStatementOptions statementOptions,
            AggregatedRelationalSqlStatementOptions referencedJoinOptions)
        {
            Requires.NotNull(statementOptions, nameof(statementOptions));
            Requires.NotNull(referencedJoinOptions, nameof(referencedJoinOptions));

            EntityRelationshipType? referencingToReferencedRelationshipType = null;
            EntityRelationshipType? referencedToReferencingRelationshipType = null;

            EntityRegistration referencingEntityRegistration = referencedJoinOptions.ReferencingEntityFormatterResolver.EntityRegistration;
            EntityRegistration referencedEntityRegistration = referencedJoinOptions.ReferencedEntityRegistration;

            PropertyDescriptor? referencingNavigationProperty = referencedJoinOptions.ReferencingNavigationProperty;
            PropertyDescriptor? referencedNavigationProperty = referencedJoinOptions.ReferencedNavigationProperty;
            PropertyRegistration[]? referencingColumnProperties = null;
            PropertyRegistration[]? referencedColumnProperties = null;

            // UPDATE: the following was disabled as we could be very wrong
            // try to guess the type of relationships we're dealing with here
            // after we have located the actual relationship registrations, we'll update them
            //if (referencingNavigationProperty != null)
            //{
            //    if (referencingNavigationProperty.IsEntityCollectionProperty())
            //    {
            //        referencingToReferencedRelationshipType = EntityRelationshipType.ParentToChildren;
            //        referencedToReferencingRelationshipType = EntityRelationshipType.ChildToParent;
            //    }
            //    else
            //    {
            //        referencingToReferencedRelationshipType = EntityRelationshipType.ChildToParent;
            //        referencedToReferencingRelationshipType = EntityRelationshipType.ParentToChildren;
            //    }
            //}
            //else if (referencedNavigationProperty != null)
            //{
            //    if (referencedNavigationProperty.IsEntityCollectionProperty())
            //    {
            //        referencingToReferencedRelationshipType = EntityRelationshipType.ChildToParent;
            //        referencedToReferencingRelationshipType = EntityRelationshipType.ParentToChildren;
            //    }
            //    else
            //    {
            //        referencingToReferencedRelationshipType = EntityRelationshipType.ParentToChildren;
            //        referencedToReferencingRelationshipType = EntityRelationshipType.ChildToParent;
            //    }
            //}

            // try to locate the referenced -> referencing relationship
            EntityRelationshipRegistration? matchedReferencedToReferencingRelationship = referencedEntityRegistration.TryLocateRelationshipThrowWhenMultipleAreFound(
                referencingEntityRegistration.EntityType,
                relationshipTypeToFind: referencedToReferencingRelationshipType,
                referencingNavigationPropertyToFind: referencedNavigationProperty);

            // set some of our guesses with the real values
            if (matchedReferencedToReferencingRelationship != null)
            {
                referencedToReferencingRelationshipType = matchedReferencedToReferencingRelationship.RelationshipType;
                referencedNavigationProperty ??= matchedReferencedToReferencingRelationship.ReferencingNavigationProperty;
            }

            // now try to locate the referencing -> referenced relationship
            var matchedReferencingToReferencedRelationship =
                referencingEntityRegistration
                    .TryLocateRelationshipThrowWhenMultipleAreFound(
                        referencedEntityRegistration.EntityType,
                        relationshipTypeToFind: referencingToReferencedRelationshipType,
                        referencedColumnPropertiesToFind: matchedReferencedToReferencingRelationship?.RelationshipType switch
                        {
                            EntityRelationshipType.ChildToParent => matchedReferencedToReferencingRelationship.ReferencingColumnProperties,
                            _ => Array.Empty<string>()
                        },
                        referencingColumnPropertiesToFind: matchedReferencedToReferencingRelationship?.RelationshipType switch
                        {
                            EntityRelationshipType.ParentToChildren => matchedReferencedToReferencingRelationship.ReferencedColumnProperties,
                            _ => Array.Empty<string>()
                        });

            if (matchedReferencingToReferencedRelationship != null)
            {
                referencingNavigationProperty ??= matchedReferencingToReferencedRelationship.ReferencingNavigationProperty;
                referencingToReferencedRelationshipType = matchedReferencingToReferencedRelationship.RelationshipType;
            }

            if (matchedReferencingToReferencedRelationship != null)
            {
                switch (referencingToReferencedRelationshipType)
                {
                    case EntityRelationshipType.ChildToParent:
                        referencingColumnProperties = matchedReferencingToReferencedRelationship!.ReferencingColumnProperties
                                                                                                 .Select(prop => referencingEntityRegistration
                                                                                                                 .GetOrThrowFrozenPropertyRegistrationByPropertyName(prop))
                                                                                                 .ToArray();
                        referencedColumnProperties = referencedEntityRegistration.GetAllOrderedFrozenPrimaryKeyRegistrations();
                        break;
                    case EntityRelationshipType.ParentToChildren:
                        referencingColumnProperties = referencingEntityRegistration.GetAllOrderedFrozenPrimaryKeyRegistrations();
                        referencedColumnProperties = matchedReferencingToReferencedRelationship!.ReferencedColumnProperties
                                                                                                .Select(prop => referencedEntityRegistration.GetOrThrowFrozenPropertyRegistrationByPropertyName(prop))
                                                                                                .ToArray();
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown relationship type '{referencingToReferencedRelationshipType}' when trying to locate the column properties involved in a relationship.");
                }
            }

            if (referencedJoinOptions.JoinOnClause == null)
            {
                // we have to come up with the join condition ourselves, for which we need a set of columns to match
                if (referencedColumnProperties == null || referencedColumnProperties.Length == 0)
                {
                    throw new InvalidOperationException($"Found no columns on the referenced entity '{referencedEntityRegistration.EntityType}' (alias: {referencedJoinOptions.ReferencedEntityAlias}) that can participate in the JOIN with the referencing entity '{referencingEntityRegistration.EntityType}' (alias: {referencedJoinOptions.ReferencingEntityAlias}).");
                }

                if (referencingColumnProperties == null || referencingColumnProperties.Length == 0)
                {
                    throw new InvalidOperationException($"Found no columns on the referencing entity '{referencingEntityRegistration.EntityType}' (alias: {referencedJoinOptions.ReferencingEntityAlias}) that can participate in the JOIN with the referenced entity '{referencedEntityRegistration.EntityType}' (alias: {referencedJoinOptions.ReferencedEntityAlias}).");
                }

                if (referencingColumnProperties.Length != referencedColumnProperties.Length)
                {
                    throw new InvalidOperationException($"The columns used in the JOIN '{referencedJoinOptions.ReferencingEntityDescriptor.EntityType}' -> '{referencedJoinOptions.ReferencedEntityRegistration.EntityType}' do not match by count.");
                }
            }

            // now we have everything we need
            return new SqlStatementJoin(
                referencedJoinOptions.JoinType,
                referencedJoinOptions.MapResultToNavigationProperties,
                referencedJoinOptions.ExtraWhereClause,
                referencedJoinOptions.ExtraOrderClause,
                referencedJoinOptions.JoinOnClause,
                referencingToReferencedRelationshipType,
                referencedJoinOptions.ReferencingEntityDescriptor,
                referencingEntityRegistration,
                referencedJoinOptions.ReferencingEntityFormatterResolver,
                referencingNavigationProperty,
                referencingNavigationProperty?.IsEntityCollectionProperty() == true,
                referencingColumnProperties,
                referencedJoinOptions.ReferencedEntityDescriptor,
                referencedJoinOptions.ReferencedEntityRegistration,
                referencedJoinOptions.ReferencedEntityFormatterResolver,
                referencedNavigationProperty,
                referencedNavigationProperty?.IsEntityCollectionProperty() == true,
                referencedColumnProperties);
        }
    }
}
