namespace Dapper.FastCrud.SqlBuilders.Dialects
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using System;

    /// <summary>
    /// Statement builder for the <seealso cref="SqlDialect.PostgreSql"/>.
    /// </summary>
    internal class PostgreSqlBuilder : GenericStatementSqlBuilder
    {
        public PostgreSqlBuilder(EntityDescriptor entityDescriptor, EntityRegistration entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.PostgreSql)
        {
        }

        /// <summary>
        /// Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            string outputQuery = this.RefreshOnInsertProperties.Length > 0
                                     ? this.ResolveWithCultureInvariantFormatter($"RETURNING {this.ConstructRefreshOnInsertColumnSelection()}")
                                     : string.Empty;

            return this.ResolveWithCultureInvariantFormatter($"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()}) {outputQuery}");
        }

        protected override string ConstructFullSelectStatementInternal(
            string selectClause,
            string fromClause,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null,
            bool forceTableColumnResolution = false)
        {
            var sql = this.ResolveWithCultureInvariantFormatter($"SELECT {selectClause} FROM {fromClause}");

            if (whereClause != null)
            {
                sql += " WHERE " + this.ResolveWithSqlFormatter(whereClause, forceTableColumnResolution);
            }

            if (orderClause != null)
            {
                sql += " ORDER BY " + this.ResolveWithSqlFormatter(orderClause, forceTableColumnResolution);
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
