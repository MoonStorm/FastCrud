namespace Dapper.FastCrud.SqlBuilders.Dialects
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using System;
    using System.Linq;

    /// <summary>
    /// Statement builder for the <seealso cref="SqlDialect.SqlAnywhere"/>.
    /// </summary>
    internal class SqlAnywhereSqlBuilder : GenericStatementSqlBuilder
    {
        public SqlAnywhereSqlBuilder(EntityDescriptor entityDescriptor, EntityRegistration entityRegistration) 
            : base(entityDescriptor, entityRegistration, SqlDialect.SqlAnywhere)
        {
        }

        /// <summary>
        /// Constructs a full insert statement
        /// </summary>
        protected override string ConstructFullInsertStatementInternal()
        {
            var mainInsertClause = FormattableString.Invariant($"INSERT INTO {this.GetTableName()} ({this.ConstructColumnEnumerationForInsert()}) VALUES ({this.ConstructParamEnumerationForInsert()})");
            if (this.RefreshOnInsertProperties.Length == 0)
            {
                return mainInsertClause;
            }

            // we have some columns that are being refreshed by the database on insert, return them
            var dbInsertedOutputColumns = string.Join(",", this.RefreshOnInsertProperties.Select(propInfo => $"inserted.{this.GetColumnName(propInfo, null, true)}"));
            return FormattableString.Invariant($@"
                SELECT {dbInsertedOutputColumns} FROM ({mainInsertClause}) REFERENCING (FINAL AS inserted)                
            ");
        }

        /// <summary>
        /// Constructs an update statement for a single entity.
        /// </summary>
        protected override string ConstructFullSingleUpdateStatementInternal()
        {
            var mainUpdateClause = FormattableString.Invariant($"UPDATE {this.GetTableName()} SET {this.ConstructUpdateClause()} WHERE {this.ConstructKeysWhereClause()}");
            if (this.RefreshOnUpdateProperties.Length == 0)
            {
                return mainUpdateClause;
            }

            var dbUpdatedOutputColumns = string.Join(",", this.RefreshOnUpdateProperties.Select(propInfo => $"updated.{this.GetColumnName(propInfo, null, true)}"));
            return FormattableString.Invariant($@"
                SELECT {dbUpdatedOutputColumns} FROM ({mainUpdateClause}) REFERENCING (FINAL AS updated)                
            ");
        }

        /// <summary>
        /// Constructs a full select statement.
        /// </summary>
        protected override string ConstructFullSelectStatementInternal(
            string selectClause,
            string fromClause,
            string? whereClause = null,
            string? orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null)
        {
            FormattableString sql = $"SELECT";

            if (limitRowsCount.HasValue)
            {
                sql = $"{sql} TOP {limitRowsCount.Value}";
            }

            if (skipRowsCount.HasValue)
            {
                // START AT 1 doesn't skip 1 record, so we need to add 1
                sql = $"{sql} START AT {skipRowsCount.Value + 1}";
            }
            
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
