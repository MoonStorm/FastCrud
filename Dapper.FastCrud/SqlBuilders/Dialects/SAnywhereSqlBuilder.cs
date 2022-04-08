namespace Dapper.FastCrud.SqlBuilders.Dialects
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Statement builder for the <seealso cref="SqlDialect.SAnywhereSql"/>.
    /// </summary>
    internal class SAnywhereSqlBuilder : GenericStatementSqlBuilder
    {
        public SAnywhereSqlBuilder(EntityDescriptor entityDescriptor, EntityRegistration entityRegistration) 
            : base(entityDescriptor, entityRegistration, SqlDialect.SAnywhereSql)
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

            var dbInsertedOutputColumns = string.Join(",", this.RefreshOnInsertProperties.Select(propInfo => $"inserted.{this.GetColumnName(propInfo, null, true)}"));
            var dbGeneratedColumns = this.ConstructRefreshOnInsertColumnSelection();

            // the union will make the constraints be ignored
            return FormattableString.Invariant($@"
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
            return FormattableString.Invariant($@"
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
            string? whereClause = null,
            string? orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null)
        {
            FormattableString sql = $"SELECT";
            
            if (limitRowsCount.HasValue) sql = $"{sql} TOP {limitRowsCount.Value}";
            if (skipRowsCount.HasValue)  sql = $"{sql} START AT {skipRowsCount.Value}";
            
            sql = $"{sql} {selectClause} FROM {fromClause}";

            if (whereClause != null)
            {
                sql = $"{sql} WHERE {whereClause}";
            }

            if (orderClause != null)
            {
                sql = $"{sql} ORDER BY {orderClause}";
            }

            return FormattableString.Invariant(sql);
        }
    }
}
