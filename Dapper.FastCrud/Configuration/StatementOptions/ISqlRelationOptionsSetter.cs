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
        /// Sets up an alias for the entity participating in the JOIN.
        /// Remember to use this alias everywhere in the query.
        /// </summary>
        TStatementOptionsBuilder WithAlias(string? tableAlias);

        /// <summary>
        /// Sets up the ON clause on the query. Remember to use the alias for the related entity in case it was set with <seealso cref="WithAlias"/>.
        /// In case the relationship is already known through the mapping, calling this method will override the implicit SQL you'd normally get for the JOIN.
        /// However in this case it is recommended to use the final WHERE clause on the main query.
        /// </summary>
        TStatementOptionsBuilder On(FormattableString? onClause);

        /// <summary>
        /// Either maps or not the result of the query onto the navigation property, if one was provided.
        /// </summary>
        TStatementOptionsBuilder MapResults(bool mapResults = true);
    }
}