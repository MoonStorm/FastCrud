namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;

    /// <summary>
    /// Statement options for entity relationships
    /// </summary>
    public interface IRelationalStatementOptionsSetter<TReferencingEntity, TStatementOptionsSetter>
        :IStandardSqlStatementOptionsSetter<TReferencingEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Includes a referred entity into the query. The relationship must be set up prior to calling this method.
        /// </summary>
        TStatementOptionsSetter Include<TReferredEntity> (
            Func<ISqlRelationOptionsSetter<TReferencingEntity, TReferredEntity>, ISqlRelationOptionsSetter<TReferencingEntity, TReferredEntity>> options = null);
    }
}
