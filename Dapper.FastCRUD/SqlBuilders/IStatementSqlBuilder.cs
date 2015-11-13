namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using Dapper.FastCrud.Mappings;

    internal interface IStatementSqlBuilder:ISqlBuilder
    {
        PropertyMapping[] SelectProperties { get; }
        PropertyMapping[] KeyProperties { get; }
        PropertyMapping[] InsertProperties { get; }
        PropertyMapping[] UpdateProperties { get; }
        PropertyMapping[] InsertKeyDatabaseGeneratedProperties { get; }
        PropertyMapping[] InsertDatabaseGeneratedProperties { get; }
        PropertyMapping[] ForeignEntityProperties { get; }
        EntityMapping EntityMapping { get; }

        /// <summary>
        /// Constructs a select statement for a single entity
        /// </summary>
        string ConstructFullSingleSelectStatement();

        /// <summary>
        /// Constructs a batch select statement
        /// </summary>
        string ConstructFullBatchSelectStatement(
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null,
            object queryParameters = null);

        /// <summary>
        /// Constructs an insert statement for a single entity.
        /// </summary>
        string ConstructFullInsertStatement();

        /// <summary>
        /// Constructs an update statement for a single entity.
        /// </summary>
        string ConstructFullSingleUpdateStatement();

        /// <summary>
        /// Constructs a batch select statement.
        /// </summary>
        string ConstructFullBatchUpdateStatement(FormattableString whereClause = null);

        /// <summary>
        /// Constructs a delete statement for a single entity.
        /// </summary>
        string ConstructFullSingleDeleteStatement();
        
        /// <summary>
        /// Constructs a batch delete statement.
        /// </summary>
        string ConstructFullBatchDeleteStatement(FormattableString whereClause = null);

        /// <summary>
        /// Resolves a property name into a database column name.
        /// </summary>
        /// <param name="propMapping">Property mapping</param>
        /// <param name="tableAlias">Table alias</param>
        /// <param name="performColumnAliasNormalization"></param>
        /// <returns>If true and the database column name differs from the property name, an AS clause will be added</returns>
        string GetColumnName(PropertyMapping propMapping, string tableAlias, bool performColumnAliasNormalization);

        ///// <summary>
        ///// Gets a relationship.
        ///// </summary>
        //EntityRelationship GetRelationship(IStatementSqlBuilder destination);

        /// <summary>
        /// Constructs a full count statement, optionally with a where clause.
        /// </summary>
        string ConstructFullCountStatement(FormattableString whereClause = null);
    }
}
