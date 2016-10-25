namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// SQL statement factory targeting relationships.
    /// </summary>
    internal class SevenEntitiesRelationshipSqlStatements<TMainEntity,
                                                          TFirstJoinedEntity,
                                                          TSecondJoinedEntity,
                                                          TThirdJoinedEntity,
                                                          TFourthJoinedEntity,
                                                          TFifthJoinedEntity,
                                                          TSixthJoinedEntity>
        : RelationshipSqlStatements<TMainEntity>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SevenEntitiesRelationshipSqlStatements(
            SixEntitiesRelationshipSqlStatements<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity, TFourthJoinedEntity, TFifthJoinedEntity> relationshipSqlStatements,
            GenericStatementSqlBuilder joinedEntitySqlStatements)
            : base(relationshipSqlStatements, joinedEntitySqlStatements)
        {
        }

        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        public override ISqlStatements<TMainEntity> CombineWith<TSeventhJoinedEntity>(ISqlStatements<TSeventhJoinedEntity> joinedEntitySqlStatements)
        {
            throw new NotSupportedException("Only 7 entities are allowed in a relationship");
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
            return connection.Query<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity, TFourthJoinedEntity, TFifthJoinedEntity, TSixthJoinedEntity, RelationshipEntityInstanceIdentity<TMainEntity>>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity, thirdJoinedEntity, fourthJoinedEntity, fifthJoinedEntity, sixthJoinedEntity) =>
                {
                    var mainEntityIdentity = relationshipInstanceBuilder.RegisterResultSetRowInstance(mainEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(firstJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(secondJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(thirdJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(fourthJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(fifthJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(sixthJoinedEntity);
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
            return connection.QueryAsync<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity, TThirdJoinedEntity, TFourthJoinedEntity, TFifthJoinedEntity, TSixthJoinedEntity, RelationshipEntityInstanceIdentity<TMainEntity>>(
                statement,
                (mainEntity, firstJoinedEntity, secondJoinedEntity, thirdJoinedEntity, fourthJoinedEntity, fifthJoinedEntity, sixthJoinedEntity) =>
                {
                    var mainEntityIdentity = relationshipInstanceBuilder.RegisterResultSetRowInstance(mainEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(firstJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(secondJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(thirdJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(fourthJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(fifthJoinedEntity);
                    relationshipInstanceBuilder.RegisterResultSetRowInstance(sixthJoinedEntity);
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