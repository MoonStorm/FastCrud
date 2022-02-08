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
                                     ? FormattableString.Invariant($"RETURNING {this.ConstructRefreshOnInsertColumnSelection()}")
                                     : string.Empty;

            return FormattableString.Invariant($"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()}) {outputQuery}");
        }

        protected override string ConstructFullSelectStatementInternal(
            string selectClause,
            string fromClause,
            string? whereClause = null,
            string? orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null)
        {
            FormattableString sql = $"SELECT {selectClause} FROM {fromClause}";

            if (whereClause != null)
            {
                sql = $"{sql} WHERE {whereClause}";
            }

            if (orderClause != null)
            {
                sql = $"{sql} ORDER BY {orderClause}";
            }

            if (limitRowsCount.HasValue)
            {
                sql = $"{sql} LIMIT {limitRowsCount}";
            }

            if (skipRowsCount.HasValue)
            {
                sql = $"{sql} OFFSET {skipRowsCount}";
            }

            return FormattableString.Invariant(sql);
        }
    }
}
