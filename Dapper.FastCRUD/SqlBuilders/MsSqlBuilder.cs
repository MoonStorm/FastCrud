using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.FastCrud.SqlBuilders
{
    using System.Globalization;
    using Dapper.FastCrud.Mappings;

    internal class MsStatementSqlBuilder : GenericStatementSqlBuilder
    {
        public MsStatementSqlBuilder(EntityMapping entityMapping)
            : base(entityMapping, true, "[", "]", "[", "]")
        {
        }

        public override string ConstructFullInsertStatement()
        {
            string outputQuery;
            if (DatabaseGeneratedProperties.Length > 0)
            {
                var outputColumns = string.Join(",", DatabaseGeneratedProperties.Select(propInfo => $"inserted.{propInfo.PropertyName}"));
                outputQuery = $"OUTPUT {outputColumns}";
            }
            else
            {
                outputQuery = string.Empty;
            }

            return $"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) {outputQuery} VALUES ({this.ConstructParamEnumerationForInsert()}) ";
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
                sql += string.Format(CultureInfo.InvariantCulture, " OFFSET {0} ROWS", skipRowsCount);
            }
            if (limitRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " FETCH NEXT {0} ROWS ONLY", limitRowsCount);
            }

            return sql;
        }
    }
}
