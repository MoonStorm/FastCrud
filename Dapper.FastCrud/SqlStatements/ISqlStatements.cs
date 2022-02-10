namespace Dapper.FastCrud.SqlStatements
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.SqlBuilders;

    internal interface ISqlStatements
    {
        ///// <summary>
        ///// Gets the SQL Builder.
        ///// </summary>
        GenericStatementSqlBuilder SqlBuilder { get; }
    }

    /// <summary>
    /// SQL statement factory.
    /// </summary>
    /// <typeparam name="TEntity">Target entity type</typeparam>
    internal interface ISqlStatements<TEntity>: ISqlStatements
    {
        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        // ISqlStatements<TEntity> CombineWith<TJoinedEntity>(ISqlStatements<TJoinedEntity> joinedEntitySqlStatements);

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        TEntity? SelectById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        Task<TEntity?> SelectByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs an INSERT operation
        /// </summary>
        void Insert(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs an INSERT operation
        /// </summary>
        Task InsertAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs an UPDATE operation on an entity identified by its keys.
        /// </summary>
        bool UpdateById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs an UPDATE operation on an entity identified by its keys.
        /// </summary>
        Task<bool> UpdateByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        int BulkUpdate(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        Task<int> BulkUpdateAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        bool DeleteById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        Task<bool> DeleteByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementoptions);

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        int BulkDelete(IDbConnection connection, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        Task<int> BulkDeleteAsync(IDbConnection connection, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        int Count(IDbConnection connection, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        Task<int> CountAsync(IDbConnection connection, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        IEnumerable<TEntity> BatchSelect(IDbConnection connection, AggregatedSqlStatementOptions statementOptions);

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        Task<IEnumerable<TEntity>> BatchSelectAsync(IDbConnection connection, AggregatedSqlStatementOptions statementoptions);
    }
}
