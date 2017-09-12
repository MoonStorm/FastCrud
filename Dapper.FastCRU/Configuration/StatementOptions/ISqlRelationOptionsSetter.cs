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
        /// Overrides the entity mapping for the current statement.
        /// </summary>
        TStatementOptionsBuilder WithEntityMappingOverride(EntityMapping<TReferredEntity> entityMapping);

        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        TStatementOptionsBuilder Where(FormattableString whereClause);

        /// <summary>
        /// Adds an ORDER BY clause to the statement.
        /// </summary>
        TStatementOptionsBuilder OrderBy(FormattableString orderByClause);

        /// <summary>
        /// A left outer join is desired.
        /// </summary>
        TStatementOptionsBuilder LeftOuterJoin();

        /// <summary>
        /// An inner join is desired.
        /// </summary>
        TStatementOptionsBuilder InnerJoin();

        ///// <summary>
        ///// A right outer join is desired.
        ///// </summary>
        //TStatementOptionsBuilder RightOuterJoin { get; }
    }
}