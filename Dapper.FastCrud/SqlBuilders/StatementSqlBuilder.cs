namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using Dapper.FastCrud.Configuration.StatementOptions;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;

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
            //this.ParentChildRelationshipProperties = this.EntityMapping.PropertyMappings
            //    .Where(propMapping => propMapping.Value.ParentChildRelationship != null)
            //    .Select(propMapping => propMapping.Value)
            //    .GroupBy(propInfo => propInfo.ParentChildRelationship.ReferencedEntityType)
            //    .ToDictionary(
            //        groupedTypePropInfo => groupedTypePropInfo.Key, 
            //        groupedTypePropInfo => groupedTypePropInfo.OrderBy(propInfo => propInfo.ParentChildRelationship.Order, 
            //        _relationshipOrderComparer).ToArray());
            //this.ChildParentRelationshipProperties = this.EntityMapping.PropertyMappings
            //    .Where(propMapping => propMapping.Value.ChildParentRelationship != null)
            //    .Select(propMapping => propMapping.Value)
            //    .GroupBy(propInfo => propInfo.ChildParentRelationship.ReferencedEntityType)
            //    .ToDictionary(
            //        groupedTypePropInfo => groupedTypePropInfo.Key,
            //        groupedTypePropInfo => groupedTypePropInfo.OrderBy(propInfo => propInfo.ChildParentRelationship.Order,
            //        _relationshipOrderComparer).ToArray());

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

            for(var leftJoinInstructionIndex = 0; leftJoinInstructionIndex < allSqlJoinInstructions.Length; leftJoinInstructionIndex++)
            {
                var leftJoinSqlInstruction = allSqlJoinInstructions[leftJoinInstructionIndex];
                var leftJoinSqlBuilder = leftJoinSqlInstruction.SqlBuilder;

                // prepare the aditional where clause
                var leftJoinAdditionalWhereClause = leftJoinSqlInstruction.WhereClause;
                if (leftJoinAdditionalWhereClause != null)
                {
                    if (additionalWhereClauseBuilder.Length > 0)
                    {
                        additionalWhereClauseBuilder.Append(" AND ");
                    }

                    additionalWhereClauseBuilder.Append('(');
                    additionalWhereClauseBuilder.Append(leftJoinSqlBuilder.ResolveWithSqlFormatter(leftJoinAdditionalWhereClause, forceTableColumnResolution: true));
                    additionalWhereClauseBuilder.Append(')');
                }

                // prepare the additional order clause
                var leftJoinAdditionalOrderClause = leftJoinSqlInstruction.OrderClause;
                if (leftJoinAdditionalOrderClause != null)
                {
                    if (additionalOrderClauseBuilder.Length > 0)
                    {
                        additionalOrderClauseBuilder.Append(',');
                    }

                    additionalOrderClauseBuilder.Append(leftJoinSqlBuilder.ResolveWithSqlFormatter(leftJoinAdditionalOrderClause, forceTableColumnResolution: true));
                }

                // add the select columns
                if (selectClauseBuilder != null)
                {
                    if (leftJoinInstructionIndex > 0)
                    {
                        selectClauseBuilder.Append(',');
                    }

                    selectClauseBuilder.Append(leftJoinSqlBuilder.ConstructColumnEnumerationForSelect(leftJoinSqlBuilder.GetTableName()));
                }

                // add the split on expression
                if (leftJoinInstructionIndex > 0)
                {
                    if (leftJoinInstructionIndex > 1)
                    {
                        splitOnExpressionBuilder.Append(',');
                    }

                    splitOnExpressionBuilder.Append(leftJoinSqlBuilder.SelectProperties.First().PropertyName);
                }

                // build the join expression
                if (leftJoinInstructionIndex == 0)
                {
                    fromClauseBuilder.Append(leftJoinSqlBuilder.GetTableName());
                }
                else
                {
                    // construct the join condition
                    joinClauseBuilder.Clear();
                    var leftEntityFinalJoinType = leftJoinSqlInstruction.JoinType;

                    // discover and append all the join conditions for the current table
                    var atLeastOneRelationshipDiscovered = false;

                    for (var rightJoinSqlBuilderIndex = 0; rightJoinSqlBuilderIndex < leftJoinInstructionIndex; rightJoinSqlBuilderIndex++)
                    {
                        var rightJoinSqlInstruction = allSqlJoinInstructions[rightJoinSqlBuilderIndex];
                        var rightJoinSqlBuilder = rightJoinSqlInstruction.SqlBuilder;

                        // get the columns involved in the relationship on the left entity - current SQL builder
                        EntityMappingRelationship leftJoinEntityRelationship;
                        bool leftJoinParentChildRelationship;
                        if (leftJoinSqlBuilder.EntityMapping.ChildParentRelationships.TryGetValue(rightJoinSqlBuilder.EntityMapping.EntityType, out leftJoinEntityRelationship))
                        {
                            leftJoinParentChildRelationship = false;
                            atLeastOneRelationshipDiscovered = true;

                            // in case the left entity is a child, one of its foreign key properties is not nullable and the join type wasn't specified, default to INNER JOIN
                            if(leftEntityFinalJoinType==SqlJoinType.NotSpecified && leftJoinEntityRelationship.ReferencingKeyProperties.Any(propMapping =>
                                                                                                                                            {
                                                                                                                                                var propType = propMapping.Descriptor.PropertyType;
                                                                                                                                                return
#if COREFX
                                                                                                                                                    propType.GetTypeInfo().IsValueType
#else
                                                                                                                                                    propType.IsValueType
#endif
                                                                                                                                                    && Nullable.GetUnderlyingType(propMapping.Descriptor.PropertyType) == null;
                                                                                                                                            }))
                            {
                                leftEntityFinalJoinType = SqlJoinType.InnerJoin;
                            }

                        }
                        else if (leftJoinSqlBuilder.EntityMapping.ParentChildRelationships.TryGetValue(rightJoinSqlBuilder.EntityMapping.EntityType, out leftJoinEntityRelationship))
                        {
                            leftJoinParentChildRelationship = true;
                            atLeastOneRelationshipDiscovered = true;

                            // in case the left entity is a parent and the join type wasn't specified, default to LEFT OUTER JOIN
                            // will be dealt with at the end
                            //if (leftEntityFinalJoinType == SqlJoinType.NotSpecified)
                            //{
                            //    leftEntityFinalJoinType = SqlJoinType.LeftOuterJoin;
                            //}
                        }
                        else
                        {
                            continue;
                        }
                        PropertyMapping[] leftJoinColumns = leftJoinEntityRelationship.ReferencingKeyProperties;

                        // get the columns involved in the relationship on the right entity
                        PropertyMapping[] rightJoinColumns;
                        if (leftJoinParentChildRelationship)
                        {
                            // we've found a parent-child relationship on the left entity, we'll be looking for a mandatory child-parent relationship on the right entity
                            EntityMappingRelationship rightJoinEntityRelationship;
                            if (rightJoinSqlBuilder.EntityMapping.ChildParentRelationships.TryGetValue(leftJoinSqlBuilder.EntityMapping.EntityType, out rightJoinEntityRelationship))
                            {
                                rightJoinColumns = rightJoinEntityRelationship.ReferencingKeyProperties;
                            }
                            else
                            {
                                throw new InvalidOperationException($"Could not find a matching child-to-parent relationship defined on '{rightJoinSqlBuilder.EntityMapping.EntityType}' involving '{leftJoinSqlBuilder.EntityMapping.EntityType}'");
                            }
                        }
                        else
                        {
                            // we've found a child-parent relationship on the left entity, we'll be looking for an optional parent-child relationship on the right entity
                            EntityMappingRelationship rightJoinEntityRelationship;
                            if (rightJoinSqlBuilder.EntityMapping.ParentChildRelationships.TryGetValue(leftJoinSqlBuilder.EntityMapping.EntityType, out rightJoinEntityRelationship))
                            {
                                rightJoinColumns = rightJoinEntityRelationship.ReferencingKeyProperties;
                            }
                            else
                            {
                                // the child collection could be missing, and that's ok, we'll assume the primary keys are the ones we need
                                rightJoinColumns = rightJoinSqlBuilder.KeyProperties;
                            }
                        }

                        joinClauseBuilder.Append('(');
                        for (var leftJoinColumnIndex = 0; leftJoinColumnIndex<leftJoinColumns.Length; leftJoinColumnIndex++)
                        {
                            if (leftJoinColumnIndex > 0)
                            {
                                joinClauseBuilder.Append(" AND ");
                            }

                            var leftJoinColumn = leftJoinColumns[leftJoinColumnIndex];
                            joinClauseBuilder.Append(leftJoinSqlBuilder.GetColumnName(leftJoinColumn, leftJoinSqlBuilder.GetTableName(), false));
                            joinClauseBuilder.Append('=');

                            // search for the corresponding column in the current entity
                            // we're doing this by index, since both sides had the relationship columns already ordered
                            if (leftJoinColumnIndex >= rightJoinColumns.Length)
                            {
                                throw new InvalidOperationException($"Property '{leftJoinColumn.PropertyName}' on the entity '{leftJoinSqlBuilder.EntityMapping.EntityType}' has no matching relationship on the entity '{rightJoinSqlBuilder.EntityMapping.EntityType}'");
                            }

                            var rightJoinColumn = rightJoinColumns[leftJoinColumnIndex];
                            joinClauseBuilder.Append(rightJoinSqlBuilder.GetColumnName(rightJoinColumn, rightJoinSqlBuilder.GetTableName(), false));

                        }
                        joinClauseBuilder.Append(')');
                    }

                    if (!atLeastOneRelationshipDiscovered)
                    {
                        throw new InvalidOperationException($"Could not find a relationship defined on '{leftJoinSqlBuilder.EntityMapping.EntityType}'");
                    }

                    // construct the final join condition for the entity
                    switch (leftEntityFinalJoinType)
                    {
                        case SqlJoinType.LeftOuterJoin:
                        case SqlJoinType.NotSpecified:
                            fromClauseBuilder.Append(" LEFT OUTER JOIN ");
                            break;
                        case SqlJoinType.InnerJoin:
                            fromClauseBuilder.Append(" JOIN ");
                            break;
                        default:
                            throw new NotSupportedException($"Join '{leftEntityFinalJoinType}' is not supported");
                    }

                    fromClauseBuilder.Append(leftJoinSqlBuilder.GetTableName());
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
    }
}
