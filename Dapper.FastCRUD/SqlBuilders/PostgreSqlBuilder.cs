namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Dapper.FastCrud.Mappings;

    internal class PostgreStatementSqlBuilder:GenericStatementSqlBuilder
    {
        public PostgreStatementSqlBuilder(EntityMapping entityMapping)
            : base(entityMapping, true, string.Empty)
        {
        }

        public override string ConstructFullInsertStatement()
        {
            string outputQuery = this.KeyDatabaseGeneratedProperties.Length > 0
                         ? string.Format(
                             CultureInfo.InvariantCulture,
                             "RETURNING {0}",
                             string.Join(
                                 ",",
                                 this.KeyDatabaseGeneratedProperties.Select(
                                     propInfo => $"{ColumnStartDelimiter}{propInfo.DatabaseColumn}{ColumnEndDelimiter} AS {propInfo.PropertyName}")))
                         : string.Empty;

            var sql = $"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()}) {outputQuery}";

            return sql;
        }

        public override string ConstructFullBatchSelectStatement(
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null)
        {
            var sql = $"SELECT {this.ConstructColumnEnumerationForSelect()} FROM {this.GetTableName()}";

            if (whereClause != null)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " WHERE {0}", whereClause);
            }
            if (orderClause != null)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " ORDER BY {0}", orderClause);
            }
            if (limitRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " LIMIT {0}", limitRowsCount);
            }
            if (skipRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " OFFSET {0}", skipRowsCount);
            }

            return sql;
        }
    }
}
