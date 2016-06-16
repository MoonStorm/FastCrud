namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Linq;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;

    internal class MySqlBuilder:GenericStatementSqlBuilder
    {
        public MySqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.MySql)
        {
        }

        /// <summary>
        /// Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            var sql = this.ResolveWithCultureInvariantFormatter(
                    $"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()}); ");

            if (this.RefreshOnInsertProperties.Length > 0)
            {
                // we have to bring some column values back
                if (this.InsertKeyDatabaseGeneratedProperties.Length == 0)
                {
                    throw new NotSupportedException($"Entity '{this.EntityMapping.EntityType.Name}' has database generated fields that don't contain a primary key.");
                }

                // we have an identity column, so we can fetch the rest of them
                if (this.InsertKeyDatabaseGeneratedProperties.Length == 1 && this.RefreshOnInsertProperties.Length == 1)
                {
                    // just one, this is going to be easy
                    sql += this.ResolveWithCultureInvariantFormatter($"SELECT LAST_INSERT_ID() as {this.GetDelimitedIdentifier(this.InsertKeyDatabaseGeneratedProperties[0].PropertyName)}");
                }
                else
                {
                    var databaseGeneratedColumnSelection = string.Join(
                        ",",
                        this.RefreshOnInsertProperties.Select(
                            propInfo =>
                            $"{this.GetColumnName(propInfo, null, true)}"));
                    sql += this.ResolveWithCultureInvariantFormatter($"SELECT {databaseGeneratedColumnSelection} FROM {this.GetTableName()} WHERE {this.GetColumnName(this.InsertKeyDatabaseGeneratedProperties[0], null, false)} = LAST_INSERT_ID()");
                }
            }
            return sql;
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

            if (skipRowsCount.HasValue)
            {
                sql += this.ResolveWithCultureInvariantFormatter($" LIMIT {skipRowsCount},{limitRowsCount ?? (int?)int.MaxValue}");
            }
            else if (limitRowsCount.HasValue)
            {
                sql += this.ResolveWithCultureInvariantFormatter($" LIMIT {limitRowsCount}");
            }

            return sql;
        }
    }
}
