namespace Dapper.FastCrud.SqlStatements
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// SQL statement factory targeting relationships.
    /// </summary>
    /// <typeparam name="TMainEntity">Main entity type</typeparam>
    /// <typeparam name="TFirstJoinedEntity">Joined entity type</typeparam>
    internal class TwoEntitiesRelationshipSqlStatements<TMainEntity, TFirstJoinedEntity> :RelationshipSqlStatements<TMainEntity>
    {
        private readonly ISqlStatements<TFirstJoinedEntity> _joinedEntitySqlStatements;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="mainEntitySqlStatements">Main entity SQL statement builder</param>
        /// <param name="joinedEntitySqlStatements">Joined entity SQL statement builder</param>
        public TwoEntitiesRelationshipSqlStatements(ISqlStatements<TMainEntity> mainEntitySqlStatements, ISqlStatements<TFirstJoinedEntity> joinedEntitySqlStatements)
            : base(mainEntitySqlStatements)
        {
            _joinedEntitySqlStatements = joinedEntitySqlStatements;
        }

        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        public override ISqlStatements<TMainEntity> CombineWith<TSecondJoinedEntity>(ISqlStatements<TSecondJoinedEntity> joinedEntitySqlStatements)
        {
            return new ThreeEntitiesRelationshipSqlStatements<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity>(this, _joinedEntitySqlStatements, joinedEntitySqlStatements);
        }

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        public override TMainEntity SelectById(IDbConnection connection, TMainEntity keyEntity, AggregatedSqlStatementOptions<TMainEntity> statementOptions)
        {
            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement, 
                out splitOnCondition, 
                this.ConstructJoinInstructions(statementOptions, _joinedEntitySqlStatements.SqlBuilder),
                whereClause:$"{this.SqlBuilder.ConstructKeysWhereClause(this.SqlBuilder.GetTableName())}");

            var relationshipInstanceBuilder = new RelationshipEntityInstanceBuilder(this.SqlBuilder.EntityMapping, _joinedEntitySqlStatements.SqlBuilder.EntityMapping);

            return connection.Query<TMainEntity, TFirstJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, joinedEntity) =>
                {
                    relationshipInstanceBuilder.Add(ref mainEntity, ref joinedEntity);
                    return mainEntity;
                },
                keyEntity,
                splitOn:splitOnCondition,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds).SingleOrDefault();
        }

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        public override async Task<TMainEntity> SelectByIdAsync(IDbConnection connection, TMainEntity keyEntity, AggregatedSqlStatementOptions<TMainEntity> statementOptions)
        {
            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitySqlStatements.SqlBuilder),
                whereClause: $"{this.SqlBuilder.ConstructKeysWhereClause(this.SqlBuilder.GetTableName())}");

            var relationshipInstanceBuilder = new RelationshipEntityInstanceBuilder(this.SqlBuilder.EntityMapping, _joinedEntitySqlStatements.SqlBuilder.EntityMapping);

            var queriedEntities = await connection.QueryAsync<TMainEntity, TFirstJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, joinedEntity) =>
                {
                    relationshipInstanceBuilder.Add(ref mainEntity, ref joinedEntity);
                    return mainEntity;
                },
                keyEntity,
                splitOn: splitOnCondition,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);

            return queriedEntities.SingleOrDefault();
        }

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        public override int Count(IDbConnection connection, AggregatedSqlStatementOptions<TMainEntity> statementOptions)
        {
            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitySqlStatements.SqlBuilder),
                selectClause:this.SqlBuilder.ConstructCountSelectClause(),                
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
        public override Task<int> CountAsync(IDbConnection connection, AggregatedSqlStatementOptions<TMainEntity> statementOptions)
        {
            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitySqlStatements.SqlBuilder),
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
        public override IEnumerable<TMainEntity> BatchSelect(IDbConnection connection, AggregatedSqlStatementOptions<TMainEntity> statementOptions)
        {
            Requires.Argument((statementOptions.LimitResults == null && statementOptions.SkipResults == null) || (statementOptions.OrderClause != null || statementOptions.RelationshipOptions.Values.Any(singleJoinOptions => singleJoinOptions.OrderClause!=null)), nameof(statementOptions), 
                "When using Top or Skip, you must provide an OrderBy clause.");

            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitySqlStatements.SqlBuilder),
                whereClause: statementOptions.WhereClause,
                orderClause: statementOptions.OrderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);

            var relationshipInstanceBuilder = new RelationshipEntityInstanceBuilder(this.SqlBuilder.EntityMapping, _joinedEntitySqlStatements.SqlBuilder.EntityMapping);

            return connection.Query<TMainEntity, TFirstJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, joinedEntity) =>
                {
                    relationshipInstanceBuilder.Add(ref mainEntity, ref joinedEntity);
                    return mainEntity;
                },
                statementOptions.Parameters,
                splitOn: splitOnCondition,
                buffered: !statementOptions.ForceStreamResults,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        public override Task<IEnumerable<TMainEntity>> BatchSelectAsync(IDbConnection connection, AggregatedSqlStatementOptions<TMainEntity> statementOptions)
        {
            Requires.Argument((statementOptions.LimitResults == null && statementOptions.SkipResults == null) || (statementOptions.OrderClause != null || statementOptions.RelationshipOptions.Values.Any(singleJoinOptions => singleJoinOptions.OrderClause != null)), nameof(statementOptions),
                "When using Top or Skip, you must provide an OrderBy clause.");

            string statement;
            string splitOnCondition;

            this.SqlBuilder.ConstructFullJoinSelectStatement(
                out statement,
                out splitOnCondition,
                this.ConstructJoinInstructions(statementOptions, _joinedEntitySqlStatements.SqlBuilder),
                whereClause: statementOptions.WhereClause,
                orderClause: statementOptions.OrderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);

            var relationshipInstanceBuilder = new RelationshipEntityInstanceBuilder(this.SqlBuilder.EntityMapping, _joinedEntitySqlStatements.SqlBuilder.EntityMapping);

            return connection.QueryAsync<TMainEntity, TFirstJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, joinedEntity) =>
                {
                    relationshipInstanceBuilder.Add(ref mainEntity, ref joinedEntity);
                    return mainEntity;
                },
                statementOptions.Parameters,
                splitOn: splitOnCondition,
                buffered: !statementOptions.ForceStreamResults,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }
    }
}
