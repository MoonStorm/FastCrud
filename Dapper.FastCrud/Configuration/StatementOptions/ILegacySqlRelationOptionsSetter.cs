namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Includes the legacy set of options used in JOIN statements.
    /// </summary>
    /// <typeparam name="TReferredEntity">Joined entity</typeparam>
    /// <typeparam name="TStatementOptionsBuilder">Options builder</typeparam>
    [Obsolete(message: "Will be removed in a future version.", error: false)]
    public interface ILegacySqlRelationOptionsSetter<TReferredEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TReferredEntity>? entityMapping);

        /// <summary>
        /// Extra filter for an ON clause in a JOIN.
        /// </summary>
        TStatementOptionsBuilder Where(FormattableString? whereClause);

        /// <summary>
        /// Adds an ORDER BY clause to the main statement.
        /// </summary>
        TStatementOptionsBuilder OrderBy(FormattableString? orderByClause);

        /// <summary>
        /// Sets the type of the JOIN to a LEFT OUTER JOIN.
        /// </summary>
        TStatementOptionsBuilder LeftOuterJoin();

        /// <summary>
        /// Sets the type of the JOIN to an INNER JOIN.
        /// </summary>
        TStatementOptionsBuilder InnerJoin();
    }
}