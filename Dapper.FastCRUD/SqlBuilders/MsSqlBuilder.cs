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

        /// <summary>
        /// Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            if (this.InsertDatabaseGeneratedProperties.Length == 0)
            {
                return $"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()})".ToString(CultureInfo.InvariantCulture);                
            }

            // one database generated field to be inserted, and that alone is a the primary key
            if (this.InsertKeyDatabaseGeneratedProperties.Length == 1 && this.InsertDatabaseGeneratedProperties.Length == 1)
            {
                var keyProperty = this.InsertKeyDatabaseGeneratedProperties[0];
                var keyPropertyType = keyProperty.Descriptor.PropertyType;

                if (keyPropertyType == typeof(int) || keyPropertyType == typeof(long))
                {
                    return
                        $@"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()});
                           SELECT SCOPE_IDENTITY() AS {this.GetDelimitedIdentifier(keyProperty.PropertyName)}".ToString(CultureInfo.InvariantCulture);
                }
            }

            var dbInsertedOutputColumns = string.Join(",", this.InsertDatabaseGeneratedProperties.Select(propInfo => $"inserted.{this.GetColumnName(propInfo, null, true)}"));
            var dbGeneratedColumns = string.Join(",", this.InsertDatabaseGeneratedProperties.Select(propInfo => $"{this.GetColumnName(propInfo, null, true)}"));

            return $@"
                SELECT *
                    INTO #temp 
                    FROM (SELECT {dbGeneratedColumns} FROM {this.GetTableName()} WHERE 1=0 
                        UNION SELECT {dbGeneratedColumns} FROM {this.GetTableName()} WHERE 1=0) as u;
            
                INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) 
                    OUTPUT {dbInsertedOutputColumns} INTO #temp 
                    VALUES ({this.ConstructParamEnumerationForInsert()});

                SELECT * FROM #temp".ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Constructs a full batch select statement
        /// </summary>
        protected override string ConstructFullBatchSelectStatementInternal(
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null,
            object queryParameters = null)
        {
            var sql = $"SELECT {this.ConstructColumnEnumerationForSelect()} FROM {this.GetTableName()}".ToString(CultureInfo.InvariantCulture);
            if (whereClause != null)
            {
                sql += string.Format(this.StatementFormatter, " WHERE {0}", whereClause);
            }
            if (orderClause != null)
            {
                sql += string.Format(this.StatementFormatter, " ORDER BY {0}", orderClause);
            }
            if (skipRowsCount.HasValue || limitRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " OFFSET {0} ROWS", skipRowsCount??0);
            }
            if (limitRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " FETCH NEXT {0} ROWS ONLY", limitRowsCount);
            }

            return sql;
        }
    }
}
