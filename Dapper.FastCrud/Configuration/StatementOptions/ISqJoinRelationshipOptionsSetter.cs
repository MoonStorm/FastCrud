namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Includes the set of options used in JOIN statements.
    /// </summary>
    /// <typeparam name="TReferencedEntity">The referenced entity</typeparam>
    /// <typeparam name="TReferencingEntity">The referencing entity</typeparam>
    /// <typeparam name="TStatementOptionsBuilder">Options builder</typeparam>
    public interface ISqJoinRelationshipOptionsSetter<TReferencingEntity, TReferencedEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        /// If set to true, the results are set on <see cref="FromProperty(Expression?)"/> and <see cref="ToProperty(Expression?)"/>.
        /// </summary>
        TStatementOptionsBuilder MapResults(bool mapResults = true);

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        TStatementOptionsBuilder FromAlias(string? referencingEntityAlias);

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        TStatementOptionsBuilder FromProperty(Expression<Func<TReferencingEntity, TReferencedEntity?>>? referencingNavigationProperty);

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        TStatementOptionsBuilder FromProperty(Expression<Func<TReferencingEntity, IEnumerable<TReferencedEntity>?>>? referencingNavigationProperty);

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        TStatementOptionsBuilder ToProperty(Expression<Func<TReferencedEntity, TReferencingEntity?>>? referencedNavigationProperty);

        /// <summary>
        /// Provides more information about the relationship between the two entities.
        /// </summary>
        TStatementOptionsBuilder ToProperty(Expression<Func<TReferencedEntity, IEnumerable<TReferencingEntity>?>>? referencedNavigationProperty);

        /// <summary>
        /// If <paramref name="condition"/> then <paramref name="then"/> else <paramref name="otherwise"/>.
        /// </summary>
        TStatementOptionsBuilder When(bool condition, Func<TStatementOptionsBuilder, TStatementOptionsBuilder> then, Func<TStatementOptionsBuilder, TStatementOptionsBuilder>? otherwise = null);
    }
}