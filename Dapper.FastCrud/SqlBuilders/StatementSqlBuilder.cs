namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using Dapper.FastCrud.Configuration.StatementOptions;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;
    using System.Reflection;

    internal abstract class GenericStatementSqlBuilder:ISqlBuilder
    {
        //private static readonly RelationshipOrderComparer _relationshipOrderComparer = new RelationshipOrderComparer();

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

        // regular statement formatter to be used for parameter resolution.
        private readonly SqlStatementFormatter _regularStatementFormatter;

        // statement formatter that would treat the C identifier as TC
        private readonly SqlStatementFormatter _forcedTableResolutionStatementFormatter;


        protected GenericStatementSqlBuilder(
            EntityDescriptor entityDescriptor,
            EntityMapping entityMapping,
            SqlDialect dialect)
        {
            var databaseOptions = OrmConfiguration.Conventions.GetDatabaseOptions(dialect);
            this.UsesSchemaForTableNames = databaseOptions.IsUsingSchemas;
            this.IdentifierStartDelimiter = databaseOptions.StartDelimiter;
            this.IdentifierEndDelimiter = databaseOptions.EndDelimiter;
            this.ParameterPrefix = databaseOptions.ParameterPrefix;

            //_entityRelationships = new ConcurrentDictionary<IStatementSqlBuilder, EntityRelationship>();
            _regularStatementFormatter = new SqlStatementFormatter(entityDescriptor, entityMapping, this, false);
            _forcedTableResolutionStatementFormatter = new SqlStatementFormatter(entityDescriptor, entityMapping, this, true);

            this.EntityDescriptor = entityDescriptor;
            this.EntityMapping = entityMapping;

            this.SelectProperties = this.EntityMapping.PropertyMappings
                .Select(propMapping => propMapping.Value)
                .ToArray();
            this.KeyProperties = this.EntityMapping.PropertyMappings
                .Where(propMapping => propMapping.Value.IsPrimaryKey)
                .Select(propMapping => propMapping.Value)
                .OrderBy(propMapping => propMapping.ColumnOrder)
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
        public PropertyMapping[] SelectProperties { get; }
        //public Dictionary<Type, PropertyMapping[]> ParentChildRelationshipProperties { get; }
        //public Dictionary<Type, PropertyMapping[]> ChildParentRelationshipProperties { get; }
        public PropertyMapping[] KeyProperties { get; }
        public PropertyMapping[] InsertProperties { get; }
        public PropertyMapping[] UpdateProperties { get; }
        public PropertyMapping[] InsertKeyDatabaseGeneratedProperties { get; }
        public PropertyMapping[] RefreshOnInsertProperties { get; }
        public PropertyMapping[] RefreshOnUpdateProperties { get; }
        protected string IdentifierStartDelimiter { get; }
        protected string IdentifierEndDelimiter { get; }
        protected bool UsesSchemaForTableNames { get; }
        protected string ParameterPrefix { get; }

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
        /// If the column name differs from the name of the property, this method will normalize the name (e.g. will return 'tableAlias.colName AS propName')
        ///   so that the deserialization performed by Dapper would succeed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnNameForSelect(string propertyName, string tableAlias = null)
        {
            PropertyMapping targetPropertyMapping;
            if (!this.EntityMapping.PropertyMappings.TryGetValue(propertyName, out targetPropertyMapping))
            {
                throw new ArgumentException($"Property '{propertyName}' was not found on '{this.EntityMapping.EntityType}'");
            }

            return this.GetColumnName(targetPropertyMapping, tableAlias, true);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnName(string propertyName, string tableAlias = null)
        {
            PropertyMapping targetPropertyMapping;
            if (!this.EntityMapping.PropertyMappings.TryGetValue(propertyName, out targetPropertyMapping))
            {
                throw new ArgumentException($"Property '{propertyName}' was not found on '{this.EntityMapping.EntityType}'");
            }

            return this.GetColumnName(targetPropertyMapping, tableAlias, false);
        }

        /// <summary>
        /// Resolves a column name
        /// </summary>
        /// <param name="propMapping">Property mapping</param>
        /// <param name="tableAlias">Table alias</param>
        /// <param name="performColumnAliasNormalization">If true and the database column name differs from the property name, an AS clause will be added</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnName(PropertyMapping propMapping, string tableAlias, bool performColumnAliasNormalization)
        {
            var sqlTableAlias = tableAlias == null ? string.Empty : $"{this.GetDelimitedIdentifier(tableAlias)}.";
            var sqlColumnAlias = (performColumnAliasNormalization && propMapping.DatabaseColumnName != propMapping.PropertyName)
                                     ? $" AS {this.GetDelimitedIdentifier(propMapping.PropertyName)}"
                                     : string.Empty;
            return this.ResolveWithCultureInvariantFormatter($"{sqlTableAlias}{this.GetDelimitedIdentifier(propMapping.DatabaseColumnName)}{sqlColumnAlias}");
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
        /// Constructs the count part of the select statement.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructCountSelectClause()
        {
            //{this.ConstructKeyColumnEnumeration()} might not have keys, besides no speed difference
            return "COUNT(*)";
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
            return this.ConstructFullSelectStatementInternal(
                this.ConstructColumnEnumerationForSelect(),
                this.GetTableName(),
                whereClause,
                orderClause,
                skipRowsCount,
                limitRowsCount);
        }

        /// <summary>
        /// Produces a formatted string from a formattable string.
        /// Table and column names will be resolved, and identifier will be properly delimited.
        /// </summary>
        /// <param name="rawSql">The raw sql to format</param>
        /// <returns>Properly formatted SQL statement</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Format(FormattableString rawSql)
        {
            return this.ResolveWithSqlFormatter(rawSql);
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

        /// <summary>
        /// Constructs a select statement containing joined entities.
        /// </summary>
        public void ConstructFullJoinSelectStatement(
            out string fullStatement,
            out string splitOnExpression,
            IEnumerable<StatementSqlBuilderJoinInstruction> joinInstructions,
            string selectClause = null,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null)
        {
            Requires.NotNull(joinInstructions, nameof(joinInstructions));
            var allSqlJoinInstructions = new[] { new StatementSqlBuilderJoinInstruction(this, SqlJoinType.LeftOuterJoin,  whereClause, orderClause) }.Concat(joinInstructions).ToArray();
            Requires.Argument(allSqlJoinInstructions.Length > 1, nameof(joinInstructions), "Unable to create a full JOIN statement when no extra SQL builders were provided");

            var selectClauseBuilder = selectClause == null ? new StringBuilder() : null;
            var fromClauseBuilder = new StringBuilder();
            var splitOnExpressionBuilder = new StringBuilder();
            var additionalWhereClauseBuilder = new StringBuilder();
            var additionalOrderClauseBuilder = new StringBuilder();
            var joinClauseBuilder = new StringBuilder();

            // enumerate through the join instructions and construct FIRST ENTITY - SECOND ENTITY joins
            for(var secondEntityJoinInstructionIndex = 0; secondEntityJoinInstructionIndex < allSqlJoinInstructions.Length; secondEntityJoinInstructionIndex++)
            {
                var secondEntityJoinSqlInstruction = allSqlJoinInstructions[secondEntityJoinInstructionIndex];
                var secondEntitySqlBuilder = secondEntityJoinSqlInstruction.SqlBuilder;

                // prepare the aditional where clause
                var joinInstructionAdditionalWhereClause = secondEntityJoinSqlInstruction.WhereClause;
                if (joinInstructionAdditionalWhereClause != null)
                {
                    if (additionalWhereClauseBuilder.Length > 0)
                    {
                        additionalWhereClauseBuilder.Append(" AND ");
                    }

                    additionalWhereClauseBuilder.Append('(');
                    additionalWhereClauseBuilder.Append(secondEntitySqlBuilder.ResolveWithSqlFormatter(joinInstructionAdditionalWhereClause, forceTableColumnResolution: true));
                    additionalWhereClauseBuilder.Append(')');
                }

                // prepare the additional order clause
                var joinInstructionAdditionalOrderClause = secondEntityJoinSqlInstruction.OrderClause;
                if (joinInstructionAdditionalOrderClause != null)
                {
                    if (additionalOrderClauseBuilder.Length > 0)
                    {
                        additionalOrderClauseBuilder.Append(',');
                    }

                    additionalOrderClauseBuilder.Append(secondEntitySqlBuilder.ResolveWithSqlFormatter(joinInstructionAdditionalOrderClause, forceTableColumnResolution: true));
                }

                // add the select columns
                if (selectClauseBuilder != null)
                {
                    if (secondEntityJoinInstructionIndex > 0)
                    {
                        selectClauseBuilder.Append(',');
                    }

                    selectClauseBuilder.Append(secondEntitySqlBuilder.ConstructColumnEnumerationForSelect(secondEntitySqlBuilder.GetTableName()));
                }

                // add the split on expression
                if (secondEntityJoinInstructionIndex > 0)
                {
                    if (secondEntityJoinInstructionIndex > 1)
                    {
                        splitOnExpressionBuilder.Append(',');
                    }

                    splitOnExpressionBuilder.Append(secondEntitySqlBuilder.SelectProperties.First().PropertyName);
                }

                // build the join expression
                if (secondEntityJoinInstructionIndex == 0)
                {
                    fromClauseBuilder.Append(secondEntitySqlBuilder.GetTableName());
                }
                else
                {
                    // construct the join condition
                    joinClauseBuilder.Clear();
                    var secondEntityFinalJoinType = secondEntityJoinSqlInstruction.JoinType;

                    // discover and append all the join conditions for the current table
                    var atLeastOneRelationshipDiscovered = false;

                    for (var firstEntityJoinInstructionIndex = 0; firstEntityJoinInstructionIndex < secondEntityJoinInstructionIndex; firstEntityJoinInstructionIndex++)
                    {
                        var firstEntitySqlInstruction = allSqlJoinInstructions[firstEntityJoinInstructionIndex];
                        var firstEntitySqlBuilder = firstEntitySqlInstruction.SqlBuilder;

                        // discover the relationship, if any
                        PropertyMapping[] firstEntityPropertyMappings;
                        PropertyMapping[] secondEntityPropertyMappings;
                        this.FindRelationship(firstEntitySqlBuilder, secondEntitySqlBuilder, out firstEntityPropertyMappings, out secondEntityPropertyMappings, ref secondEntityFinalJoinType);

                        if (firstEntityPropertyMappings == null || secondEntityPropertyMappings == null)
                        {
                            // no relationship was found, move on
                            continue;
                        }

                        atLeastOneRelationshipDiscovered = true;

                        joinClauseBuilder.Append('(');
                        for (var firstEntityPropertyIndex = 0; firstEntityPropertyIndex<firstEntityPropertyMappings.Length; firstEntityPropertyIndex++)
                        {
                            if (firstEntityPropertyIndex > 0)
                            {
                                joinClauseBuilder.Append(" AND ");
                            }

                            var firstEntityProperty = firstEntityPropertyMappings[firstEntityPropertyIndex];
                            joinClauseBuilder.Append(firstEntitySqlBuilder.GetColumnName(firstEntityProperty, firstEntitySqlBuilder.GetTableName(), false));
                            joinClauseBuilder.Append('=');

                            // search for the corresponding column in the current entity
                            // we're doing this by index, since both sides had the relationship columns already ordered
                            var secondEntityProperty = secondEntityPropertyMappings[firstEntityPropertyIndex];
                            joinClauseBuilder.Append(secondEntitySqlBuilder.GetColumnName(secondEntityProperty, secondEntitySqlBuilder.GetTableName(), false));
                        }
                        joinClauseBuilder.Append(')');
                    }

                    if (!atLeastOneRelationshipDiscovered)
                    {
                        throw new InvalidOperationException($"Could not find any relationships involving '{secondEntitySqlBuilder.EntityMapping.EntityType}'");
                    }

                    // construct the final join condition for the entity
                    switch (secondEntityFinalJoinType)
                    {
                        case SqlJoinType.LeftOuterJoin:
                        case SqlJoinType.NotSpecified:
                            fromClauseBuilder.Append(" LEFT OUTER JOIN ");
                            break;
                        case SqlJoinType.InnerJoin:
                            fromClauseBuilder.Append(" JOIN ");
                            break;
                        default:
                            throw new NotSupportedException($"Join '{secondEntityFinalJoinType}' is not supported");
                    }

                    fromClauseBuilder.Append(secondEntitySqlBuilder.GetTableName());
                    fromClauseBuilder.Append(" ON ");
                    fromClauseBuilder.Append(joinClauseBuilder.ToString());
                }
            }

            splitOnExpression = splitOnExpressionBuilder.ToString();
            if (additionalWhereClauseBuilder.Length > 0)
            {
                whereClause = $"{additionalWhereClauseBuilder}";
            }
            else
            {
                whereClause = null;
            }

            if (additionalOrderClauseBuilder.Length > 0)
            {
                orderClause = $"{additionalOrderClauseBuilder}";
            }
            else
            {
                orderClause = null;
            }

            if (selectClauseBuilder != null)
            {
                selectClause = selectClauseBuilder.ToString();
            }
            fullStatement = this.ConstructFullSelectStatementInternal(selectClause, fromClauseBuilder.ToString(), whereClause, orderClause, skipRowsCount, limitRowsCount, true);
        }

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
            return string.Join(" AND ", this.KeyProperties.Select(propInfo => $"{this.GetColumnName(propInfo, tableAlias, false)}={this.ParameterPrefix + propInfo.PropertyName}"));
        }

        /// <summary>
        /// Constructs a column selection of all columns to be refreshed on update of the form <code>@PropertyName1,@PropertyName2...</code>
        /// </summary>
        /// <returns></returns>
        protected virtual string ConstructRefreshOnUpdateColumnSelection()
        {
            return string.Join(",", this.RefreshOnUpdateProperties.Select(propInfo => this.GetColumnName(propInfo, null, true)));
        }

        /// <summary>
        /// Constructs a column selection of all columns to be refreshed on insert of the form <code>@PropertyName1,@PropertyName2...</code>
        /// </summary>
        /// <returns></returns>
        protected virtual string ConstructRefreshOnInsertColumnSelection()
        {
            return string.Join(",", this.RefreshOnInsertProperties.Select(propInfo => this.GetColumnName(propInfo, null, true)));
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
            return string.Join(",", this.InsertProperties.Select(propInfo => $"{this.ParameterPrefix + propInfo.PropertyName}"));
        }

        /// <summary>
        /// Constructs a update clause of form <code>ColumnName=@PropertyName, ...</code> with all the updateable columns (e.g. <code>EmployeeId=@EmployeeId,DeskNo=@DeskNo</code>)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        protected virtual string ConstructUpdateClauseInternal(string tableAlias = null)
        {
            return string.Join(",", UpdateProperties.Select(propInfo => $"{this.GetColumnName(propInfo, tableAlias, false)}={this.ParameterPrefix + propInfo.PropertyName}"));
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
                sql += this.ResolveWithCultureInvariantFormatter($";SELECT {this.ConstructRefreshOnUpdateColumnSelection()} FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()};");
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
            return (whereClause == null)
                       ? this.ResolveWithCultureInvariantFormatter($"SELECT {this.ConstructCountSelectClause()} FROM {this.GetTableName()}")
                       : this.ResolveWithSqlFormatter($"SELECT {this.ConstructCountSelectClause()} FROM {this.GetTableName()} WHERE {whereClause}");
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

        protected abstract string ConstructFullSelectStatementInternal(
            string selectClause,
            string fromClause,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null,
            bool forceTableColumnResolution = false);

        /// <summary>
        /// Resolves a formattable string using the invariant culture, ignoring any special identifiers.
        /// </summary>
        /// <param name="formattableString">Raw formattable string</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string ResolveWithCultureInvariantFormatter(FormattableString formattableString)
        {
            return formattableString.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Resolves a formattable string using the SQL formatter
        /// </summary>
        /// <param name="formattableString">Raw formattable string</param>
        /// <param name="forceTableColumnResolution">If true, the table is always going to be used as alias for column identifiers</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string ResolveWithSqlFormatter(FormattableString formattableString, bool forceTableColumnResolution = false)
        {
            return formattableString.ToString(forceTableColumnResolution ? _forcedTableResolutionStatementFormatter : _regularStatementFormatter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FindRelationship(
            GenericStatementSqlBuilder firstEntitySqlBuilder,
            GenericStatementSqlBuilder secondEntitySqlBuilder, 
            out PropertyMapping[] firstEntityRelationshipPropertyMappings, 
            out PropertyMapping[] secondEntityRelationshipPropertyMappings, 
            ref SqlJoinType secondEntityJoinType)
        {
            var firstEntityMapping = firstEntitySqlBuilder.EntityMapping;
            var secondEntityMapping = secondEntitySqlBuilder.EntityMapping;

            EntityMappingRelationship firstToSecondEntityMappingRelationship;
            EntityMappingRelationship secondToFirstEntityMappingRelationship;

            // two flags in order to cover 1-to-1 relationships
            bool firstToSecondParentChildRelationship;
            bool secondToFirstParentChildRelationship;

            this.FindRelationship(firstEntityMapping, secondEntityMapping, out firstToSecondEntityMappingRelationship, out firstToSecondParentChildRelationship);
            this.FindRelationship(secondEntityMapping, firstEntityMapping, out secondToFirstEntityMappingRelationship, out secondToFirstParentChildRelationship);

            if (firstToSecondEntityMappingRelationship == null && secondToFirstEntityMappingRelationship == null)
            {
                // no relationship was found on either side
                firstEntityRelationshipPropertyMappings = null;
                secondEntityRelationshipPropertyMappings = null;
                return;
            }

            firstEntityRelationshipPropertyMappings = firstToSecondEntityMappingRelationship?.ReferencingKeyProperties;
            secondEntityRelationshipPropertyMappings = secondToFirstEntityMappingRelationship?.ReferencingKeyProperties;

            // fix the lack of relationship info on one side, this is an acceptable scenario on parent entities only
            if (firstToSecondEntityMappingRelationship == null)
            {
                if (secondToFirstParentChildRelationship)
                {
                    // first * - 1 second
                    throw new InvalidOperationException($"Expected to find foreign keys on the '{firstEntityMapping.EntityType}' entity for the '{secondEntityMapping.EntityType}' entity");
                }
                else
                {
                    // first 1 - * second
                    firstEntityRelationshipPropertyMappings = firstEntitySqlBuilder.KeyProperties;
                    firstToSecondParentChildRelationship = true;
                }
            }
            else if (secondToFirstEntityMappingRelationship == null)
            {
                if (firstToSecondParentChildRelationship)
                {
                    // second * - 1 first
                    throw new InvalidOperationException($"Expected to find foreign keys on the '{secondEntityMapping.EntityType}' entity for the '{firstEntityMapping.EntityType}' entity");
                }
                else
                {
                    // second 1 - * second
                    secondEntityRelationshipPropertyMappings = secondEntitySqlBuilder.KeyProperties;
                    secondToFirstParentChildRelationship = true;
                }
            }

            if (firstEntityRelationshipPropertyMappings.Length != secondEntityRelationshipPropertyMappings.Length)
            {
                throw new InvalidOperationException($"Mismatch in the number of properties that are part of the relationship between '{firstEntityMapping.EntityType}' ({firstEntityRelationshipPropertyMappings.Length} properties) and '{secondEntityMapping.EntityType}' ({secondEntityRelationshipPropertyMappings.Length} properties)");
            }

            // in case the second entity is a child, one of its foreign key properties is not nullable and the join type wasn't specified, default to INNER JOIN
            if (secondEntityJoinType == SqlJoinType.NotSpecified && secondToFirstParentChildRelationship == false && secondEntityRelationshipPropertyMappings.Any(propMapping =>
            {
                var propType = propMapping.Descriptor.PropertyType;
                return
#if NETSTANDARD
                                                                                                                                                    propType.GetTypeInfo().IsValueType
#else
                                                                                                                                                    propType.IsValueType
#endif
                                                                                                                                                    && Nullable.GetUnderlyingType(propMapping.Descriptor.PropertyType) == null;
            }))
            {
                secondEntityJoinType = SqlJoinType.InnerJoin;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FindRelationship(EntityMapping targetEntityMapping, EntityMapping entityMappingToDiscover, out EntityMappingRelationship targetRelationship, out bool targetParentChildRelationship)
        {
            if (targetEntityMapping.ChildParentRelationships.TryGetValue(entityMappingToDiscover.EntityType, out targetRelationship))
            {
                targetParentChildRelationship = false;
            }
            else if (targetEntityMapping.ParentChildRelationships.TryGetValue(entityMappingToDiscover.EntityType, out targetRelationship))
            {
                targetParentChildRelationship = true;
            }
            else
            {
                targetParentChildRelationship = false;
                targetRelationship = null;
            }
        }
    }
}
