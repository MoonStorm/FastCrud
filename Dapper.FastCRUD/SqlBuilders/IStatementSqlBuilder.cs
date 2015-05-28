namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Collections.Generic;
    using Dapper.FastCrud.Mappings;

    internal interface IStatementSqlBuilder:ISqlBuilder
    {
        PropertyMapping[] SelectProperties { get; }
        PropertyMapping[] KeyProperties { get; }
        PropertyMapping[] InsertProperties { get; }
        PropertyMapping[] UpdateProperties { get; }
        PropertyMapping[] KeyDatabaseGeneratedProperties { get; }
        PropertyMapping[] DatabaseGeneratedProperties { get; }

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
    }
}
