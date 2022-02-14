namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions.Builders;

    /// <summary>
    /// Statement options for entity relationships
    /// </summary>
    public interface IRelationalSqlStatementOptionsSetter<TStatementOptionsBuilder>
    {
        /// <summary>
        /// Sets up an alias for the main entity to be used in a relationship.
        /// It is recommended to add aliases to the joined entities as well.
        /// </summary>
        public TStatementOptionsBuilder WithAlias(string? mainEntityAlias);
        
        /// <summary>
        /// Includes a referenced entity into the query.
        /// The referencing entity will tried to be inferred.
        /// If more than one relationship was found between one of the existing entities and <seealso cref="TReferencedEntity"/>, use the join relationship options to add more information to be used in locating the relationship.
        /// </summary>
        TStatementOptionsBuilder Include<TReferencedEntity> (Action<ISqlJoinOptionsBuilder<TReferencedEntity>>? join = null);
    }
}
