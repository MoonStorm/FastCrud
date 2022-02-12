namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Includes the set of options used in JOIN statements.
    /// </summary>
    /// <typeparam name="TReferredEntity">Joined entity</typeparam>
    /// <typeparam name="TStatementOptionsBuilder">Options builder</typeparam>
    public interface ISqlRelationOptionsSetter<TReferredEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// Overrides the mapping for the entity participating in the JOIN.
        /// </summary>
        TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TReferredEntity>? entityMapping);

        /// <summary>
        /// Provides the alias the referencing entity is known as  throughout the statement.
        /// </summary>
        TStatementOptionsBuilder FromAlias(string? referencingEntityAlias);

        /// <summary>
        /// Sets up an alias for the new referenced entity participating in the JOIN.
        /// Remember to use this alias everywhere in the query.
        /// </summary>
        TStatementOptionsBuilder ToAlias(string? referencedEntityAlias);

        /// <summary>
        /// Sets up the ON clause on the query.
        /// Remember to use aliases instead of the entities in case you called <seealso cref="ToAlias"/> and <seealso cref="FromAlias"/>.
        /// In case the relationship is already known through the mapping, calling this method will override the implicit SQL ON clause you'd normally get automatically for the relationship.
        /// </summary>
        TStatementOptionsBuilder On(FormattableString? onClause);

        /// <summary>
        /// Either maps or not the result of the query onto the navigation property, if one was provided.
        /// </summary>
        TStatementOptionsBuilder MapResults(bool mapResults = true);

        /// <summary>
        /// Has no effect on the statement builder.
        /// Provided in case you want to include conditional options.
        /// </summary>
        TStatementOptionsBuilder NoOp();
    }
}