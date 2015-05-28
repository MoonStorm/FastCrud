using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.FastCrud.SqlBuilders
{
    using System.Globalization;
    using Dapper.FastCrud.Mappings;

    internal class SqLiteBuilder:GenericStatementSqlBuilder
    {
        public SqLiteBuilder(EntityMapping entityMapping)
            : base(entityMapping, false, string.Empty)
        {
            if (this.KeyProperties.Length > 1)
            {
                throw new NotSupportedException($"Entity <{entityMapping.EntityType.Name}> has more than one primary keys. This is not supported by SqLite.");
            }
        }

        public override string ConstructFullInsertStatement()
        {
            var sql = $"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()}); ";

            if (this.KeyDatabaseGeneratedProperties.Length > 0)
            {
                // we have an identity column, so we can fetch the rest of them
                if (this.KeyDatabaseGeneratedProperties.Length == 1)
                {
                    // just one, this is going to be easy
                    sql += $"SELECT last_insert_rowid() as {this.KeyDatabaseGeneratedProperties[0].PropertyName}";
                }
                else
                {
                    var databaseGeneratedColumnSelection = string.Join(
                        ",",
                        this.KeyDatabaseGeneratedProperties.Select(
                            propInfo => $"{ColumnStartDelimiter}{propInfo.DatabaseColumn}{ColumnEndDelimiter} AS {propInfo.PropertyName}"));
                    sql +=
                        $"SELECT {databaseGeneratedColumnSelection} FROM {this.GetTableName()} WHERE {ColumnStartDelimiter}{this.KeyDatabaseGeneratedProperties[0].DatabaseColumn}{ColumnEndDelimiter} = last_insert_rowid()";
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
