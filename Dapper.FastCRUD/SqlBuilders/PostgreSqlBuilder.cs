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
            string selectClause,
            string fromClause,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null)
        {
            var sql = this.ResolveWithCultureInvariantFormatter($"SELECT {selectClause} FROM {fromClause}");

            if (whereClause != null)
            {
                sql += " WHERE " + this.ResolveWithSqlFormatter(whereClause);
            }
            if (orderClause != null)
            {
                sql += " ORDER BY " + this.ResolveWithSqlFormatter(orderClause);
            }
            if (limitRowsCount.HasValue)
            {
                sql += this.ResolveWithCultureInvariantFormatter($" LIMIT {limitRowsCount}");
            }
            if (skipRowsCount.HasValue)
            {
                sql += this.ResolveWithCultureInvariantFormatter($" OFFSET {skipRowsCount}");
            }

            return sql;
        }
    }
}
