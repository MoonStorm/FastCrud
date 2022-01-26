namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Linq;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;

    internal class MsSql2008Builder : MsSqlBuilder
    {
        public MsSql2008Builder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.MsSql2008)
        {
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
            string orderBySql = (orderClause == null) ? string.Empty : " ORDER BY " + this.ResolveWithSqlFormatter(orderClause, forceTableColumnResolution);
            string whereSql = (whereClause == null) ? string.Empty : " WHERE " + this.ResolveWithSqlFormatter(whereClause, forceTableColumnResolution);

            if (String.IsNullOrEmpty(orderBySql) || !(skipRowsCount.HasValue || limitRowsCount.HasValue))
            {
                return ResolveWithCultureInvariantFormatter($"SELECT {selectClause} FROM {fromClause} {whereSql} {orderBySql}");
            }

            string sql = "SELECT * FROM (";
            sql += this.ResolveWithCultureInvariantFormatter($"SELECT {selectClause}, ROW_NUMBER() OVER({orderBySql}) AS [Tu3gD4i0_INDEX] FROM {fromClause} {whereSql}");
            sql += ") AS Tu3gD4i0";
            sql += " WHERE " + this.ResolveWithCultureInvariantFormatter($"[Tu3gD4i0_INDEX] BETWEEN {(skipRowsCount ?? 0) + 1} AND {(skipRowsCount ?? 0) + limitRowsCount}");
            sql += " ORDER BY Tu3gD4i0_INDEX ASC"

            return sql;
        }
    }
}
