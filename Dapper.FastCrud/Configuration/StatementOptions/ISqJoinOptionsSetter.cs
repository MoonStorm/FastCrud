namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using Dapper.FastCrud.Configuration.StatementOptions.Builders;
    using System;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Includes the set of options used in JOIN statements.
    /// </summary>
    /// <typeparam name="TReferencedEntity">Joined entity</typeparam>
    /// <typeparam name="TStatementOptionsBuilder">Options builder</typeparam>
    public interface ISqJoinOptionsSetter<TReferencedEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// Sets up an alias for the main entity to be used in a relationship.
        /// It is recommended to add aliases to the joined entities as well.
        /// </summary>
        TStatementOptionsBuilder WithAlias(string? referencedEntityAlias);

        /// <summary>
        /// Overrides the mapping for the entity participating in the JOIN.
        /// </summary>
        TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TReferencedEntity>? entityMapping);

        /// <summary>
        /// Sets up the ON clause on the query.
        /// Remember to use aliases instead of the entities in case you called <seealso cref="ToAlias"/> and <seealso cref="FromAlias"/>.
        /// In case the relationship is already known through the mapping, calling this method will override the implicit SQL ON clause you'd normally get automatically for the relationship.
        /// </summary>
        TStatementOptionsBuilder On(FormattableString? onClause);

        /// <summary>
        /// Has no effect on the statement builder.
        /// Provided in case you want to include conditional options.
        /// </summary>
        TStatementOptionsBuilder NoOp();

        /// <summary>
        /// Extra filter appended to the main WHERE clause.
        /// The formattable string is scoped on the referenced entity.
        /// </summary>
        [Obsolete("This method is obsolete. It is recommended to use the main WHERE clause instead.", false)]
        TStatementOptionsBuilder Where(FormattableString? whereClause);

        /// <summary>
        /// Extra ORDER BY clause to the main statement.
        /// The formattable string is scoped on the referenced entity.
        /// </summary>
        [Obsolete("This method is obsolete. It is recommended to use the main ORDER BY clause instead.", false)]
        TStatementOptionsBuilder OrderBy(FormattableString? orderByClause);

        /// <summary>
        /// Sets the type of the JOIN to a LEFT OUTER JOIN.
        /// </summary>
        TStatementOptionsBuilder LeftOuterJoin();

        /// <summary>
        /// Sets the type of the JOIN to an INNER JOIN.
        /// </summary>
        TStatementOptionsBuilder InnerJoin();

        /// <summary>
        /// If set tot true, mapping relationships will be set on the navigation properties.
        /// This flag can be overriden in a specific relationship by <seealso cref="ISqJoinRelationshipOptionsSetter{TReferencingEntity,TReferencedEntity,TStatementOptionsBuilder}.MapResults"/>.
        /// </summary>
        TStatementOptionsBuilder MapResults(bool mapResults = true);

        /// <summary>
        /// Specifies the referencing entity inside a relationships.
        /// </summary>
        TStatementOptionsBuilder Referencing<TReferencingEntity>(Action<ISqlJoinRelationshipOptionsBuilder<TReferencingEntity, TReferencedEntity>>? relationship = null);
    }
}