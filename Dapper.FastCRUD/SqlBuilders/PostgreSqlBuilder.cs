namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;

    internal class PostgreSqlBuilder : GenericStatementSqlBuilder
    {
        public PostgreSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.PostgreSql)
        {
        }

        public override string ConstructFullInsertStatement()
        {
            string outputQuery = this.InsertDatabaseGeneratedProperties.Length > 0
                         ? string.Format(
                             CultureInfo.InvariantCulture,
                             "RETURNING {0}",
                             string.Join(
                                 ",",
                                 this.InsertDatabaseGeneratedProperties.Select(
                                     propInfo => this.GetColumnName(propInfo,null,true))))
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
                sql += string.Format(this.StatementFormatter, " WHERE {0}", whereClause);
            }
            if (orderClause != null)
            {
                sql += string.Format(this.StatementFormatter, " ORDER BY {0}", orderClause);
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
