namespace Dapper.FastCrud.SqlStatements
{
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.SqlBuilders;
    using System.Linq;

    /// <summary>
    /// SQL statement factory targeting relationships.
    /// </summary>
    /// <typeparam name="TEntity">Target entity type</typeparam>
    internal abstract class RelationshipSqlStatements<TEntity> : ISqlStatements<TEntity>
    {
        private readonly ISqlStatements<TEntity> _mainEntitySqlStatements;
        private readonly GenericStatementSqlBuilder[] _joinedEntitiesSqlBuilders;
        private readonly EntityMapping[] _allEntityMappings;

        /// <summary>
        /// Constructor that takes as arguments the statements for an existing relationship plus a newly joined entity sql builder.
        /// </summary>
        protected RelationshipSqlStatements(RelationshipSqlStatements<TEntity> relationshipStatements, GenericStatementSqlBuilder newlyJoinedEntitySqlBuilder)
            :this(relationshipStatements._mainEntitySqlStatements, relationshipStatements._joinedEntitiesSqlBuilders,newlyJoinedEntitySqlBuilder)
        {            
        }

        /// <summary>
        /// Constructor that takes as arguments the sql statements for the main entity plus a newly joined entity sql builder.
        /// </summary>
        protected RelationshipSqlStatements(ISqlStatements<TEntity> mainEntitySqlStatements, GenericStatementSqlBuilder newlyJoinedEntitySqlBuilder)
            :this(mainEntitySqlStatements, null, newlyJoinedEntitySqlBuilder)
        {            
        }

        private RelationshipSqlStatements(ISqlStatements<TEntity> mainEntitySqlStatements, GenericStatementSqlBuilder[] joinedEntitiesSqlBuilders, GenericStatementSqlBuilder newlyJoinedEntitySqlBuilder)
        {
            _mainEntitySqlStatements = mainEntitySqlStatements;

            var alreadyJoinedEntitiesCount = joinedEntitiesSqlBuilders?.Length??0;

            _joinedEntitiesSqlBuilders = new GenericStatementSqlBuilder[alreadyJoinedEntitiesCount + 1];
            _allEntityMappings = new EntityMapping[alreadyJoinedEntitiesCount + 2];
            for (var joinedSqlBuilderIndex = 0; joinedSqlBuilderIndex < alreadyJoinedEntitiesCount; joinedSqlBuilderIndex++)
            {
                _allEntityMappings[joinedSqlBuilderIndex + 1] = joinedEntitiesSqlBuilders[joinedSqlBuilderIndex].EntityMapping;
                _joinedEntitiesSqlBuilders[joinedSqlBuilderIndex] = joinedEntitiesSqlBuilders[joinedSqlBuilderIndex];
            }

            _joinedEntitiesSqlBuilders[alreadyJoinedEntitiesCount] = newlyJoinedEntitySqlBuilder;
            _allEntityMappings[0] = mainEntitySqlStatements.SqlBuilder.EntityMapping;
            _allEntityMappings[alreadyJoinedEntitiesCount + 1] = newlyJoinedEntitySqlBuilder.EntityMapping;
        }

        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        public abstract ISqlStatements<TEntity> CombineWith<TJoinedEntity>(ISqlStatements<TJoinedEntity> joinedEntitySqlStatements);

        /// <summary>
        /// Gets the publicly accessible SQL builder.
        /// </summary>
        public GenericStatementSqlBuilder SqlBuilder => _mainEntitySqlStatements.SqlBuilder;

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        public TEntity SelectById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitiesSqlBuilders),
                whereClause: $"{this.SqlBuilder.ConstructKeysWhereClause(this.SqlBuilder.GetTableName())}");

            var relationshipInstanceBuilder = new RelationshipEntityInstanceBuilder(_allEntityMappings);
            var queriedEntityIdentities = this.Query(connection,
                statement, 
                splitOnCondition,
                keyEntity,
                false,
                statementOptions.Transaction, 
                (int?)statementOptions.CommandTimeout?.TotalSeconds, relationshipInstanceBuilder);
            return this.FilterDuplicates(queriedEntityIdentities).SingleOrDefault();
        }

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        public async Task<TEntity> SelectByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitiesSqlBuilders),
                whereClause: $"{this.SqlBuilder.ConstructKeysWhereClause(this.SqlBuilder.GetTableName())}");

            var relationshipInstanceBuilder = new RelationshipEntityInstanceBuilder(_allEntityMappings);
            var queriedEntityIdentities  = await this.QueryAsync(connection, 
                statement, 
                splitOnCondition, 
                keyEntity,
                false,
                statementOptions.Transaction, 
                (int?)statementOptions.CommandTimeout?.TotalSeconds, relationshipInstanceBuilder);

            // a problem in the Dapper library would cause this function to fail
            // see https://github.com/StackExchange/dapper-dot-net/issues/596 for more info
            return this.FilterDuplicates(queriedEntityIdentities).SingleOrDefault();
        }

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        public int Count(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitiesSqlBuilders),
                selectClause: this.SqlBuilder.ConstructCountSelectClause(),
                whereClause: statementOptions.WhereClause);

            return connection.ExecuteScalar<int>(
                statement,
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        public Task<int> CountAsync(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitiesSqlBuilders),
                selectClause: this.SqlBuilder.ConstructCountSelectClause(),
                whereClause: statementOptions.WhereClause);

            return connection.ExecuteScalarAsync<int>(
                statement,
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        public IEnumerable<TEntity> BatchSelect(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            //validation removed, up to the engine to fail
            //Requires.Argument((statementOptions.LimitResults == null && statementOptions.SkipResults == null) || (statementOptions.OrderClause != null || statementOptions.RelationshipOptions.Values.Any(singleJoinOptions => singleJoinOptions.OrderClause != null)), nameof(statementOptions),
            //    "When using Top or Skip, you must provide an OrderBy clause.");

            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitiesSqlBuilders),
                whereClause: statementOptions.WhereClause,
                orderClause: statementOptions.OrderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);

            var relationshipInstanceBuilder = new RelationshipEntityInstanceBuilder(_allEntityMappings);

            var queriedEntityIdentities = this.Query(connection,
                              statement,
                              splitOnCondition,
                              statementOptions.Parameters,
                              !statementOptions.ForceStreamResults,
                              statementOptions.Transaction,
                              (int?)statementOptions.CommandTimeout?.TotalSeconds,
                              relationshipInstanceBuilder);
            return this.FilterDuplicates(queriedEntityIdentities);
        }

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        public async Task<IEnumerable<TEntity>> BatchSelectAsync(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            //validation removed, up to the engine to fail
            //Requires.Argument((statementOptions.LimitResults == null && statementOptions.SkipResults == null) || (statementOptions.OrderClause != null || statementOptions.RelationshipOptions.Values.Any(singleJoinOptions => singleJoinOptions.OrderClause != null)), nameof(statementOptions),
            //    "When using Top or Skip, you must provide an OrderBy clause.");

            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitiesSqlBuilders),
                whereClause: statementOptions.WhereClause,
                orderClause: statementOptions.OrderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);

            var relationshipInstanceBuilder = new RelationshipEntityInstanceBuilder(_allEntityMappings);

            var queriedEntityIdentities = await this.QueryAsync(connection,
                              statement,
                              splitOnCondition,
                              statementOptions.Parameters,
                              !statementOptions.ForceStreamResults,
                              statementOptions.Transaction,
                              (int?)statementOptions.CommandTimeout?.TotalSeconds,
                              relationshipInstanceBuilder);

            // a problem in the Dapper library would cause this function to fail
            // see https://github.com/StackExchange/dapper-dot-net/issues/596 for more info
            return this.FilterDuplicates(queriedEntityIdentities);
        }

        /// <summary>
        /// Performs an INSERT operation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            _mainEntitySqlStatements.Insert(connection, entity, statementOptions);
        }

        /// <summary>
        /// Performs an INSERT operation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task InsertAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return _mainEntitySqlStatements.InsertAsync(connection, entity, statementOptions);
        }

        /// <summary>
        /// Performs an UPDATE operation on an entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return _mainEntitySqlStatements.UpdateById(connection, keyEntity, statementOptions);
        }

        /// <summary>
        /// Performs an UPDATE operation on an entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> UpdateByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return _mainEntitySqlStatements.UpdateByIdAsync(connection, keyEntity, statementOptions);
        }

        /// <summary>
        /// Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BulkUpdate(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return _mainEntitySqlStatements.BulkUpdate(connection, entity, statementOptions);
        }

        /// <summary>
        /// Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> BulkUpdateAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return _mainEntitySqlStatements.BulkUpdateAsync(connection, entity, statementOptions);
        }

        /// <summary>
        /// Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DeleteById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return _mainEntitySqlStatements.DeleteById(connection, keyEntity, statementOptions);
        }

        /// <summary>
        /// Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> DeleteByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementoptions)
        {
            return _mainEntitySqlStatements.DeleteByIdAsync(connection, keyEntity, statementoptions);
        }

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BulkDelete(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return _mainEntitySqlStatements.BulkDelete(connection, statementOptions);
        }

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> BulkDeleteAsync(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            return _mainEntitySqlStatements.BulkDeleteAsync(connection, statementOptions);
        }

        protected IEnumerable<StatementSqlBuilderJoinInstruction> ConstructJoinInstructions(AggregatedSqlStatementOptions<TEntity> statementOptions, params GenericStatementSqlBuilder[] joinedEntitySqlBuilders)
        {
            foreach (var joinedEntitySqlBuilder in joinedEntitySqlBuilders)
            {
                var joinedEntityOptions = statementOptions.RelationshipOptions[joinedEntitySqlBuilder.EntityMapping.EntityType];
                yield return new StatementSqlBuilderJoinInstruction(joinedEntitySqlBuilder, joinedEntityOptions.JoinType, joinedEntityOptions.WhereClause, joinedEntityOptions.OrderClause);
            }
        }

        protected abstract IEnumerable<RelationshipEntityInstanceIdentity<TEntity>> Query(
            IDbConnection connection, 
            string statement, 
            string splitOnCondition, 
            object parameters,
            bool buffered,
            IDbTransaction transaction, 
            int? commandTimeout, 
            RelationshipEntityInstanceBuilder relationshipInstanceBuilder);

        protected abstract Task<IEnumerable<RelationshipEntityInstanceIdentity<TEntity>>> QueryAsync(
            IDbConnection connection,
            string statement,
            string splitOnCondition,
            object parameters,
            bool buffered,
            IDbTransaction transaction,
            int? commandTimeout,
            RelationshipEntityInstanceBuilder relationshipInstanceBuilder);

        private IEnumerable<TEntity> FilterDuplicates(IEnumerable<RelationshipEntityInstanceIdentity<TEntity>> entityIdentities)
        {
            return entityIdentities.Where(entityIdentity => !entityIdentity.IsDuplicate).Select(entityIdentity => entityIdentity.TypedInstance);
        }
    }
}
