namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// SQL statement factory targeting relationships.
    /// </summary>
    internal class MultipleEntitiesRelationshipSqlStatements<TMainEntity> : RelationshipSqlStatements<TMainEntity>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="relationshipSqlStatements">Main entity SQL statement builder</param>
        /// <param name="joinedEntitySqlStatements">Joined entity SQL statement builder</param>
        public MultipleEntitiesRelationshipSqlStatements(RelationshipSqlStatements<TMainEntity> relationshipSqlStatements, GenericStatementSqlBuilder joinedEntitySqlStatements)
            : base(relationshipSqlStatements, joinedEntitySqlStatements)
        {
        }

        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        public override ISqlStatements<TMainEntity> CombineWith<TJoinedEntity>(ISqlStatements<TJoinedEntity> joinedEntitySqlStatements)
        {
            return new MultipleEntitiesRelationshipSqlStatements<TMainEntity>(this, joinedEntitySqlStatements.SqlBuilder);
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

            var types = new[] { typeof(TMainEntity) };
            types = types.Concat(this._joinedEntitiesSqlBuilders.Select(x => x.EntityMapping.EntityType)).ToArray();

            return connection.Query<RelationshipEntityInstanceIdentity<TMainEntity>>(statement, types,
                (resultEntity) =>
                {
                    var mainEntityIdentity = relationshipInstanceBuilder.RegisterResultSetRowInstance((TMainEntity)resultEntity[0]);

                    for (var i = 0; i < resultEntity.Length; i++)
                    {
                        if (i > 0)
                        {
                            relationshipInstanceBuilder.RegisterResultSetRowInstance(resultEntity[i], types[i].FullName);
                        }
                    }

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
            var types = new[] { typeof(TMainEntity) };
            types = types.Concat(this._joinedEntitiesSqlBuilders.Select(x => x.EntityMapping.EntityType)).ToArray();

            return connection.QueryAsync<RelationshipEntityInstanceIdentity<TMainEntity>>(statement, types,
                (resultEntity) =>
                {
                    var mainEntityIdentity = relationshipInstanceBuilder.RegisterResultSetRowInstance((TMainEntity)resultEntity[0]);

                    for (var i = 0; i < resultEntity.Length; i++)
                    {
                        if (i > 0)
                        {
                            relationshipInstanceBuilder.RegisterResultSetRowInstance(resultEntity[i], types[i].FullName);
                        }
                    }

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