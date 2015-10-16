namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Mappings;

    internal abstract class GenericStatementSqlBuilder:IStatementSqlBuilder
    {
        private readonly ConcurrentDictionary<IStatementSqlBuilder, EntityRelationship> _entityRelationships;

        protected GenericStatementSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping, bool usesTableSchema, string startEndIdentifierDelimiter)
            :this(entityDescriptor, entityMapping,usesTableSchema,startEndIdentifierDelimiter,startEndIdentifierDelimiter)
        {            
        }

        protected GenericStatementSqlBuilder(
            EntityDescriptor entityDescriptor,
            EntityMapping entityMapping,
            bool usesTableSchema,
            string identifierStartDelimiter,
            string identifierEndDelimiter
            )
        {
            _entityRelationships = new ConcurrentDictionary<IStatementSqlBuilder, EntityRelationship>();
            this.StatementFormatter = new SqlStatementFormatter(entityDescriptor,entityMapping,this);
            this.EntityDescriptor = entityDescriptor;
            this.EntityMapping = entityMapping;
            this.UsesSchemaForTableNames = usesTableSchema;
            this.IdentifierStartDelimiter = identifierStartDelimiter;
            this.IdentifierEndDelimiter = identifierEndDelimiter;

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

        /// <summary>
        /// Gets the statement formatter to be used for parameter resolution.
        /// </summary>
        protected SqlStatementFormatter StatementFormatter { get; }

        /// <summary>
        /// Returns the table name associated with the current entity.
        /// </summary>
        public virtual string GetTableName(string tableAlias = null)
        {
            var sqlAlias = tableAlias == null
                ? string.Empty
                : $" AS {this.GetDelimitedIdentifier(tableAlias)}";

            var fullTableName = ((!this.UsesSchemaForTableNames) || string.IsNullOrEmpty(this.EntityMapping.SchemaName))
                                ? $"{this.GetDelimitedIdentifier(this.EntityMapping.TableName)}"
                                : $"{this.GetDelimitedIdentifier(this.EntityMapping.SchemaName)}.{this.GetDelimitedIdentifier(this.EntityMapping.TableName)}";
            return  $"{fullTableName}{sqlAlias}".ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// </summary>
        public string GetColumnName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property, string tableAlias = null)
        {
            var propName = ((MemberExpression)property.Body).Member.Name;
            return this.GetColumnName(propName, tableAlias);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// </summary>
        public string GetColumnName(string propertyName, string tableAlias = null)
        {
            return this.GetColumnName(this.EntityMapping.PropertyMappings[propertyName], tableAlias, false);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// </summary>
        public virtual string GetColumnName(PropertyMapping propMapping, string tableAlias, bool performColumnAliasNormalization)
        {
            var sqlTableAlias = tableAlias == null ? string.Empty : $"{this.GetDelimitedIdentifier(tableAlias)}.";
            var sqlColumnAlias = (performColumnAliasNormalization && propMapping.DatabaseColumnName != propMapping.PropertyName)
                                     ? $" AS {this.GetDelimitedIdentifier(propMapping.PropertyName)}"
                                     : string.Empty;
            return $"{sqlTableAlias}{this.GetDelimitedIdentifier(propMapping.DatabaseColumnName)}{sqlColumnAlias}".ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Constructs a condition of form <code>ColumnName=@PropertyName and ...</code> with all the key columns (e.g. <code>Id=@Id and EmployeeId=@EmployeeId</code>)
        /// </summary>
        public virtual string ConstructKeysWhereClause(string tableAlias = null)
        {
            return string.Join(" AND ", this.KeyProperties.Select(propInfo => $"{this.GetColumnName(propInfo, tableAlias, false)}=@{propInfo.PropertyName}"));
        }

        /// <summary>
        /// Constructs an enumeration of the key values.
        /// </summary>
        public virtual string ConstructKeyColumnEnumeration(string tableAlias = null)
        {
            return string.Join(",", this.KeyProperties.Select(propInfo => this.GetColumnName(propInfo, tableAlias, true)));
        }

        /// <summary>
        /// Constructs an enumeration of all the selectable columns (i.e. all the columns corresponding to entity properties which are not part of a relationship).
        /// (e.g. Id, HouseNo, AptNo)
        /// </summary>
        public virtual string ConstructColumnEnumerationForSelect(string tableAlias = null)
        {
            return string.Join(",", this.SelectProperties.Select(propInfo => this.GetColumnName(propInfo, tableAlias, true)));
        }

        /// <summary>
        /// Constructs an enumeration of all the columns available for insert.
        /// (e.g. HouseNo, AptNo)
        /// </summary>
        public virtual string ConstructColumnEnumerationForInsert()
        {
            return string.Join(",", this.InsertProperties.Select(propInfo => this.GetColumnName(propInfo, null, false)));
        }

        /// <summary>
        /// Constructs an enumeration of all the parameters denoting properties that are bound to columns available for insert.
        /// (e.g. @HouseNo, @AptNo)
        /// </summary>
        public virtual string ConstructParamEnumerationForInsert()
        {
            return string.Join(",", this.InsertProperties.Select(propInfo => $"@{propInfo.PropertyName}"));
        }

        /// <summary>
        /// Constructs a update clause of form <code>ColumnName=@PropertyName, ...</code> with all the updateable columns (e.g. <code>EmployeeId=@EmployeeId,DeskNo=@DeskNo</code>)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        public virtual string ConstructUpdateClause(string tableAlias = null)
        {
            return string.Join(",", UpdateProperties.Select(propInfo => $"{this.GetColumnName(propInfo, tableAlias, false)}=@{propInfo.PropertyName}"));
        }

        /// <summary>
        /// Constructs a full insert statement
        /// </summary>
        public abstract string ConstructFullInsertStatement();

        /// <summary>
        /// Constructs a full update statement
        /// </summary>
        public virtual string ConstructFullUpdateStatement()
        {
            return $"UPDATE {this.GetTableName()} SET {this.ConstructUpdateClause()} WHERE {this.ConstructKeysWhereClause()}".ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Constructs a full delete statement
        /// </summary>
        public virtual string ConstructFullDeleteStatement()
        {
            return $"DELETE FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}";
        }
        
        /// <summary>
        /// Constructs a full count statement
        /// </summary>
        public string ConstructFullCountStatement(FormattableString whereClause = null)
        {
            //{this.ConstructKeyColumnEnumeration()} might not have keys, besides no speed difference
            return (whereClause == null)
                       ? $"SELECT COUNT(*) FROM {this.GetTableName()}".ToString(CultureInfo.InvariantCulture)
                       : $"SELECT COUNT(*) FROM {this.GetTableName()} WHERE {whereClause}".ToString(this.StatementFormatter);
        }

        /// <summary>
        /// Returns a delimited SQL identifier.
        /// </summary>
        /// <param name="sqlIdentifier">Non-delimited SQL identifier</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetDelimitedIdentifier(string sqlIdentifier)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}{2}",
                this.IdentifierStartDelimiter,
                sqlIdentifier,
                this.IdentifierEndDelimiter);
        }

        /// <summary>
        /// Constructs a select statement for a single entity type
        /// </summary>
        public virtual string ConstructFullSingleSelectStatement()
        {
            return
                $"SELECT {this.ConstructColumnEnumerationForSelect()} FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}"
                    .ToString(CultureInfo.InvariantCulture);
        }

        public virtual EntityRelationship GetRelationship(IStatementSqlBuilder destination)
        {
            return _entityRelationships.GetOrAdd(
                destination,
                (destinationSqlBuilder) => new EntityRelationship(this, destinationSqlBuilder));
        }

        public virtual string ConstructMultiSelectStatement(IStatementSqlBuilder[] additionalIncludes)
        {
            var queryBuilder = new StringBuilder();
            //queryBuilder.Append($"SELECT {this.ConstructColumnEnumerationForSelect(this.GetTableName())}");
            //foreach (var additionalInclude in additionalIncludes)
            //{
            //    queryBuilder.Append($",{additionalInclude.ConstructColumnEnumerationForSelect(additionalInclude.GetTableName())}");
            //}
            //queryBuilder.Append($" FROM {this.GetTableName()}");

            //IStatementSqlBuilder leftSqlBuilder = this;

            //foreach (var rightSqlBuilder in additionalIncludes)
            //{
            //    // find the property linked to this entity
            //    var atLeastOneLinkProp = false;
            //    var joinLeftProps = joinLeftSqlBuilder.ForeignEntityProperties
            //        .Where(propInfo => propInfo.Descriptor.PropertyType == additionalInclude.EntityMapping.EntityType)
            //        .Select(propInfo => joinLeftSqlBuilder.EntityMapping.PropertyMappings[propInfo.PropertyName]);

            //    foreach (var joinLeftProp in joinLeftProps)
            //    {
            //        if (!atLeastOneLinkProp)
            //        {
            //            atLeastOneLinkProp = true;
            //            queryBuilder.Append($" LEFT OUTER JOIN {additionalInclude.GetTableName()} ON ");
            //        }
            //        else
            //        {
            //            queryBuilder.Append(" AND ");
            //        }
            //        queryBuilder.Append( $"{joinLeftSqlBuilder.GetColumnName(joinLeftProp, joinLeftSqlBuilder.GetTableName())}={additionalInclude.GetColumnName(joinLeftProp, additionalInclude.GetTableName())}");
            //    }

            //    if (!atLeastOneLinkProp)
            //    {
            //        throw new InvalidOperationException($"No foreign key constraint was found between the primary entity {joinLeftSqlBuilder.EntityMapping.EntityType} and the foreign entity {additionalInclude.EntityMapping.EntityType}");
            //    }
            //    joinLeftSqlBuilder = additionalInclude;
            //}

            return queryBuilder.ToString();
        }

        public abstract string ConstructFullBatchSelectStatement(
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null);

        protected string IdentifierStartDelimiter { get;}
        protected string IdentifierEndDelimiter { get;}
        protected bool UsesSchemaForTableNames { get; }

        public EntityDescriptor EntityDescriptor { get; }
        public EntityMapping EntityMapping { get; }
        public PropertyMapping[] ForeignEntityProperties { get; }
        public PropertyMapping[] SelectProperties { get; }
        public PropertyMapping[] KeyProperties { get; }
        public PropertyMapping[] InsertProperties { get; }
        public PropertyMapping[] UpdateProperties { get; }
        public PropertyMapping[] InsertKeyDatabaseGeneratedProperties { get; }
        public PropertyMapping[] InsertDatabaseGeneratedProperties { get; }
    }
}
