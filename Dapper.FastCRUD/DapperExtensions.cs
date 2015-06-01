namespace Dapper.FastCrud
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    public static class DapperExtensions
    {
        /// <summary>
        /// Queries the database for a single record based on its primary keys.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityKeys">The entity from which the primary keys will be extracted and used for filtering.</param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>Returns a single entity by a single id from table or NULL if none could be found.</returns>
        public static TEntity Get<TEntity>(
            this IDbConnection connection, 
            TEntity entityKeys, 
            IDbTransaction transaction = null, 
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).SingleSelect(connection, entityKeys, transaction, commandTimeout);
        }

        /// <summary>
        /// Returns all the records in the database.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="streamResult">If set to true, the resulting list of entities is not entirely loaded in memory. This is useful for processing large result sets.</param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>Gets a list of all entities</returns>
        public static IEnumerable<TEntity> Get<TEntity>(
            this IDbConnection connection,
            bool streamResult = false,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null, 
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).BatchSelect(
                connection,
                streamResults: streamResult,
                transaction: transaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// Queries the database for a set of records.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="queryParameters"></param>
        /// <param name="streamResult">If set to true, the resulting list of entities is not entirely loaded in memory. This is useful for processing large result sets.</param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="whereClause">Where clause (e.g. $"{nameof(User.Name)} = @UserName and {nameof(User.LoggedIn)} = @UserLoggedIn" )</param>
        /// <param name="orderClause">Order clause (e.g. $"{nameof(User.Name)} ASC, {nameof(User.LoggedIn)} DESC" )</param>
        /// <param name="skipRowsCount">Number of rows to skip.</param>
        /// <param name="limitRowsCount">Maximum number of rows to return.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>Gets a list of all entities</returns>
        public static IEnumerable<TEntity> Find<TEntity>(
            this IDbConnection connection,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null,
            bool streamResult = false, 
            IDbTransaction transaction = null, 
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).BatchSelect(
                connection, 
                whereClause:whereClause,
                orderClause:orderClause,
                skipRowsCount:skipRowsCount,
                limitRowsCount:limitRowsCount,
                queryParameters:queryParameters,
                streamResults:streamResult,
                transaction:transaction,
                commandTimeout:commandTimeout);
        }

        /// <summary>
        /// Inserts an entity into the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        public static void Insert<TEntity>(
            this IDbConnection connection, 
            TEntity entityToInsert, 
            IDbTransaction transaction = null, 
            TimeSpan? commandTimeout = null, 
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).SingleInsert(connection, entityToInsert, transaction: transaction, commandTimeout: commandTimeout);
        }


        /// <summary>
        /// Updates a record in the database.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToUpdate">The entity you wish to update.</param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>True if the item was updated.</returns>
        public static bool Update<TEntity>(
            this IDbConnection connection, 
            TEntity entityToUpdate, 
            IDbTransaction transaction = null, 
            TimeSpan? commandTimeout = null, 
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).SingleUpdate(connection, entityToUpdate, transaction: transaction, commandTimeout: commandTimeout);
        }

        /// <summary>
        /// Deletes an entity by its primary keys.
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToDelete">The entity you wish to remove.</param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>True if the entity was found and successfully deleted.</returns>
        public static bool Delete<TEntity>(
            this IDbConnection connection, 
            TEntity entityToDelete, 
            IDbTransaction transaction = null, 
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).SingleDelete(connection, entityToDelete, transaction: transaction, commandTimeout: commandTimeout);
        }

        /// <summary>
        /// Gets the table name for an entity type
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        public static ISqlBuilder GetSqlBuilder<TEntity>(this IDbConnection connection, EntityMapping<TEntity> entityMappingOverride = null)
        {
            return OrmConfiguration.GetSqlBuilder<TEntity>(entityMappingOverride);
        }

    }
}
