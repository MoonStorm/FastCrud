namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;

    internal class MsSqlBuilder : GenericStatementSqlBuilder
    {
        public MsSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.MsSql)
        {
        }

        public override string ConstructFullInsertStatement()
        {
            string outputQuery;
            if (InsertDatabaseGeneratedProperties.Length > 0)
            {
                var outputColumns = string.Join(",", InsertDatabaseGeneratedProperties.Select(propInfo => $"inserted.{this.GetColumnName(propInfo,null,true)}"));
                outputQuery = $"OUTPUT {outputColumns}";
            }
            else
            {
                outputQuery = string.Empty;
            }

            return $"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) {outputQuery} VALUES ({this.ConstructParamEnumerationForInsert()})".ToString(CultureInfo.InvariantCulture);
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
                sql += string.Format(this.StatementFormatter, " WHERE {0}", whereClause);
            }
            if (orderClause != null)
            {
                sql += string.Format(this.StatementFormatter, " ORDER BY {0}", orderClause);
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
