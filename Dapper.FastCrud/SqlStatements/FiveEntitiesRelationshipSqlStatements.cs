namespace Dapper.FastCrud.SqlStatements
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// SQL statement factory targeting relationships.
    /// </summary>
    internal class FiveEntitiesRelationshipSqlStatements<TMainEntity,
                                                         TFirstJoinedEntity,
                                                         TSecondJoinedEntity,
                                                         TThirdJoinedEntity,
                                                         TFourthJoinedEntity>
        : RelationshipSqlStatements<TMainEntity>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FiveEntitiesRelationshipSqlStatements(FourEntitiesRelationshipSqlStatements<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity> relationshipSqlStatements, GenericStatementSqlBuilder joinedEntitySqlStatements)
            : base(relationshipSqlStatements, joinedEntitySqlStatements)
        {
        }

        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        public override ISqlStatements<TMainEntity> CombineWith<TFifthJoinedEntity>(ISqlStatements<TFifthJoinedEntity> joinedEntitySqlStatements)
        {
            return new SixEntitiesRelationshipSqlStatements<TMainEntity,TFirstJoinedEntity,TSecondJoinedEntity,TThirdJoinedEntity,TFourthJoinedEntity,TFifthJoinedEntity>(this, joinedEntitySqlStatements.SqlBuilder);
        }

        protected override IEnumerable<TMainEntity> Query(
            IDbConnection connection,
            string statement,
            string splitOnCondition,
            object parameters,
            bool buffered,
            IDbTransaction transaction,
            int? commandTimeout,
            RelationshipEntityInstanceBuilder relationshipInstanceBuilder)
        {
            return connection.Query<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity, TFourthJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity, thirdJoinedEntity, fourthJoinedEntity) =>
                {
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref mainEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref firstJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref secondJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref thirdJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref fourthJoinedEntity);
                    relationshipInstanceBuilder.EndResultSetRow();

                    return mainEntity;
                },
                parameters,
                buffered: buffered,
                splitOn: splitOnCondition,
                transaction: transaction,
                commandTimeout: commandTimeout);
        }

        protected override Task<IEnumerable<TMainEntity>> QueryAsync(
            IDbConnection connection,
            string statement,
            string splitOnCondition,
            object parameters,
            bool buffered,
            IDbTransaction transaction,
            int? commandTimeout,
            RelationshipEntityInstanceBuilder relationshipInstanceBuilder)
        {
            return connection.QueryAsync<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity, TFourthJoinedEntity, TMainEntity>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity, thirdJoinedEntity, fourthJoinedEntity) =>
                {
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref mainEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref firstJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref secondJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref thirdJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(ref fourthJoinedEntity);
                    relationshipInstanceBuilder.EndResultSetRow();

                    return mainEntity;
                },
                parameters,
                buffered: buffered,
                splitOn: splitOnCondition,
                transaction: transaction,
                commandTimeout: commandTimeout);
        }
    }
}