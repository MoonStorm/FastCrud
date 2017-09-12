namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Linq;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;

    internal class MsSqlBuilder : GenericStatementSqlBuilder
    {
        public MsSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
            : base(entityDescriptor, entityMapping, SqlDialect.MsSql)
        {
        }

        /// <summary>
        /// Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            if (this.RefreshOnInsertProperties.Length == 0)
            {
                return this.ResolveWithCultureInvariantFormatter($"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()})");
            }

            // one database generated field to be inserted, and that alone is a the primary key
            if (this.InsertKeyDatabaseGeneratedProperties.Length == 1 && this.RefreshOnInsertProperties.Length == 1)
            {
                var keyProperty = this.InsertKeyDatabaseGeneratedProperties[0];
                var keyPropertyType = keyProperty.Descriptor.PropertyType;

                if (keyPropertyType == typeof(int) || keyPropertyType == typeof(long))
                {
                    return
                        this.ResolveWithCultureInvariantFormatter(
                            $@"INSERT 
                                    INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) 
                                    VALUES ({this.ConstructParamEnumerationForInsert()});
                           SELECT SCOPE_IDENTITY() AS {this.GetDelimitedIdentifier(keyProperty.PropertyName)}");
                }
            }

            var dbInsertedOutputColumns = string.Join(",", this.RefreshOnInsertProperties.Select(propInfo => $"inserted.{this.GetColumnName(propInfo, null, true)}"));
            var dbGeneratedColumns = this.ConstructRefreshOnInsertColumnSelection();

            // the union will make the constraints be ignored
            return this.ResolveWithCultureInvariantFormatter($@"
                SELECT *
                    INTO #temp 
                    FROM (SELECT {dbGeneratedColumns} FROM {this.GetTableName()} WHERE 1=0 
                        UNION SELECT {dbGeneratedColumns} FROM {this.GetTableName()} WHERE 1=0) as u;
            
                INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) 
                    OUTPUT {dbInsertedOutputColumns} INTO #temp 
                    VALUES ({this.ConstructParamEnumerationForInsert()});

                SELECT * FROM #temp");
        }

        /// <summary>
        /// Constructs an update statement for a single entity.
        /// </summary>
        protected override string ConstructFullSingleUpdateStatementInternal()
        {
            if (this.RefreshOnUpdateProperties.Length == 0)
            {
                return base.ConstructFullSingleUpdateStatementInternal();
            }

            var dbUpdatedOutputColumns = string.Join(",", this.RefreshOnUpdateProperties.Select(propInfo => $"inserted.{this.GetColumnName(propInfo, null, true)}"));
            var dbGeneratedColumns = string.Join(",", this.RefreshOnUpdateProperties.Select(propInfo => $"{this.GetColumnName(propInfo, null, true)}"));

            // the union will make the constraints be ignored
            return this.ResolveWithCultureInvariantFormatter($@"
                SELECT *
                    INTO #temp 
                    FROM (SELECT {dbGeneratedColumns} FROM {this.GetTableName()} WHERE 1=0 
                        UNION SELECT {dbGeneratedColumns} FROM {this.GetTableName()} WHERE 1=0) as u;

                UPDATE {this.GetTableName()} 
                    SET {this.ConstructUpdateClause()}
                    OUTPUT {dbUpdatedOutputColumns} INTO #temp
                    WHERE {this.ConstructKeysWhereClause()}

                SELECT * FROM #temp");
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
            if (skipRowsCount.HasValue || limitRowsCount.HasValue)
            {
                sql += this.ResolveWithCultureInvariantFormatter($" OFFSET {skipRowsCount ?? 0} ROWS");
            }
            if (limitRowsCount.HasValue)
            {
                sql += this.ResolveWithCultureInvariantFormatter($" FETCH NEXT {limitRowsCount} ROWS ONLY");
            }

            return sql;
        }
    }
}
