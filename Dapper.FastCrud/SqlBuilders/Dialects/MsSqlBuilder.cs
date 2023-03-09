namespace Dapper.FastCrud.SqlBuilders.Dialects
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using System;
    using System.Linq;

    /// <summary>
    /// Statement builder for the <seealso cref="SqlDialect.MsSql"/>.
    /// </summary>
    internal class MsSqlBuilder : GenericStatementSqlBuilder
    {
        public MsSqlBuilder(EntityDescriptor entityDescriptor, EntityRegistration entityMapping)
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
                return FormattableString.Invariant($"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()})");
            }

            // one database generated field to be returned, and that alone is the primary key
            if (this.InsertKeyDatabaseGeneratedProperties.Length == 1 && this.RefreshOnInsertProperties.Length == 1)
            {
                var keyProperty = this.InsertKeyDatabaseGeneratedProperties[0];
                var keyPropertyType = keyProperty.Descriptor.PropertyType;

                if (keyPropertyType == typeof(int) || keyPropertyType == typeof(long))
                {
                    return FormattableString.Invariant($@"
                           INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) 
                                                             VALUES ({this.ConstructParamEnumerationForInsert()});
                           SELECT SCOPE_IDENTITY() AS {this.GetDelimitedIdentifier(keyProperty.PropertyName)}");
                }
            }

            // more than one field to be returned or not the primary key

            // the union will make the constraints be ignored
            // we only record the primary keys and not all the properties that need to be refreshed on insert because of timestamp/rowversion columns
            // we can't use simple OUTPUT vars (other than the keys) because of triggers
            var keyColumnRegularEnumeration = string.Join(",", this.KeyProperties.Select(prop => this.GetColumnName(prop)));
            var keyColumnInsertEnumeration = string.Join(",", this.KeyProperties.Select(prop => FormattableString.Invariant($"inserted.{this.GetColumnName(prop)}")));
            return FormattableString.Invariant($@"
               SELECT * 
                    INTO #temp 
                    FROM (
                        SELECT {keyColumnRegularEnumeration} FROM {this.GetTableName()} WHERE 1=0 
                        UNION 
                        SELECT {keyColumnRegularEnumeration} FROM {this.GetTableName()} WHERE 1=0
                    ) as [combined];

                INSERT  INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) 
                        OUTPUT {keyColumnInsertEnumeration} INTO #temp 
                        VALUES ({this.ConstructParamEnumerationForInsert()});

                SELECT {string.Join(",", this.RefreshOnInsertProperties.Select(prop => this.GetColumnName(prop, "main", true)))}
                    FROM {this.GetTableName()} AS {this.GetDelimitedIdentifier("main")}
                    INNER JOIN #temp
                        ON {string.Join(" AND ", this.KeyProperties.Select(prop => FormattableString.Invariant($"{this.GetColumnName(prop, "main")} = {this.GetColumnName(prop, "#temp")}")))}
            ");
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

            //var dbUpdatedOutputColumns = string.Join(",", this.RefreshOnUpdateProperties.Select(propInfo => $"inserted.{this.GetColumnName(propInfo, null, true)}"));
            //var dbGeneratedColumns = string.Join(",", this.RefreshOnUpdateProperties.Select(propInfo => $"{this.GetColumnName(propInfo, null, true)}"));
            //// the union will make the constraints be ignored
            //return FormattableString.Invariant($@"
            //    SELECT *
            //        INTO #temp 
            //        FROM (SELECT {dbGeneratedColumns} FROM {this.GetTableName()} WHERE 1=0 
            //            UNION SELECT {dbGeneratedColumns} FROM {this.GetTableName()} WHERE 1=0) as u;

            //    UPDATE {this.GetTableName()} 
            //        SET {this.ConstructUpdateClause()}
            //        OUTPUT {dbUpdatedOutputColumns} INTO #temp
            //        WHERE {this.ConstructKeysWhereClause()}

            //    SELECT * FROM #temp");

            // more than one field to be returned or not the primary key

            // we can't use simple OUTPUT vars (other than the keys) because of triggers
            var keyColumnRegularEnumeration = string.Join(",", this.KeyProperties.Select(prop => this.GetColumnName(prop)));
            return FormattableString.Invariant($@"
                UPDATE {this.GetTableName()} 
                    SET {this.ConstructUpdateClause()}
                    WHERE {this.ConstructKeysWhereClause()}

                SELECT {string.Join(",", this.RefreshOnUpdateProperties.Select(prop => this.GetColumnName(prop, null, true)))}
                    FROM {this.GetTableName()}
                    WHERE {this.ConstructKeysWhereClause()}
            ");
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

            if (skipRowsCount.HasValue || limitRowsCount.HasValue)
            {
                sql = $"{sql}  OFFSET {skipRowsCount ?? 0} ROWS";
            }

            if (limitRowsCount.HasValue)
            {
                sql = $"{sql} FETCH NEXT {limitRowsCount} ROWS ONLY";
            }

            return FormattableString.Invariant(sql);
        }
    }
}
