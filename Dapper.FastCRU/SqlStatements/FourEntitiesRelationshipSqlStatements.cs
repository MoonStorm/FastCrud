namespace Dapper.FastCrud.SqlStatements
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// SQL statement factory targeting relationships.
    /// </summary>
    internal class FourEntitiesRelationshipSqlStatements<TMainEntity,
                                                         TFirstJoinedEntity,
                                                         TSecondJoinedEntity,
                                                         TThirdJoinedEntity>
        : RelationshipSqlStatements<TMainEntity>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FourEntitiesRelationshipSqlStatements(ThreeEntitiesRelationshipSqlStatements<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity> relationshipSqlStatements, GenericStatementSqlBuilder joinedEntitySqlStatements)
            : base(relationshipSqlStatements, joinedEntitySqlStatements)
        {
        }

        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        public override ISqlStatements<TMainEntity> CombineWith<TFourthJoinedEntity>(ISqlStatements<TFourthJoinedEntity> joinedEntitySqlStatements)
        {
            return new FiveEntitiesRelationshipSqlStatements<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity, TFourthJoinedEntity>(this, joinedEntitySqlStatements.SqlBuilder);
        }

        protected override IEnumerable<RelationshipEntityInstanceIdentity<TMainEntity>> Query(
            IDbConnection connection,
            string statement,
            string splitOnCondition,
            object parameters,
            bool buffered,
            IDbTransaction transaction,
            int? commandTimeout,
            RelationshipEntityInstanceBuilder relationshipInstanceBuilder)
        {
            return connection.Query<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity, RelationshipEntityInstanceIdentity<TMainEntity>>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity, thirdJoinedEntity) =>
                {
                    var mainEntityIdentity = relationshipInstanceBuilder.RegisterResultSetRowInstance(mainEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(firstJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(secondJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(thirdJoinedEntity);
                    relationshipInstanceBuilder.EndResultSetRow();

                    return mainEntityIdentity;
                },
                parameters,
                buffered: buffered,
                splitOn: splitOnCondition,
                transaction: transaction,
                commandTimeout: commandTimeout);
        }

        protected override Task<IEnumerable<RelationshipEntityInstanceIdentity<TMainEntity>>> QueryAsync(
            IDbConnection connection,
            string statement,
            string splitOnCondition,
            object parameters,
            bool buffered,
            IDbTransaction transaction,
            int? commandTimeout,
            RelationshipEntityInstanceBuilder relationshipInstanceBuilder)
        {
            return connection.QueryAsync<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity, RelationshipEntityInstanceIdentity<TMainEntity>>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity, thirdJoinedEntity) =>
                {
                    var mainEntityIdentity = relationshipInstanceBuilder.RegisterResultSetRowInstance(mainEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(firstJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(secondJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(thirdJoinedEntity);
                    relationshipInstanceBuilder.EndResultSetRow();

                    return mainEntityIdentity;
                },
                parameters,
                buffered: buffered,
                splitOn: splitOnCondition,
                transaction: transaction,
                commandTimeout: commandTimeout);
        }
    }
}