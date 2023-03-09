namespace Dapper.FastCrud.SqlBuilders
{
    using Dapper.FastCrud.Configuration.StatementOptions;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlStatements.MultiEntity;
    using Dapper.FastCrud.Validations;
    using System.Text;

    internal abstract class GenericStatementSqlBuilder:ISqlBuilder
    {
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
            EntityRegistration entityRegistration,
            SqlDialect dialect)
        {
            var databaseOptions = OrmConfiguration.Conventions.GetDatabaseOptions(dialect);
            this.UsesSchemaForTableNames = databaseOptions.IsUsingSchemas;
            this.IdentifierStartDelimiter = databaseOptions.StartDelimiter;
            this.IdentifierEndDelimiter = databaseOptions.EndDelimiter;
            this.ParameterPrefix = databaseOptions.ParameterPrefix;
            this.ParameterSuffix = databaseOptions.ParameterSuffix; // Necessary for Sybase Anyhwere SQL

            //_entityRelationships = new ConcurrentDictionary<IStatementSqlBuilder, EntityRelationship>();
            //_regularStatementFormatter = new SqlStatementFormatter(entityDescriptor, entityMapping, this, false);
            //_forcedTableResolutionStatementFormatter = new SqlStatementFormatter(entityDescriptor, entityMapping, this, true);

            this.EntityDescriptor = entityDescriptor;
            this.EntityRegistration = entityRegistration;

            this.SelectProperties = this.EntityRegistration.GetAllOrderedFrozenPropertyRegistrations()
                .ToArray();
            this.KeyProperties = this.EntityRegistration.GetAllOrderedFrozenPrimaryKeyRegistrations()
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
            _noConditionFullCountStatement = new Lazy<string>(()=>this.ConstructFullCountStatementInternal(null, null, false, null),LazyThreadSafetyMode.PublicationOnly);
            _fullSingleSelectStatement = new Lazy<string>(()=>this.ConstructFullSingleSelectStatementInternal(),LazyThreadSafetyMode.PublicationOnly);
        }
        
        /// <summary>
        /// The entity descriptor the current instance of sql builder was created for.
        /// </summary>
        public EntityDescriptor EntityDescriptor { get; }

        /// <summary>
        /// The entity registration the current instance of sql builder was created for.
        /// </summary>
        public EntityRegistration EntityRegistration { get; }

        /// <summary>
        /// All the properties that participate in selects.
        /// </summary>
        public PropertyRegistration[] SelectProperties { get; }

        /// <summary>
        /// All the primary key properties.
        /// </summary>
        public PropertyRegistration[] KeyProperties { get; }

        /// <summary>
        /// All the properties that participate in inserts.
        /// </summary>
        public PropertyRegistration[] InsertProperties { get; }

        /// <summary>
        /// All the properties that participate in updates.
        /// </summary>
        public PropertyRegistration[] UpdateProperties { get; }

        /// <summary>
        /// Primary key properties that are generated on inserts.
        /// </summary>
        public PropertyRegistration[] InsertKeyDatabaseGeneratedProperties { get; }
        
        /// <summary>
        /// Properties that require to be updated on inserts.
        /// </summary>
        public PropertyRegistration[] RefreshOnInsertProperties { get; }
        
        /// <summary>
        /// Properties that require to be updated on updates.
        /// </summary>
        public PropertyRegistration[] RefreshOnUpdateProperties { get; }

        /// <summary>
        /// Delimiter to be used at the start of identifiers (e.g. [UserTable])
        /// </summary>
        protected string IdentifierStartDelimiter { get; }

        /// <summary>
        /// Delimiter to be used at the end of identifiers (e.g. [UserTable])
        /// </summary>
        protected string IdentifierEndDelimiter { get; }

        /// <summary>
        /// If true, schema qualified table names should be used.
        /// </summary>
        protected bool UsesSchemaForTableNames { get; }

        /// <summary>
        /// The prefix that needs to be used for sql parameters (e.g. @UserId)
        /// </summary>
        protected string ParameterPrefix { get; }

        /// <summary>
        /// The suffix that needs to be used for sql parameters (e.g. ?UserId?)
        /// </summary>
        protected string ParameterSuffix { get; }

        #region section for all the methods exposed both publicly and internally

        /// <summary>
        /// Returns a SQL parameter, prefixed as set in the database dialect options.
        /// <param name="parameterName">The name of the parameter. It is recommended to use nameof.</param>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetPrefixedParameter(string parameterName)
        {
            Validate.NotNullOrEmpty(parameterName, nameof(parameterName));

            return FormattableString.Invariant($"{this.ParameterPrefix}{parameterName}{this.ParameterSuffix}");
        }

        /// <summary>
        /// Returns the table name associated with the current entity.
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        /// <param name="normalizeAlias">If set, table AS alias is returned.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetTableName(string? tableAlias = null, bool normalizeAlias = false)
        {
            if (tableAlias == null)
            {
                return _noAliasTableName.Value;
            }

            if (normalizeAlias)
            {
                return this.GetTableNameInternal(tableAlias);
            }

            return this.GetDelimitedIdentifier(tableAlias);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property, string? tableAlias = null)
        {
            Validate.NotNull(property, nameof(property));

            var propName = ((MemberExpression)property.Body).Member.Name;
            return this.GetColumnName(propName, tableAlias);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// If the column name differs from the name of the property, this method will normalize the name (e.g. will return 'tableAlias.colName AS propName')
        ///   so that the deserialization performed by Dapper would succeed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnNameForSelect(string propertyName, string? tableAlias = null)
        {
            Validate.NotNullOrEmpty(propertyName, nameof(propertyName));

            var targetPropertyRegistration = this.EntityRegistration.TryGetFrozenPropertyRegistrationByPropertyName(propertyName);
            if (targetPropertyRegistration == null)
            {
                throw new ArgumentException($"Property '{propertyName}' was not found on '{this.EntityRegistration.EntityType}'");
            }

            return this.GetColumnName(targetPropertyRegistration, tableAlias, true);
        }

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="tableAlias">Table alias</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetColumnName(string propertyName, string? tableAlias = null)
        {
            var targetPropertyMapping = this.EntityRegistration.TryGetFrozenPropertyRegistrationByPropertyName(propertyName);
            if(targetPropertyMapping == null)
            {
                throw new ArgumentException($"Property '{propertyName}' was not found on '{this.EntityRegistration.EntityType}'");
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
        internal string GetColumnName(PropertyRegistration propMapping, string? tableAlias = null, bool performColumnAliasNormalization = false)
        {
            var sqlTableAlias = tableAlias == null ? string.Empty : $"{this.GetDelimitedIdentifier(tableAlias)}.";
            var sqlColumnAlias = (performColumnAliasNormalization && propMapping.DatabaseColumnName != propMapping.PropertyName)
                                     ? $" AS {this.GetDelimitedIdentifier(propMapping.PropertyName)}"
                                     : string.Empty;
            return FormattableString.Invariant($"{sqlTableAlias}{this.GetDelimitedIdentifier(propMapping.DatabaseColumnName)}{sqlColumnAlias}");
        }

        /// <summary>
        /// Constructs a condition of form <code>ColumnName=@PropertyName and ...</code> with all the key columns (e.g. <code>Id=@Id and EmployeeId=@EmployeeId</code>)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructKeysWhereClause(string? tableAlias = null)
        {
            return tableAlias == null ? _noAliasKeysWhereClause.Value : this.ConstructKeysWhereClauseInternal(tableAlias);
        }

        /// <summary>
        /// Constructs an enumeration of the key values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructKeyColumnEnumeration(string? tableAlias = null)
        {
            return tableAlias == null ? _noAliasKeyColumnEnumeration.Value : this.ConstructKeyColumnEnumerationInternal(tableAlias);
        }

        /// <summary>
        /// Constructs an enumeration of all the selectable columns (i.e. all the columns corresponding to entity properties which are not part of a relationship).
        /// (e.g. Id, HouseNo, AptNo)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConstructColumnEnumerationForSelect(string? tableAlias = null)
        {
            return this.ConstructColumnEnumerationForSelect(tableAlias, null);
        }

        /// <summary>
        /// Constructs an enumeration of all the selectable columns, including the ones of the entities participating in JOINs.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal string ConstructColumnEnumerationForSelect(string? tableAlias, SqlStatementJoin[]? joins)
        {
            var joinsRequiringColumnEnumeration = joins?.Where(join => join.RequiresResultMapping).ToArray();

            // we might not return columns from the join, but we need to qualify the columns in case joins with other tables are still present
            if ((joinsRequiringColumnEnumeration?.Length ?? 0) == 0 && joins?.Length > 0 && tableAlias==null)
            {
                tableAlias = this.EntityRegistration.TableName;
            }

            return tableAlias == null && (joinsRequiringColumnEnumeration == null || joinsRequiringColumnEnumeration.Length == 0)
                       ? _noAliasColumnEnumerationForSelect.Value : 
                       this.ConstructColumnEnumerationForSelectInternal(tableAlias, joinsRequiringColumnEnumeration);
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
        public string ConstructUpdateClause(string? tableAlias = null)
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
        public string ConstructFullBatchUpdateStatement(string? whereClause = null)
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
        public string ConstructFullBatchDeleteStatement(string? whereClause = null)
        {
            return whereClause == null ? _noConditionFullBatchDeleteStatement.Value : this.ConstructFullBatchDeleteStatementInternal(whereClause);
        }

        /// <summary>
        /// Constructs the count part of the select statement.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal string ConstructCountSelectClause()
        {
            return "COUNT(*)";
        }

        /// <summary>
        /// Constructs a full count statement, optionally with a where clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal string ConstructFullCountStatement(
            string? fromClause,
            string? whereClause,
            bool hasJoins,
            string? mainEntityAliasWhenInAJoin)
        {
            return whereClause == null && fromClause == null && !hasJoins
                       ? _noConditionFullCountStatement.Value 
                       : this.ConstructFullCountStatementInternal(fromClause, whereClause, hasJoins, mainEntityAliasWhenInAJoin);
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
            string selectClause,
            string fromClause,
            string? whereClause,
            string? orderClause,
            long? skipRowsCount,
            long? limitRowsCount)
        {
            return this.ConstructFullSelectStatementInternal(
                selectClause,
                fromClause,
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
        [Obsolete("This method is no longer supported. Use Sql.Format instead.", error:true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Format(FormattableString rawSql)
        {
            // we can't support this method since the SQL builder is completely isolated from the formatter
            throw new NotSupportedException();
            //return this.ResolveWithSqlFormatter(rawSql);
        }

        /// <summary>
        /// Returns a delimited SQL identifier.
        /// </summary>
        /// <param name="sqlIdentifier">Delimited or non-delimited SQL identifier</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetDelimitedIdentifier(string sqlIdentifier)
        {
            Validate.NotNullOrEmpty(sqlIdentifier, nameof(sqlIdentifier));

            var startsWithIdentifier = !string.IsNullOrEmpty(this.IdentifierStartDelimiter) && sqlIdentifier.StartsWith(this.IdentifierStartDelimiter);
            var endsWithIdentifier = !string.IsNullOrEmpty(this.IdentifierEndDelimiter) &&  sqlIdentifier.EndsWith(this.IdentifierEndDelimiter);

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}{2}",
                startsWithIdentifier ? string.Empty : this.IdentifierStartDelimiter,
                sqlIdentifier,
                endsWithIdentifier ? string.Empty : this.IdentifierEndDelimiter);
        }

        /// <summary>
        /// Constructs a split-on expression for a statement containing joins.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal string ConstructSplitOnExpression(SqlStatementJoin[] joins)
        {
            return string.Join(
                    ",", 
                    joins.Where(join => join.RequiresResultMapping)
                         .Select(join => join.ReferencedEntitySqlBuilder.SelectProperties[0].PropertyName));
        }

        /// <summary>
        /// Returns a WHERE clause.
        /// </summary>
        internal virtual string? ConstructWhereClause(GenericSqlStatementFormatter formatter, FormattableString? mainWhere, SqlStatementJoin[]? joins = null)
        {
            string? mainWhereClause = mainWhere?.ToString(formatter);

            var joinsWithLegacyExtraWhereClauses = joins?.Where(join => join.JoinExtraWhereClause != null).ToArray();
            if (joinsWithLegacyExtraWhereClauses == null || joinsWithLegacyExtraWhereClauses.Length == 0)
            {
                return mainWhereClause;
            }

            FormattableString finalWhereClause;
            var requiresFirstAnd = false;
            if (mainWhereClause != null)
            {
                finalWhereClause = $"{mainWhereClause}";
                requiresFirstAnd = true;
            }
            else
            {
                finalWhereClause = $"";
            }

            var firstJoin = true;
            foreach (var join in joinsWithLegacyExtraWhereClauses)
            {
                if (!firstJoin || requiresFirstAnd)
                {
                    finalWhereClause = $"({finalWhereClause}) AND";
                }

                firstJoin = false;
                using (formatter.SetActiveMainResolver(join.ReferencedEntityFormatterResolver, true))
                {
                    finalWhereClause = $"{finalWhereClause} ({join.JoinExtraWhereClause!.ToString(formatter)})";
                }
            }

            return FormattableString.Invariant(finalWhereClause);
        }

        /// <summary>
        /// Returns an ORDER BY clause.
        /// </summary>
        internal virtual string? ConstructOrderClause(GenericSqlStatementFormatter formatter, FormattableString? mainOrder, SqlStatementJoin[]? joins = null)
        {
            string? mainOrderClause = mainOrder?.ToString(formatter);

            var joinsWithLegacyOrderClauses = joins?.Where(join => join.JoinExtraOrderByClause != null).ToArray();
            if (joinsWithLegacyOrderClauses == null || joinsWithLegacyOrderClauses.Length == 0)
            {
                return mainOrderClause;
            }

            FormattableString finalOrderClause;
            var requiresFirstComma = false;
            if (mainOrderClause != null)
            {
                finalOrderClause = $"{mainOrderClause}";
                requiresFirstComma = true;
            }
            else
            {
                finalOrderClause = $"";
            }

            var firstJoin = true;
            foreach (var join in joinsWithLegacyOrderClauses)
            {
                if (!firstJoin || requiresFirstComma)
                {
                    finalOrderClause = $"{finalOrderClause},";
                }

                firstJoin = false;
                using (formatter.SetActiveMainResolver(join.ReferencedEntityFormatterResolver, true))
                {
                    finalOrderClause = $"{finalOrderClause} {join.JoinExtraOrderByClause!.ToString(formatter)}";
                }
            }

            return FormattableString.Invariant(finalOrderClause);
        }

        /// <summary>
        /// Returns a FROM clause.
        /// </summary>
        internal virtual string ConstructFromClause(GenericSqlStatementFormatter formatter, string? tableAlias = null, SqlStatementJoin[]? joins = null)
        {
            Validate.NotNull(formatter, nameof(formatter));

            var mainTableFromClause = this.GetTableNameInternal(tableAlias);

            if (joins == null || joins.Length == 0)
            {
                return mainTableFromClause;
            }

            FormattableString finalFromClause = $"{mainTableFromClause}";
            foreach (var join in joins)
            {
                // mainTable JOIN refTableOrAlias
                finalFromClause = $"{finalFromClause} {this.ResolveSqlJoinType(join.JoinType)} {join.ReferencedEntitySqlBuilder.GetTableNameInternal(join.ReferencedEntityFormatterResolver?.Alias)}";

                // use the join condition, if provided
                if (join.JoinOnClause != null)
                {
                    using (formatter.SetActiveMainResolver(join.ReferencedEntityFormatterResolver, true))
                    {
                        finalFromClause = $"{finalFromClause} ON {join.JoinOnClause.ToString(formatter)}";
                    }
                }
                else
                {
                    finalFromClause = $"{finalFromClause} ON ";

                    var firstColumnMatching = true;
                    foreach (var joinRelationship in join.ResolvedRelationships
                                                         .Where(relationship => relationship.ReferencingColumnProperties != null && relationship.ReferencedColumnProperties != null))
                    {
                        for (var columnIndex = 0; columnIndex < joinRelationship.ReferencingColumnProperties.Length; columnIndex++)
                        {
                            if (firstColumnMatching)
                            {
                                firstColumnMatching = false;
                            }
                            else
                            {
                                finalFromClause = $"{finalFromClause} AND";
                            }

                            var referencingFullyQualifiedColumn = joinRelationship.ReferencingEntitySqlBuilder.GetColumnName(
                                joinRelationship.ReferencingColumnProperties[columnIndex],
                                joinRelationship.ReferencingEntityFormatterResolver!.Alias ?? joinRelationship.ReferencingEntityRegistration.TableName,
                                false);
                            var referencedFullyQualifiedColumn = joinRelationship.ReferencedEntitySqlBuilder.GetColumnName(
                                joinRelationship.ReferencedColumnProperties[columnIndex],
                                joinRelationship.ReferencedEntityFormatterResolver.Alias ?? joinRelationship.ReferencedEntityRegistration.TableName,
                                false);

                            finalFromClause = $"{finalFromClause} {referencingFullyQualifiedColumn} = {referencedFullyQualifiedColumn}";
                        }
                    }
                }
            }

            return FormattableString.Invariant(finalFromClause);
        }

        #endregion

        #region section for all the virtual methods visible only by the sql builder implementations

        /// <summary>
        /// Constructs an enumeration of all the selectable columns.
        /// </summary>
        protected virtual string ConstructColumnEnumerationForSelectInternal(string? mainTableAlias = null, SqlStatementJoin[]? joins = null)
        {
            if (joins?.Length > 0)
            {
                mainTableAlias ??= this.EntityRegistration.TableName;
            }

            var mainEntityColumnEnumeration = string.Join(",", this.SelectProperties
                                                                   .Select(propInfo => this.GetColumnName(propInfo, mainTableAlias, true)));

            var joinEntityColumnEnumerations = joins?.Select(join => join.ReferencedEntitySqlBuilder.ConstructColumnEnumerationForSelect(join.ReferencedEntityFormatterResolver?.Alias ?? join.ReferencedEntityRegistration.TableName));

            var finalEntityColumnEnumerations = (joins == null || joins.Length == 0)
                                                    ? mainEntityColumnEnumeration
                                                    : string.Join(", ", new[] { mainEntityColumnEnumeration }.Concat(joinEntityColumnEnumerations!));

            return finalEntityColumnEnumerations;
        }

        /// <summary>
        /// Resolves a SQL join type.
        /// </summary>
        protected virtual string ResolveSqlJoinType(SqlJoinType joinType)
        {
            return joinType switch
            {
                SqlJoinType.LeftOuterJoin => "LEFT JOIN",
                SqlJoinType.InnerJoin => "JOIN",
                _ => throw new InvalidOperationException($"Unable to resolve the relationship {joinType}")
            };
        }

        /// <summary>
        /// Returns the table name associated with the current entity.
        /// </summary>
        protected virtual string GetTableNameInternal(string? tableAliasToNormalize = null)
        {
            var sqlAlias = tableAliasToNormalize == null
                ? string.Empty
                : $" AS {this.GetDelimitedIdentifier(tableAliasToNormalize)}";

            FormattableString fullTableName;
            fullTableName = $"{this.GetDelimitedIdentifier(this.EntityRegistration.TableName)}";
            if (this.UsesSchemaForTableNames && !string.IsNullOrEmpty(this.EntityRegistration.SchemaName))
            {
                fullTableName = $"{this.GetDelimitedIdentifier(this.EntityRegistration.SchemaName)}.{fullTableName}";
            }

            if (this.UsesSchemaForTableNames && !string.IsNullOrEmpty(this.EntityRegistration.DatabaseName))
            {
                if (string.IsNullOrEmpty(this.EntityRegistration.SchemaName))
                {
                    fullTableName = $".{fullTableName}";
                }
                fullTableName = $"{this.GetDelimitedIdentifier(this.EntityRegistration.DatabaseName)}.{fullTableName}";
            }

            return FormattableString.Invariant($"{fullTableName}{sqlAlias}");
        }

        /// <summary>
        /// Constructs a condition of form <code>ColumnName=@PropertyName and ...</code> with all the key columns (e.g. <code>Id=@Id and EmployeeId=@EmployeeId</code>)
        /// </summary>
        protected virtual string ConstructKeysWhereClauseInternal(string? tableAlias = null)
        {
            return string.Join(" AND ", this.KeyProperties.Select(propInfo => $"{this.GetColumnName(propInfo, tableAlias, false)}={this.GetPrefixedParameter(propInfo.PropertyName)}"));
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
        protected virtual string ConstructKeyColumnEnumerationInternal(string? tableAlias = null)
        {
            return string.Join(",", this.KeyProperties.Select(propInfo => this.GetColumnName(propInfo, tableAlias, true)));
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
            return string.Join(",", this.InsertProperties.Select(propInfo => $"{this.ParameterPrefix}{propInfo.PropertyName}{this.ParameterSuffix}"));
        }

        /// <summary>
        /// Constructs a update clause of form <code>ColumnName=@PropertyName, ...</code> with all the updateable columns (e.g. <code>EmployeeId=@EmployeeId,DeskNo=@DeskNo</code>)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        protected virtual string ConstructUpdateClauseInternal(string tableAlias = null)
        {
            return string.Join(",", UpdateProperties.Select(propInfo => $"{this.GetColumnName(propInfo, tableAlias, false)}={this.ParameterPrefix}{propInfo.PropertyName}{this.ParameterSuffix}"));
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
                throw new NotSupportedException($"Entity '{this.EntityRegistration.EntityType.Name}' has no primary key. UPDATE is not possible.");
            }

            FormattableString sql = $"UPDATE {this.GetTableName()} SET {this.ConstructUpdateClause()} WHERE {this.ConstructKeysWhereClause()}";
            if (this.RefreshOnUpdateProperties.Length > 0)
            {
                sql = $"{sql};SELECT {this.ConstructRefreshOnUpdateColumnSelection()} FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()};";
            }

            return FormattableString.Invariant(sql);
        }

        /// <summary>
        /// Constructs a batch select statement.
        /// </summary>
        protected virtual string ConstructFullBatchUpdateStatementInternal(string? whereClause = null)
        {
            FormattableString updateStatement = $"UPDATE {this.GetTableName()} SET {this.ConstructUpdateClause()}";
            if (whereClause != null)
            {
                return FormattableString.Invariant($"{updateStatement} WHERE {whereClause}");
            }

            return FormattableString.Invariant(updateStatement);
        }

        /// <summary>
        /// Constructs a delete statement for a single entity.
        /// </summary>
        protected virtual string ConstructFullSingleDeleteStatementInternal()
        {
            if (this.KeyProperties.Length == 0)
            {
                throw new NotSupportedException($"Entity '{this.EntityRegistration.EntityType.Name}' has no primary key. DELETE is not possible.");
            }

            return FormattableString.Invariant($"DELETE FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}");
        }

        /// <summary>
        /// Constructs a batch delete statement.
        /// </summary>
        protected virtual string ConstructFullBatchDeleteStatementInternal(string? whereClause = null)
        {
            FormattableString deleteStatement = $"DELETE FROM {this.GetTableName()}";
            if (whereClause != null)
            {
                return FormattableString.Invariant($"{deleteStatement} WHERE {whereClause}");
            }

            return FormattableString.Invariant(deleteStatement);
        }

        /// <summary>
        /// Constructs a full count statement, optionally with a where clause.
        /// </summary>
        protected virtual string ConstructFullCountStatementInternal(string? fromClause, string? whereClause, bool joinsPresent, string? mainEntityAliasWhenInAJoin)
        {
            FormattableString statement;
            if (joinsPresent)
            {
                statement = $@"SELECT 
                        {this.ConstructCountSelectClause()} 
                        FROM (SELECT DISTINCT {(this.KeyProperties.Length > 0
                                                    ? this.ConstructKeyColumnEnumeration(mainEntityAliasWhenInAJoin??this.EntityRegistration.TableName) 
                                                    : this.ConstructColumnEnumerationForSelect(mainEntityAliasWhenInAJoin??this.EntityRegistration.TableName))} 
                                FROM {fromClause ?? this.GetTableName()}
                                {(whereClause!=null? $" WHERE {whereClause}": string.Empty)}
                             ) AS {this.GetDelimitedIdentifier(Guid.NewGuid().ToString("N"))}";
            }
            else
            {
                statement = $@"SELECT {this.ConstructCountSelectClause()} FROM {fromClause ?? this.GetTableName()} {(whereClause != null ? $"WHERE {whereClause}" : string.Empty)}";
            }
            return FormattableString.Invariant(statement);
        }

        /// <summary>
        /// Constructs a select statement for a single entity
        /// </summary>
        protected virtual string ConstructFullSingleSelectStatementInternal()
        {
            if (this.KeyProperties.Length == 0)
            {
                throw new NotSupportedException($"Entity '{this.EntityRegistration.EntityType.Name}' has no primary key. SELECT is not possible.");
            }

            return FormattableString.Invariant($"SELECT {this.ConstructColumnEnumerationForSelect()} FROM {this.GetTableName()} WHERE {this.ConstructKeysWhereClause()}");
        }

        /// <summary>
        /// Constructs a full select statement.
        /// </summary>
        protected abstract string ConstructFullSelectStatementInternal(
            string selectClause,
            string fromClause,
            string? whereClause = null,
            string? orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null);

    #endregion
    }
}
