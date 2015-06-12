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

        string ConstructFullSingleSelectStatement();
        string ConstructFullBatchSelectStatement(
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null);
        string ConstructFullInsertStatement();
        string ConstructFullUpdateStatement();
        string ConstructFullDeleteStatement();
        string GetColumnName(PropertyMapping propMapping, string alias = null);
    }
}
