namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Dapper.FastCrud.Mappings;

    internal abstract class GenericStatementSqlBuilder:IStatementSqlBuilder
    {
        private ConcurrentDictionary<IStatementSqlBuilder, EntityRelationship> _entityRelationships;

        protected GenericStatementSqlBuilder(EntityMapping entityMapping, bool usesTableSchema, string tableAndColumnDelimiter)
            :this(entityMapping,usesTableSchema,tableAndColumnDelimiter,tableAndColumnDelimiter,tableAndColumnDelimiter,tableAndColumnDelimiter)
        {            
            _entityRelationships = new ConcurrentDictionary<IStatementSqlBuilder, EntityRelationship>();
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

            this.SelectProperties = this.EntityMapping.PropertyMappings
                .Where(propMapping => !propMapping.Value.IsReferencingForeignEntity)
                .Select(propMapping => propMapping.Value)
                .ToArray();
            this.KeyProperties = this.EntityMapping.PropertyMappings
                .Where(propMapping => propMapping.Value.IsPrimaryKey)
                .Select(propMapping => propMapping.Value)
                .ToArray();
            this.InsertDatabaseGeneratedProperties = this.SelectProperties
                .Where(propInfo => propInfo.IsDatabaseGenerated && propInfo.IsExcludedFromInserts)
                .ToArray();
            this.InsertKeyDatabaseGeneratedProperties = this.KeyProperties
                .Intersect(this.InsertDatabaseGeneratedProperties)
                .ToArray();
            this.UpdateProperties = this.SelectProperties
                .Where(propInfo => !propInfo.IsExcludedFromUpdates)
                .ToArray();
            this.InsertProperties = this.SelectProperties
                .Where(propInfo => !propInfo.IsExcludedFromInserts)
                .ToArray();
            this.ForeignEntityProperties =
                this.EntityMapping.PropertyMappings
                .Where(propMapping => propMapping.Value.IsReferencingForeignEntity)
                .Select(propMapping => propMapping.Value)
                .ToArray();
        }

        public virtual EntityRelationship GetRelationship(IStatementSqlBuilder destination)
        {
            return _entityRelationships.GetOrAdd(
                destination,
                (destinationSqlBuilder) => new EntityRelationship(this, destinationSqlBuilder));
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

        public string GetColumnName(string propertyName, string alias = null)
        {
            return this.GetColumnName(this.EntityMapping.PropertyMappings[propertyName], alias);
        }

        public virtual string GetColumnName(PropertyMapping propMapping, string alias = null)
        {
            var sqlAlias = alias == null ? string.Empty : $"{alias}.";
            return $"{sqlAlias}{this.ColumnStartDelimiter}{propMapping.DatabaseColumnName}{this.ColumnEndDelimiter}";
        }

        public virtual string ConstructKeysWhereClause(string alias = null)
        {
            return string.Join(" AND ", this.KeyProperties.Select(propInfo => $"{this.GetColumnName(propInfo, alias)}=@{propInfo.PropertyName}"));
        }

        public virtual string ConstructKeyColumnEnumeration(string alias = null)
        {
            var sqlAlias = alias == null ? string.Empty : $"{alias}.";
            return string.Join(
                ",",
                KeyProperties.Select(propInfo => $"{sqlAlias}{ColumnStartDelimiter}{propInfo.DatabaseColumnName}{ColumnEndDelimiter}"));
        }

        public virtual string ConstructColumnEnumerationForSelect(string alias = null)
        {
            return string.Join(",", this.SelectProperties.Select(propInfo => this.GetColumnName(propInfo, alias)));
        }

        public virtual string ConstructColumnEnumerationForInsert()
        {
            return string.Join(",", this.InsertProperties.Select(propInfo => this.GetColumnName(propInfo)));
        }

        public virtual string ConstructParamEnumerationForInsert()
        {
            return string.Join(",", this.InsertProperties.Select(propInfo => $"@{propInfo.PropertyName}"));
        }

        public virtual string ConstructUpdateClause(string alias = null)
        {
            return string.Join(",", UpdateProperties.Select(propInfo => $"{this.GetColumnName(propInfo, alias)}=@{propInfo.PropertyName}"));
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
        
        public string ConstructFullCountStatement(FormattableString whereClause = null)
        {
            var sql = $"SELECT COUNT({this.ConstructKeyColumnEnumeration()}) FROM {this.GetTableName()}";

            if (whereClause != null)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " WHERE {0}", whereClause);
            }

            return sql;
        }

        public virtual string ConstructFullSingleSelectStatement()
        {
            return $"SELECT {this.ConstructColumnEnumerationForSelect()} FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}";
        }

        public virtual string ConstructMultiSelectStatement(IStatementSqlBuilder[] additionalIncludes)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"SELECT {this.ConstructColumnEnumerationForSelect(this.GetTableName())}");
            foreach (var additionalInclude in additionalIncludes)
            {
                queryBuilder.Append($",{additionalInclude.ConstructColumnEnumerationForSelect(additionalInclude.GetTableName())}");
            }
            queryBuilder.Append($" FROM {this.GetTableName()}");

            IStatementSqlBuilder leftSqlBuilder = this;

            foreach (var rightSqlBuilder in additionalIncludes)
            {
                // find the property linked to this entity
                var atLeastOneLinkProp = false;
                var joinLeftProps = joinLeftSqlBuilder.ForeignEntityProperties
                    .Where(propInfo => propInfo.Descriptor.PropertyType == additionalInclude.EntityMapping.EntityType)
                    .Select(propInfo => joinLeftSqlBuilder.EntityMapping.PropertyMappings[propInfo.PropertyName]);

                foreach (var joinLeftProp in joinLeftProps)
                {
                    if (!atLeastOneLinkProp)
                    {
                        atLeastOneLinkProp = true;
                        queryBuilder.Append($" LEFT OUTER JOIN {additionalInclude.GetTableName()} ON ");
                    }
                    else
                    {
                        queryBuilder.Append(" AND ");
                    }
                    queryBuilder.Append( $"{joinLeftSqlBuilder.GetColumnName(joinLeftProp, joinLeftSqlBuilder.GetTableName())}={additionalInclude.GetColumnName(joinLeftProp, additionalInclude.GetTableName())}");
                }

                if (!atLeastOneLinkProp)
                {
                    throw new InvalidOperationException($"No foreign key constraint was found between the primary entity {joinLeftSqlBuilder.EntityMapping.EntityType} and the foreign entity {additionalInclude.EntityMapping.EntityType}");
                }
                joinLeftSqlBuilder = additionalInclude;
            }

            return queryBuilder.ToString();
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
        public PropertyMapping[] ForeignEntityProperties { get; private set; }
        public PropertyMapping[] SelectProperties { get; private set; }
        public PropertyMapping[] KeyProperties { get; private set; }
        public PropertyMapping[] InsertProperties { get; private set; }
        public PropertyMapping[] UpdateProperties { get; private set; }
        public PropertyMapping[] InsertKeyDatabaseGeneratedProperties { get; private set; }
        public PropertyMapping[] InsertDatabaseGeneratedProperties { get; private set; }
    }
}
