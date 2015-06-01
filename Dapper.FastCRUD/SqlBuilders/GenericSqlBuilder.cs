namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dapper.FastCrud.Mappings;

    internal abstract class GenericStatementSqlBuilder:IStatementSqlBuilder
    {
        protected GenericStatementSqlBuilder(EntityMapping entityMapping, bool usesTableSchema, string tableAndColumnDelimiter)
            :this(entityMapping,usesTableSchema,tableAndColumnDelimiter,tableAndColumnDelimiter,tableAndColumnDelimiter,tableAndColumnDelimiter)
        {            
        }

        protected GenericStatementSqlBuilder(
            EntityMapping entityMapping,
            bool usesTableSchema,
            string tableStartDelimiter,
            string tableEndDelimiter,
            string columnStartDelimiter,
            string columnEndDelimiter
            )
        {
            this.EntityMapping = entityMapping;
            this.UsesSchemaForTableNames = usesTableSchema;
            this.TableStartDelimiter = tableStartDelimiter;
            this.TableEndDelimiter = tableEndDelimiter;
            this.ColumnStartDelimiter = columnStartDelimiter;
            this.ColumnEndDelimiter = columnEndDelimiter;

            this.SelectProperties = this.EntityMapping.PropertyMappings.Select(propMapping => propMapping.Value).ToArray();
            this.KeyProperties = this.EntityMapping.PropertyMappings.Where(propMapping => propMapping.Value.IsKey).Select(propMapping => propMapping.Value).ToArray();
            this.DatabaseGeneratedProperties = this.SelectProperties.Where(propInfo => propInfo.IsDatabaseGenerated).ToArray();
            this.KeyDatabaseGeneratedProperties = this.KeyProperties.Intersect(this.DatabaseGeneratedProperties).ToArray();
            this.UpdateProperties = this.SelectProperties.Except(this.KeyProperties).Where(propInfo => !propInfo.IsExcludedFromUpdates).ToArray();
            this.InsertProperties = this.SelectProperties.Except(this.DatabaseGeneratedProperties).ToArray();
        }

        public virtual string GetTableName(string alias = null)
        {
            var sqlAlias = alias == null
                ? string.Empty
                : $" AS {alias}";

            var fullTableName = ((!this.UsesSchemaForTableNames) || string.IsNullOrEmpty(EntityMapping.SchemaName))
                                ? $"{TableStartDelimiter}{EntityMapping.TableName}{TableEndDelimiter}"
                                : $"{TableStartDelimiter}{EntityMapping.SchemaName}{TableEndDelimiter}.{TableStartDelimiter}{EntityMapping.TableName}{TableEndDelimiter}";
            return $"{fullTableName}{sqlAlias}";
        }

        public virtual string ConstructKeysWhereClause(string alias = null)
        {
            var sqlAlias = alias == null ? string.Empty : $"{alias}.";
            return string.Join(
                " AND ",
                KeyProperties.Select(propInfo => $"{sqlAlias}{ColumnStartDelimiter}{propInfo.DatabaseColumnName}{ColumnEndDelimiter}=@{propInfo.PropertyName}"));
        }

        public virtual string ConstructColumnEnumerationForSelect(string alias = null)
        {
            var sqlAlias = alias == null ? string.Empty : $"{alias}.";
            return string.Join(
                ",",
                SelectProperties.Select(
                    propInfo =>
                        {
                            if (propInfo.DatabaseColumnName != propInfo.PropertyName)
                            {
                                return $"{sqlAlias}{ColumnStartDelimiter}{propInfo.DatabaseColumnName}{ColumnEndDelimiter} AS {propInfo.PropertyName}";
                            }

                            return $"{sqlAlias}{propInfo.DatabaseColumnName}";
                        }));
        }

        public virtual string ConstructColumnEnumerationForInsert()
        {
            return string.Join(",", InsertProperties.Select(propInfo => $"{ColumnStartDelimiter}{propInfo.DatabaseColumnName}{ColumnEndDelimiter}"));
        }

        public virtual string ConstructParamEnumerationForInsert()
        {
            return string.Join(",", InsertProperties.Select(propInfo => $"@{propInfo.PropertyName}"));
        }

        public virtual string ConstructUpdateClause(string alias = null)
        {
            var sqlAlias = alias == null ? string.Empty : $"{alias}.";
            return string.Join(
                ",",
                UpdateProperties.Select(propInfo => $"{sqlAlias}{ColumnStartDelimiter}{propInfo.DatabaseColumnName}{ColumnEndDelimiter}=@{propInfo.PropertyName}"));
        }

        public abstract string ConstructFullInsertStatement();

        public virtual string ConstructFullUpdateStatement()
        {
            return $"UPDATE {this.GetTableName()} SET {this.ConstructUpdateClause()} WHERE {this.ConstructKeysWhereClause()}";
        }

        public virtual string ConstructFullDeleteStatement()
        {
            return $"DELETE FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}";
        }

        public virtual string ConstructFullSingleSelectStatement()
        {
            return $"SELECT {this.ConstructColumnEnumerationForSelect()} FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}";
        }

        public abstract string ConstructFullBatchSelectStatement(
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null);

        protected string TableStartDelimiter { get; private set; }
        protected string TableEndDelimiter { get; private set; }
        protected string ColumnStartDelimiter { get; private set; }
        protected string ColumnEndDelimiter { get; private set; }
        protected bool UsesSchemaForTableNames { get; private set; }

        public EntityMapping EntityMapping { get; private set; }
        public PropertyMapping[] SelectProperties { get; private set; }
        public PropertyMapping[] KeyProperties { get; private set; }
        public PropertyMapping[] InsertProperties { get; private set; }
        public PropertyMapping[] UpdateProperties { get; private set; }
        public PropertyMapping[] KeyDatabaseGeneratedProperties { get; private set; }
        public PropertyMapping[] DatabaseGeneratedProperties { get; private set; }
    }
}
