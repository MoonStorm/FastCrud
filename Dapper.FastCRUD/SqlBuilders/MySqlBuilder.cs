namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Dapper.FastCrud.Mappings;

    internal class MySqlBuilder:GenericStatementSqlBuilder
    {
        public MySqlBuilder(EntityMapping entityMapping)
            : base(entityMapping, false, "`")
        {
        }

        public override string ConstructFullInsertStatement()
        {
            var sql = $"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()}); ";

            if (this.KeyDatabaseGeneratedProperties.Length > 0)
            {
                // we have an identity column, so we can fetch the rest of them
                if(this.KeyDatabaseGeneratedProperties.Length == 1 && this.DatabaseGeneratedProperties.Length == 1)
                {
                    // just one, this is going to be easy
                    sql += $"SELECT LAST_INSERT_ID() as {this.KeyDatabaseGeneratedProperties[0].PropertyName}";
                }
                else
                {
                    var databaseGeneratedColumnSelection = string.Join(
                        ",",
                        this.DatabaseGeneratedProperties.Select(
                            propInfo => $"{ColumnStartDelimiter}{propInfo.DatabaseColumnName}{ColumnEndDelimiter} AS {propInfo.PropertyName}"));
                    sql +=
                        $"SELECT {databaseGeneratedColumnSelection} FROM {this.GetTableName()} WHERE {ColumnStartDelimiter}{this.KeyDatabaseGeneratedProperties[0].DatabaseColumnName}{ColumnEndDelimiter} = LAST_INSERT_ID()";
                }
            }
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

            if (skipRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " LIMIT {0},{1}", skipRowsCount, limitRowsCount ?? (int?)int.MaxValue);
            }
            else if (limitRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " LIMIT {0}", limitRowsCount);
            }

            return sql;
        }
    }
}
