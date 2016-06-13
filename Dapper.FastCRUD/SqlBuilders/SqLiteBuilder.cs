namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;

    internal class SqLiteBuilder:GenericStatementSqlBuilder
    {
        public SqLiteBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.SqLite)
        {
            if (this.KeyProperties.Length > 1)
            {
                throw new NotSupportedException($"Entity <{entityMapping.EntityType.Name}> has more than one primary keys. This is not supported by SqLite.");
            }
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
                    sql += this.ResolveWithCultureInvariantFormatter($"SELECT last_insert_rowid() as {this.GetDelimitedIdentifier(this.InsertKeyDatabaseGeneratedProperties[0].PropertyName)};");
                }
                else
                {
                    var databaseGeneratedColumnSelection = string.Join(
                        ",",
                        this.RefreshOnInsertProperties.Select(
                            propInfo => this.GetColumnName(propInfo, null, true)));
                    sql += this.ResolveWithCultureInvariantFormatter($"SELECT {databaseGeneratedColumnSelection} FROM {this.GetTableName()} WHERE {this.GetColumnName(this.InsertKeyDatabaseGeneratedProperties[0], null, false)} = last_insert_rowid();");
                }
            }

            return sql;
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

            if (limitRowsCount.HasValue || skipRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " LIMIT {0}", limitRowsCount??-1);
            }
            if (skipRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " OFFSET {0}", skipRowsCount);
            }

            return sql;
        }
    }
}
