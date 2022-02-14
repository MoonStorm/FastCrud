namespace Dapper.FastCrud.SqlStatements.MultiEntity
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Formatters.Contexts;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Holds resolved information about a SQL join relationship.
    /// </summary>
    internal class SqlStatementJoinRelationship
    {
        /// <summary>
        /// Constructor taking a set of options related to a relationship.
        /// </summary>
        public SqlStatementJoinRelationship(
            SqlStatementFormatterResolver referencingEntityResolver,
            PropertyDescriptor? referencingEntityNavigationProperty,
            PropertyRegistration[]? referencingEntityColumnProperties,

            SqlStatementFormatterResolver referencedEntityResolver,
            PropertyDescriptor? referencedEntityNavigationProperty,
            PropertyRegistration[]? referencedEntityColumnProperties,

            bool mapResults
        )
        {
            Requires.NotNull(referencingEntityResolver, nameof(referencingEntityResolver));
            Requires.NotNull(referencedEntityResolver, nameof(referencedEntityResolver));

            this.ReferencingEntityFormatterResolver = referencingEntityResolver;
            this.ReferencingNavigationProperty = referencingEntityNavigationProperty;
            this.ReferencingNavigationPropertyIsCollection = this.ReferencingNavigationProperty?.IsEntityCollectionProperty() == true;
            this.ReferencingColumnProperties = referencingEntityColumnProperties;

            this.ReferencedEntityFormatterResolver = referencedEntityResolver;
            this.ReferencedNavigationProperty = referencedEntityNavigationProperty;
            this.ReferencedNavigationPropertyIsCollection = this.ReferencedNavigationProperty?.IsEntityCollectionProperty() == true;
            this.ReferencedColumnProperties = referencedEntityColumnProperties;

            this.MapResults = mapResults;
        }

        /// <summary>
        /// If true, the map results should be set in <see cref="ReferencingNavigationProperty"/> and <see cref="ReferencedNavigationProperty"/>, if available.
        /// </summary>
        public bool MapResults { get; }

        /// <summary>
        /// Holds the entity descriptor for the referencing entity descriptor.
        /// </summary>
        public EntityDescriptor ReferencingEntityDescriptor => this.ReferencingEntityFormatterResolver.EntityDescriptor;

        /// <summary>
        /// Holds the entity descriptor for the referenced entity descriptor.
        /// </summary>
        public EntityDescriptor ReferencedEntityDescriptor => this.ReferencedEntityFormatterResolver.EntityDescriptor;

        /// <summary>
        /// Holds the SQL Formatter for the referencing entity.
        /// </summary>
        public SqlStatementFormatterResolver ReferencingEntityFormatterResolver { get; }

        /// <summary>
        /// Holds the SQL Formatter for the referenced entity.
        /// </summary>
        public SqlStatementFormatterResolver ReferencedEntityFormatterResolver { get; }

        /// <summary>
        /// Holds the entity registration for the referencing entity.
        /// </summary>
        public EntityRegistration ReferencingEntityRegistration => this.ReferencingEntityFormatterResolver.EntityRegistration;

        /// <summary>
        /// Holds the entity registration for the referenced entity.
        /// </summary>
        public EntityRegistration ReferencedEntityRegistration => this.ReferencedEntityFormatterResolver.EntityRegistration;

        /// <summary>
        /// Holds the entity alias for the referencing entity.
        /// </summary>
        public string? ReferencingEntityAlias => this.ReferencingEntityFormatterResolver.Alias;

        /// <summary>
        /// Holds the entity alias for the referenced entity.
        /// </summary>
        public string? ReferencedEntityAlias => this.ReferencedEntityFormatterResolver.Alias;

        /// <summary>
        /// Holds the SQL builder responsible for the referencing entity.
        /// </summary>
        public GenericStatementSqlBuilder ReferencingEntitySqlBuilder => this.ReferencingEntityFormatterResolver.SqlBuilder;

        /// <summary>
        /// Holds the SQL builder responsible for the referencing entity.
        /// </summary>
        public GenericStatementSqlBuilder ReferencedEntitySqlBuilder => this.ReferencedEntityFormatterResolver.SqlBuilder;

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
    }
}
