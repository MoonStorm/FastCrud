namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Resolvers;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// SQL statement factory targeting relationships.
    /// </summary>
    /// <typeparam name="TMainEntity">Main entity type</typeparam>
    /// <typeparam name="TFirstJoinedEntity">Joined entity type</typeparam>
    /// <typeparam name="TSecondJoinedEntity">Joined entity type</typeparam>
    internal class ThreeEntitiesRelationshipSqlStatements<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity>:RelationshipSqlStatements<TMainEntity>
    {
        private readonly ISqlStatements<TFirstJoinedEntity> _firstJoinedEntitySqlStatements;
        private readonly ISqlStatements<TSecondJoinedEntity> _secondJoinedEntitySqlStatements;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="mainEntitySqlStatements">Main entity SQL statement builder</param>
        /// <param name="firstJoinedEntitySqlStatements">Joined entity SQL statement builder</param>
        /// <param name="secondJoinedEntitySqlStatements">Joined entity SQL statement builder</param>
        public ThreeEntitiesRelationshipSqlStatements(
            ISqlStatements<TMainEntity> mainEntitySqlStatements, 
            ISqlStatements<TFirstJoinedEntity> firstJoinedEntitySqlStatements,
            ISqlStatements<TSecondJoinedEntity> secondJoinedEntitySqlStatements)
            : base(mainEntitySqlStatements)
        {
            _firstJoinedEntitySqlStatements = firstJoinedEntitySqlStatements;
            _secondJoinedEntitySqlStatements = secondJoinedEntitySqlStatements;
        }

        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        public override ISqlStatements<TMainEntity> CombineWith<TJoinedEntity1>(ISqlStatements<TJoinedEntity1> joinedEntitySqlStatements)
        {
            throw new NotImplementedException();
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
                this.ConstructJoinInstructions(statementOptions, _firstJoinedEntitySqlStatements.SqlBuilder, _secondJoinedEntitySqlStatements.SqlBuilder),
                whereClause:$"{this.SqlBuilder.ConstructKeysWhereClause(this.SqlBuilder.GetTableName())}");

            return connection.Query<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity) =>
                {
                    this.AttachEntity(this.SqlBuilder.EntityMapping, mainEntity, firstJoinedEntity);
                    this.AttachEntity(_firstJoinedEntitySqlStatements.SqlBuilder.EntityMapping, firstJoinedEntity, secondJoinedEntity);
                    return mainEntity;
                },
                keyEntity,
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
                this.ConstructJoinInstructions(statementOptions, _firstJoinedEntitySqlStatements.SqlBuilder, _secondJoinedEntitySqlStatements.SqlBuilder),
                whereClause: $"{this.SqlBuilder.ConstructKeysWhereClause(this.SqlBuilder.GetTableName())}");

            var queriedEntities = await connection.QueryAsync<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity) =>
                {
                    this.AttachEntity(this.SqlBuilder.EntityMapping, mainEntity, firstJoinedEntity);
                    this.AttachEntity(_firstJoinedEntitySqlStatements.SqlBuilder.EntityMapping, firstJoinedEntity, secondJoinedEntity);
                    return mainEntity;
                },
                keyEntity,
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
                this.ConstructJoinInstructions(statementOptions, _firstJoinedEntitySqlStatements.SqlBuilder, _secondJoinedEntitySqlStatements.SqlBuilder),
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
                this.ConstructJoinInstructions(statementOptions, _firstJoinedEntitySqlStatements.SqlBuilder, _secondJoinedEntitySqlStatements.SqlBuilder),
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
                this.ConstructJoinInstructions(statementOptions, _firstJoinedEntitySqlStatements.SqlBuilder, _secondJoinedEntitySqlStatements.SqlBuilder),
                whereClause: statementOptions.WhereClause,
                orderClause: statementOptions.OrderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);

            return connection.Query<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity) =>
                {
                    this.AttachEntity(this.SqlBuilder.EntityMapping, mainEntity, firstJoinedEntity);
                    this.AttachEntity(_firstJoinedEntitySqlStatements.SqlBuilder.EntityMapping, firstJoinedEntity, secondJoinedEntity);
                    return mainEntity;
                },
                statementOptions.Parameters,
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
                this.ConstructJoinInstructions(statementOptions, _firstJoinedEntitySqlStatements.SqlBuilder, _secondJoinedEntitySqlStatements.SqlBuilder),
                whereClause: statementOptions.WhereClause,
                orderClause: statementOptions.OrderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);

            return connection.QueryAsync<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity) =>
                {
                    this.AttachEntity(this.SqlBuilder.EntityMapping, mainEntity, firstJoinedEntity);
                    this.AttachEntity(_firstJoinedEntitySqlStatements.SqlBuilder.EntityMapping, firstJoinedEntity, secondJoinedEntity);
                    return mainEntity;
                },
                statementOptions.Parameters,
                buffered: !statementOptions.ForceStreamResults,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }
    }
}
