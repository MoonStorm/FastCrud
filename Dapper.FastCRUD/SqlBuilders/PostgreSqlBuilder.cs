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

        /// <summary>
        /// Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            string outputQuery = this.RefreshOnInsertProperties.Length > 0
             ? string.Format(
                 CultureInfo.InvariantCulture,
                 "RETURNING {0}",
                 string.Join(
                     ",",
                     this.RefreshOnInsertProperties.Select(
                         propInfo => this.GetColumnName(propInfo, null, true))))
             : string.Empty;

            return this.ResolveWithCultureInvariantFormatter($"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()}) {outputQuery}");
        }

        protected override string ConstructFullSelectStatementInternal(
            string selectColumns,
            string fromClause,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null)
        {
            var sql = this.ResolveWithCultureInvariantFormatter($"SELECT {selectColumns} FROM {fromClause}");

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
