namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;

    internal abstract class GenericStatementSqlBuilder:IStatementSqlBuilder
    {
        //private readonly ConcurrentDictionary<IStatementSqlBuilder, EntityRelationship> _entityRelationships;
        private readonly Lazy<string> _noAliasKeysWhereClause;
        private readonly Lazy<string> _noAliasTableName;
        private readonly Lazy<string> _noAliasKeyColumnEnumeration;
        private readonly Lazy<string> _noAliasColumnEnumerationForSelect;
        private readonly Lazy<string> _columnEnumerationForInsert;
        private readonly Lazy<string> _paramEnumerationForInsert;
        private readonly Lazy<string> _noAliasUpdateClause;
        private readonly Lazy<string> _fullInsertStatement;
        private readonly Lazy<string> _fullSingleUpdateStatement;
        private readonly Lazy<string> _noConditionFullBatchUpdateStatement;
        private readonly Lazy<string> _fullSingleDeleteStatement;
        private readonly Lazy<string> _noConditionFullBatchDeleteStatement;
        private readonly Lazy<string> _noConditionFullCountStatement;
        private readonly Lazy<string> _fullSingleSelectStatement; 

        protected GenericStatementSqlBuilder(
            EntityDescriptor entityDescriptor,
            EntityMapping entityMapping,
            SqlDialect dialect)
        {
            var databaseOptions = OrmConfiguration.Conventions.GetDatabaseOptions(dialect);
            this.UsesSchemaForTableNames = databaseOptions.IsUsingSchemas;
            this.IdentifierStartDelimiter = databaseOptions.StartDelimiter;
            this.IdentifierEndDelimiter = databaseOptions.EndDelimiter;


            //_entityRelationships = new ConcurrentDictionary<IStatementSqlBuilder, EntityRelationship>();
            this.StatementFormatter = new SqlStatementFormatter(entityDescriptor,entityMapping,this);
            this.EntityDescriptor = entityDescriptor;
            this.EntityMapping = entityMapping;

            this.SelectProperties = this.EntityMapping.PropertyMappings
                .Where(propMapping => !propMapping.Value.IsReferencingForeignEntity)
                .Select(propMapping => propMapping.Value)
                .ToArray();
            this.KeyProperties = this.EntityMapping.PropertyMappings
                .Where(propMapping => propMapping.Value.IsPrimaryKey)
                .Select(propMapping => propMapping.Value)
                .ToArray();
            this.RefreshOnInsertProperties = this.SelectProperties
                .Where(propInfo => propInfo.IsRefreshedOnInserts)
                .ToArray();
            this.RefreshOnUpdateProperties = this.SelectProperties
                .Where(propInfo => propInfo.IsRefreshedOnUpdates)
                .ToArray();
            this.InsertKeyDatabaseGeneratedProperties = this.KeyProperties
                .Intersect(this.RefreshOnInsertProperties)
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

            _noAliasTableName = new Lazy<string>(()=>this.GetTableNameInternal(),LazyThreadSafetyMode.PublicationOnly);
            _noAliasKeysWhereClause = new Lazy<string>(()=>this.ConstructKeysWhereClauseInternal(), LazyThreadSafetyMode.PublicationOnly);
            _noAliasKeyColumnEnumeration = new Lazy<string>(() => this.ConstructKeyColumnEnumerationInternal(), LazyThreadSafetyMode.PublicationOnly);
            _noAliasColumnEnumerationForSelect = new Lazy<string>(() => this.ConstructColumnEnumerationForSelectInternal(), LazyThreadSafetyMode.PublicationOnly);
            _columnEnumerationForInsert = new Lazy<string>(()=>this.ConstructColumnEnumerationForInsertInternal(), LazyThreadSafetyMode.PublicationOnly);
            _paramEnumerationForInsert = new Lazy<string>(()=>this.ConstructParamEnumerationForInsertInternal(),LazyThreadSafetyMode.PublicationOnly);
            _noAliasUpdateClause = new Lazy<string>(()=>this.ConstructUpdateClauseInternal(),LazyThreadSafetyMode.PublicationOnly);
            _fullInsertStatement = new Lazy<string>(()=>this.ConstructFullInsertStatementInternal(),LazyThreadSafetyMode.PublicationOnly);
            _fullSingleUpdateStatement = new Lazy<string>(()=>this.ConstructFullSingleUpdateStatementInternal(),LazyThreadSafetyMode.PublicationOnly);
            _noConditionFullBatchUpdateStatement = new Lazy<string>(()=>this.ConstructFullBatchUpdateStatementInternal(),LazyThreadSafetyMode.PublicationOnly);
            _fullSingleDeleteStatement = new Lazy<string>(()=>this.ConstructFullSingleDeleteStatementInternal(),LazyThreadSafetyMode.PublicationOnly);
            _noConditionFullBatchDeleteStatement = new Lazy<string>(()=>this.ConstructFullBatchDeleteStatementInternal(),LazyThreadSafetyMode.PublicationOnly);
            _noConditionFullCountStatement = new Lazy<string>(()=>this.ConstructFullCountStatementInternal(),LazyThreadSafetyMode.PublicationOnly);
            _fullSingleSelectStatement = new Lazy<string>(()=>this.ConstructFullSingleSelectStatementInternal(),LazyThreadSafetyMode.PublicationOnly);
        }

        public EntityDescriptor EntityDescriptor { get; }
        public EntityMapping EntityMapping { get; }
        public PropertyMapping[] ForeignEntityProperties { get; }
        public PropertyMapping[] SelectProperties { get; }
        public PropertyMapping[] KeyProperties { get; }
        public PropertyMapping[] InsertProperties { get; }
        public PropertyMapping[] UpdateProperties { get; }
        public PropertyMapping[] InsertKeyDatabaseGeneratedProperties { get; }
        public PropertyMapping[] RefreshOnInsertProperties { get; }
        public PropertyMapping[] RefreshOnUpdateProperties { get; }
        protected string IdentifierStartDelimiter { get; }
        protected string IdentifierEndDelimiter { get; }
        protected bool UsesSchemaForTableNames { get; }

        /// <summary>
        /// Gets the statement formatter to be used for parameter resolution.
        /// </summary>
        protected SqlStatementFormatter StatementFormatter { get; }

        /// <summary>
        /// Produces a formatted string from a formattable string.
        /// Table and column names will be resolved, and identifier will be properly delimited.
        /// </summary>
        /// <param name="rawSql">The raw sql to format</param>
        /// <returns>Properly formatted SQL statement</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Format(FormattableString rawSql)
        {
            return rawSql.ToString(this.StatementFormatter);
        }

        /// <summary>
        /// Returns the table name associated with the current entity.
        /// </summary>
        /// <param name="tableAlias">Optional table alias using AS.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetTableName(string tableAlias = null)
        {
            return tableAlias == null ? _noAliasTableName.Value : this.GetTableNameInternal(tableAlias);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property, string tableAlias = null)
        {
            var propName = ((MemberExpression)property.Body).Member.Name;
            return this.GetColumnName(propName, tableAlias);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnName(string propertyName, string tableAlias = null)
        {
            return this.GetColumnName(this.EntityMapping.PropertyMappings[propertyName], tableAlias, false);
        }

        /// <summary>
        /// Resolves a column name
        /// </summary>
        /// <param name="propMapping">Property mapping</param>
        /// <param name="tableAlias">Table alias</param>
        /// <param name="performColumnAliasNormalization"></param>
        /// <returns>If true and the database column name differs from the property name, an AS clause will be added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnName(PropertyMapping propMapping, string tableAlias, bool performColumnAliasNormalization)
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
        /// <param name="tableAlias">Optional table alias.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructKeysWhereClause(string tableAlias = null)
        {
            return tableAlias == null ? _noAliasKeysWhereClause.Value : this.ConstructKeysWhereClauseInternal(tableAlias);
        }

        /// <summary>
        /// Constructs an enumeration of the key values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructKeyColumnEnumeration(string tableAlias = null)
        {
            return tableAlias == null ? _noAliasKeyColumnEnumeration.Value : this.ConstructKeyColumnEnumerationInternal(tableAlias);
        }

        /// <summary>
        /// Constructs an enumeration of all the selectable columns (i.e. all the columns corresponding to entity properties which are not part of a relationship).
        /// (e.g. Id, HouseNo, AptNo)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructColumnEnumerationForSelect(string tableAlias = null)
        {
            return tableAlias == null ? _noAliasColumnEnumerationForSelect.Value : this.ConstructColumnEnumerationForSelectInternal(tableAlias);
        }

        /// <summary>
        /// Constructs an enumeration of all the columns available for insert.
        /// (e.g. HouseNo, AptNo)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructColumnEnumerationForInsert()
        {
            return _columnEnumerationForInsert.Value;
        }

        /// <summary>
        /// Constructs an enumeration of all the parameters denoting properties that are bound to columns available for insert.
        /// (e.g. @HouseNo, @AptNo)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructParamEnumerationForInsert()
        {
            return _paramEnumerationForInsert.Value;
        }

        /// <summary>
        /// Constructs a update clause of form <code>ColumnName=@PropertyName, ...</code> with all the updateable columns (e.g. <code>EmployeeId=@EmployeeId,DeskNo=@DeskNo</code>)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructUpdateClause(string tableAlias = null)
        {
            return tableAlias == null ? _noAliasUpdateClause.Value : this.ConstructUpdateClauseInternal(tableAlias);
        }

        /// <summary>
        /// Constructs an insert statement for a single entity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructFullInsertStatement()
        {
            return _fullInsertStatement.Value;
        }

        /// <summary>
        /// Constructs an update statement for a single entity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructFullSingleUpdateStatement()
        {
            return _fullSingleUpdateStatement.Value;
        }

        /// <summary>
        /// Constructs a batch select statement.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructFullBatchUpdateStatement(FormattableString whereClause = null)
        {
            return whereClause == null ? _noConditionFullBatchUpdateStatement.Value : this.ConstructFullBatchUpdateStatementInternal(whereClause);
        }

        /// <summary>
        /// Constructs a delete statement for a single entity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructFullSingleDeleteStatement()
        {
            return _fullSingleDeleteStatement.Value;
        }

        /// <summary>
        /// Constructs a batch delete statement.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructFullBatchDeleteStatement(FormattableString whereClause = null)
        {
            return whereClause == null ? _noConditionFullBatchDeleteStatement.Value : this.ConstructFullBatchDeleteStatementInternal(whereClause);
        }

        /// <summary>
        /// Constructs a full count statement, optionally with a where clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructFullCountStatement(FormattableString whereClause = null)
        {
            return whereClause == null ? _noConditionFullCountStatement.Value : this.ConstructFullCountStatementInternal(whereClause);
        }

        /// <summary>
        /// Constructs a select statement for a single entity
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructFullSingleSelectStatement()
        {
            return _fullSingleSelectStatement.Value;
        }

        /// <summary>
        /// Constructs a batch select statement
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructFullBatchSelectStatement(
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null,
            object queryParameters = null)
        {
            return this.ConstructFullBatchSelectStatementInternal(whereClause, orderClause, skipRowsCount, limitRowsCount, queryParameters);
        }

        /// <summary>
        /// Returns a delimited SQL identifier.
        /// </summary>
        /// <param name="sqlIdentifier">Delimited or non-delimited SQL identifier</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetDelimitedIdentifier(string sqlIdentifier)
        {
            Requires.NotNullOrEmpty(sqlIdentifier, nameof(sqlIdentifier));

            var startsWithIdentifier = sqlIdentifier.StartsWith(this.IdentifierStartDelimiter);
            var endsWithIdentifier = sqlIdentifier.EndsWith(this.IdentifierEndDelimiter);

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}{2}",
                startsWithIdentifier ? string.Empty : this.IdentifierStartDelimiter,
                sqlIdentifier,
                endsWithIdentifier ? string.Empty : this.IdentifierEndDelimiter);
        }

        //public EntityRelationship GetRelationship(IStatementSqlBuilder destination)
        //{
        //    return _entityRelationships.GetOrAdd(
        //        destination,
        //        destinationSqlBuilder => new EntityRelationship(this, destinationSqlBuilder));
        //}

        //public string ConstructMultiSelectStatement(IStatementSqlBuilder[] additionalIncludes)
        //{
        //    var queryBuilder = new StringBuilder();
        //    //queryBuilder.Append($"SELECT {this.ConstructColumnEnumerationForSelect(this.GetTableName())}");
        //    //foreach (var additionalInclude in additionalIncludes)
        //    //{
        //    //    queryBuilder.Append($",{additionalInclude.ConstructColumnEnumerationForSelect(additionalInclude.GetTableName())}");
        //    //}
        //    //queryBuilder.Append($" FROM {this.GetTableName()}");

        //    //IStatementSqlBuilder leftSqlBuilder = this;

        //    //foreach (var rightSqlBuilder in additionalIncludes)
        //    //{
        //    //    // find the property linked to this entity
        //    //    var atLeastOneLinkProp = false;
        //    //    var joinLeftProps = joinLeftSqlBuilder.ForeignEntityProperties
        //    //        .Where(propInfo => propInfo.Descriptor.PropertyType == additionalInclude.EntityMapping.EntityType)
        //    //        .Select(propInfo => joinLeftSqlBuilder.EntityMapping.PropertyMappings[propInfo.PropertyName]);

        //    //    foreach (var joinLeftProp in joinLeftProps)
        //    //    {
        //    //        if (!atLeastOneLinkProp)
        //    //        {
        //    //            atLeastOneLinkProp = true;
        //    //            queryBuilder.Append($" LEFT OUTER JOIN {additionalInclude.GetTableName()} ON ");
        //    //        }
        //    //        else
        //    //        {
        //    //            queryBuilder.Append(" AND ");
        //    //        }
        //    //        queryBuilder.Append( $"{joinLeftSqlBuilder.GetColumnName(joinLeftProp, joinLeftSqlBuilder.GetTableName())}={additionalInclude.GetColumnName(joinLeftProp, additionalInclude.GetTableName())}");
        //    //    }

        //    //    if (!atLeastOneLinkProp)
        //    //    {
        //    //        throw new InvalidOperationException($"No foreign key constraint was found between the primary entity {joinLeftSqlBuilder.EntityMapping.EntityType} and the foreign entity {additionalInclude.EntityMapping.EntityType}");
        //    //    }
        //    //    joinLeftSqlBuilder = additionalInclude;
        //    //}

        //    return queryBuilder.ToString();
        //}

        /// <summary>
        /// Returns the table name associated with the current entity.
        /// </summary>
        protected virtual string GetTableNameInternal(string tableAlias = null)
        {
            var sqlAlias = tableAlias == null
                ? string.Empty
                : $" AS {this.GetDelimitedIdentifier(tableAlias)}";

            FormattableString fullTableName;
            if ((!this.UsesSchemaForTableNames) || string.IsNullOrEmpty(this.EntityMapping.SchemaName))
            {
                fullTableName = $"{this.GetDelimitedIdentifier(this.EntityMapping.TableName)}";
            }                                
            else
            {
                fullTableName = $"{this.GetDelimitedIdentifier(this.EntityMapping.SchemaName)}.{this.GetDelimitedIdentifier(this.EntityMapping.TableName)}";
            }

            return this.ResolveWithCultureInvariantFormatter($"{fullTableName}{sqlAlias}");
        }

        /// <summary>
        /// Constructs a condition of form <code>ColumnName=@PropertyName and ...</code> with all the key columns (e.g. <code>Id=@Id and EmployeeId=@EmployeeId</code>)
        /// </summary>
        protected virtual string ConstructKeysWhereClauseInternal(string tableAlias = null)
        {
            return string.Join(" AND ", this.KeyProperties.Select(propInfo => $"{this.GetColumnName(propInfo, tableAlias, false)}=@{propInfo.PropertyName}"));
        }

        /// <summary>
        /// Constructs an enumeration of the key values.
        /// </summary>
        protected virtual string ConstructKeyColumnEnumerationInternal(string tableAlias = null)
        {
            return string.Join(",", this.KeyProperties.Select(propInfo => this.GetColumnName(propInfo, tableAlias, true)));
        }

        /// <summary>
        /// Constructs an enumeration of all the selectable columns (i.e. all the columns corresponding to entity properties which are not part of a relationship).
        /// (e.g. Id, HouseNo, AptNo)
        /// </summary>
        protected virtual string ConstructColumnEnumerationForSelectInternal(string tableAlias = null)
        {
            return string.Join(",", this.SelectProperties.Select(propInfo => this.GetColumnName(propInfo, tableAlias, true)));
        }

        /// <summary>
        /// Constructs an enumeration of all the columns available for insert.
        /// (e.g. HouseNo, AptNo)
        /// </summary>
        protected virtual string ConstructColumnEnumerationForInsertInternal()
        {
            return string.Join(",", this.InsertProperties.Select(propInfo => this.GetColumnName(propInfo, null, false)));
        }

        /// <summary>
        /// Constructs an enumeration of all the parameters denoting properties that are bound to columns available for insert.
        /// (e.g. @HouseNo, @AptNo)
        /// </summary>
        protected virtual string ConstructParamEnumerationForInsertInternal()
        {
            return string.Join(",", this.InsertProperties.Select(propInfo => $"@{propInfo.PropertyName}"));
        }

        /// <summary>
        /// Constructs a update clause of form <code>ColumnName=@PropertyName, ...</code> with all the updateable columns (e.g. <code>EmployeeId=@EmployeeId,DeskNo=@DeskNo</code>)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        protected virtual string ConstructUpdateClauseInternal(string tableAlias = null)
        {
            return string.Join(",", UpdateProperties.Select(propInfo => $"{this.GetColumnName(propInfo, tableAlias, false)}=@{propInfo.PropertyName}"));
        }

        /// <summary>
        /// Constructs a full insert statement
        /// </summary>
        protected abstract string ConstructFullInsertStatementInternal();

        /// <summary>
        /// Constructs an update statement for a single entity.
        /// </summary>
        protected virtual string ConstructFullSingleUpdateStatementInternal()
        {
            if (this.KeyProperties.Length == 0)
            {
                throw new NotSupportedException($"Entity '{this.EntityMapping.EntityType.Name}' has no primary key. UPDATE is not possible.");
            }

            var sql = this.ResolveWithCultureInvariantFormatter(
                    $"UPDATE {this.GetTableName()} SET {this.ConstructUpdateClause()} WHERE {this.ConstructKeysWhereClause()}");
            if (this.RefreshOnUpdateProperties.Length > 0)
            {
                var databaseGeneratedColumnSelection = string.Join(
                    ",",
                    this.RefreshOnUpdateProperties.Select(
                        propInfo => this.GetColumnName(propInfo, null, true)));
                sql += this.ResolveWithCultureInvariantFormatter($";SELECT {databaseGeneratedColumnSelection} FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()};");
            }

            return sql;
        }

        /// <summary>
        /// Constructs a batch select statement.
        /// </summary>
        protected virtual string ConstructFullBatchUpdateStatementInternal(FormattableString whereClause = null)
        {
            FormattableString updateStatement = $"UPDATE {this.GetTableName()} SET {this.ConstructUpdateClause()}";
            if (whereClause != null)
            {
                return this.ResolveWithSqlFormatter($"{updateStatement} WHERE {whereClause}");
            }

            return this.ResolveWithCultureInvariantFormatter(updateStatement);
        }

        /// <summary>
        /// Constructs a delete statement for a single entity.
        /// </summary>
        protected virtual string ConstructFullSingleDeleteStatementInternal()
        {
            if (this.KeyProperties.Length == 0)
            {
                throw new NotSupportedException($"Entity '{this.EntityMapping.EntityType.Name}' has no primary key. DELETE is not possible.");
            }

            return this.ResolveWithCultureInvariantFormatter(
                $"DELETE FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}");
        }

        /// <summary>
        /// Constructs a batch delete statement.
        /// </summary>
        protected virtual string ConstructFullBatchDeleteStatementInternal(FormattableString whereClause = null)
        {
            FormattableString deleteStatement = $"DELETE FROM {this.GetTableName()}";
            if (whereClause != null)
            {
                return this.ResolveWithSqlFormatter($"{deleteStatement} WHERE {whereClause}");
            }

            return this.ResolveWithCultureInvariantFormatter(deleteStatement);
        }

        /// <summary>
        /// Constructs a full count statement, optionally with a where clause.
        /// </summary>
        protected virtual string ConstructFullCountStatementInternal(FormattableString whereClause = null)
        {
            //{this.ConstructKeyColumnEnumeration()} might not have keys, besides no speed difference
            return (whereClause == null)
                       ? this.ResolveWithCultureInvariantFormatter($"SELECT COUNT(*) FROM {this.GetTableName()}")
                       : this.ResolveWithSqlFormatter($"SELECT COUNT(*) FROM {this.GetTableName()} WHERE {whereClause}");
        }

        /// <summary>
        /// Constructs a select statement for a single entity
        /// </summary>
        protected virtual string ConstructFullSingleSelectStatementInternal()
        {
            if (this.KeyProperties.Length == 0)
            {
                throw new NotSupportedException($"Entity '{this.EntityMapping.EntityType.Name}' has no primary key. SELECT is not possible.");
            }

            return this.ResolveWithCultureInvariantFormatter(
                    $"SELECT {this.ConstructColumnEnumerationForSelect()} FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}");
        }

        /// <summary>
        /// Constructs a full batch select statement
        /// </summary>
        protected abstract string ConstructFullBatchSelectStatementInternal(
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null,
            object queryParameters = null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string ResolveWithCultureInvariantFormatter(FormattableString formattableString)
        {
            return formattableString.ToString(CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string ResolveWithSqlFormatter(FormattableString formattableString)
        {
            return formattableString.ToString(this.StatementFormatter);
        }
    }
}
