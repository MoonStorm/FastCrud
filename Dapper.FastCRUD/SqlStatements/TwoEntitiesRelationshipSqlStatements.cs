namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Resolvers;
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// SQL statement factory targeting relationships.
    /// </summary>
    /// <typeparam name="TMainEntity">Main entity type</typeparam>
    /// <typeparam name="TJoinedEntity">Joined entity type</typeparam>
    internal class TwoEntitiesRelationshipSqlStatements<TMainEntity, TJoinedEntity>:RelationshipSqlStatements<TMainEntity>
    {
        private GenericStatementSqlBuilder _joinedEntitySqlBuilder;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="mainEntitySqlStatements">Main entity SQL statement builder</param>
        /// <param name="joinedEntitySqlBuilder">Joined entity SQL statement builder</param>
        public TwoEntitiesRelationshipSqlStatements(ISqlStatements<TMainEntity> mainEntitySqlStatements, GenericStatementSqlBuilder joinedEntitySqlBuilder)
            : base(mainEntitySqlStatements)
        {
            _joinedEntitySqlBuilder = joinedEntitySqlBuilder;
        }

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        public override TMainEntity SelectById(IDbConnection connection, TMainEntity keyEntity, AggregatedSqlStatementOptions statementOptions)
        {
            throw new NotImplementedException();
        }
    }
}
