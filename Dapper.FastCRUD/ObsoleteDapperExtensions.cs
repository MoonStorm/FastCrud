namespace Dapper.FastCrud
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Builders;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    [Obsolete("Will be removed in a future version", false)]
    public static class ObsoleteDapperExtensions
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
        [Obsolete("Will be removed in a future version", false)]
        public static TEntity Get<TEntity>(
            this IDbConnection connection, 
            TEntity entityKeys, 
            IDbTransaction transaction, 
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.Get(entityKeys, statement => SetupStandardStatementOptions(
                statement, 
                transaction, 
                commandTimeout, 
                entityMappingOverride));
        }

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
        [Obsolete("Will be removed in a future version", false)]
        public static Task<TEntity> GetAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityKeys,
            IDbTransaction transaction,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.GetAsync(entityKeys, statement => SetupStandardStatementOptions(
                statement, 
                transaction, 
                commandTimeout, 
                entityMappingOverride));
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
        [Obsolete("Will be removed in a future version", false)]
        public static IEnumerable<TEntity> Get<TEntity>(
            this IDbConnection connection,
            bool streamResult = false,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null, 
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.Find<TEntity>(
                statement => SetupRangedStatementOptions(
                    statement,
                    transaction,
                    commandTimeout,
                    entityMappingOverride,
                    streamResults: streamResult));
        }

        /// <summary>
        /// Returns all the records in the database.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>Gets a list of all entities</returns>
        [Obsolete("Will be removed in a future version", false)]
        public static Task<IEnumerable<TEntity>> GetAsync<TEntity>(
            this IDbConnection connection,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.FindAsync<TEntity>(
                statement => SetupRangedStatementOptions(
                    statement,
                    transaction,
                    commandTimeout,
                    entityMappingOverride));
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
        [Obsolete("Will be removed in a future version", false)]
        public static IEnumerable<TEntity> Find<TEntity>(
            this IDbConnection connection,
            FormattableString whereClause,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null,
            bool streamResult = false,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.Find<TEntity>(
                statement => SetupRangedStatementOptions(
                    statement,
                    transaction,
                    commandTimeout,
                    entityMappingOverride,
                    whereClause,
                    queryParameters,
                    orderClause,
                    skipRowsCount,
                    limitRowsCount,
                    streamResult));
        }

        /// <summary>
        /// Queries the database for a set of records.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="queryParameters"></param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="whereClause">Where clause (e.g. $"{nameof(User.Name)} = @UserName and {nameof(User.LoggedIn)} = @UserLoggedIn" )</param>
        /// <param name="orderClause">Order clause (e.g. $"{nameof(User.Name)} ASC, {nameof(User.LoggedIn)} DESC" )</param>
        /// <param name="skipRowsCount">Number of rows to skip.</param>
        /// <param name="limitRowsCount">Maximum number of rows to return.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>Gets a list of all entities</returns>
        [Obsolete("Will be removed in a future version", false)]
        public static Task<IEnumerable<TEntity>> FindAsync<TEntity>(
            this IDbConnection connection,
            FormattableString whereClause,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.FindAsync<TEntity>(
                statement => SetupRangedStatementOptions(
                    statement,
                    transaction,
                    commandTimeout,
                    entityMappingOverride,
                    whereClause,
                    queryParameters,
                    orderClause,
                    skipRowsCount,
                    limitRowsCount));
        }

        /// <summary>
        /// Inserts a row into the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        [Obsolete("Will be removed in a future version", false)]
        public static void Insert<TEntity>(
            this IDbConnection connection, 
            TEntity entityToInsert, 
            IDbTransaction transaction, 
            TimeSpan? commandTimeout = null, 
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            connection.Insert(entityToInsert, statement => SetupStandardStatementOptions(
                statement, 
                transaction, 
                commandTimeout, 
                entityMappingOverride));
        }

        /// <summary>
        /// Inserts an entity into the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        [Obsolete("Will be removed in a future version", false)]
        public static Task InsertAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityToInsert,
            IDbTransaction transaction,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.InsertAsync(entityToInsert, statement => SetupStandardStatementOptions(
                statement,
                transaction,
                commandTimeout,
                entityMappingOverride));
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
        [Obsolete("Will be removed in a future version", false)]
        public static bool Update<TEntity>(
            this IDbConnection connection, 
            TEntity entityToUpdate, 
            IDbTransaction transaction, 
            TimeSpan? commandTimeout = null, 
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.Update(entityToUpdate, statement => SetupStandardStatementOptions(
                statement,
                transaction,
                commandTimeout,
                entityMappingOverride));
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
        [Obsolete("Will be removed in a future version", false)]
        public static Task<bool> UpdateAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityToUpdate,
            IDbTransaction transaction,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.UpdateAsync(entityToUpdate, statement => SetupStandardStatementOptions(
                statement,
                transaction,
                commandTimeout,
                entityMappingOverride));
        }

        /// <summary>
        /// Gets the count of a set of records.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="queryParameters"></param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="whereClause">Where clause (e.g. $"{nameof(User.Name)} = @UserName and {nameof(User.LoggedIn)} = @UserLoggedIn" )</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>Gets a list of all entities</returns>
        [Obsolete("Will be removed in a future version", false)]
        public static int Count<TEntity>(
            this IDbConnection connection,
            FormattableString whereClause,
            object queryParameters = null,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.Count<TEntity>(statement => SetupConditionalStatementOptions(
                statement,
                transaction,
                commandTimeout,
                entityMappingOverride,
                whereClause,
                queryParameters));
        }

        /// <summary>
        /// Queries the database for a set of records.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="queryParameters"></param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="whereClause">Where clause (e.g. $"{nameof(User.Name)} = @UserName and {nameof(User.LoggedIn)} = @UserLoggedIn" )</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>Gets a list of all entities</returns>
        [Obsolete("Will be removed in a future version", false)]
        public static Task<int> CountAsync<TEntity>(
            this IDbConnection connection,
            FormattableString whereClause,
            object queryParameters = null,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.CountAsync<TEntity>(statement => SetupConditionalStatementOptions(
                statement,
                transaction,
                commandTimeout,
                entityMappingOverride,
                whereClause,
                queryParameters));
        }

        /// <summary>
        /// Deletes a record by its primary keys.
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToDelete">The entity you wish to remove.</param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>True if the entity was found and successfully deleted.</returns>
        [Obsolete("Will be removed in a future version", false)]
        public static bool Delete<TEntity>(
            this IDbConnection connection, 
            TEntity entityToDelete, 
            IDbTransaction transaction, 
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.Delete(entityToDelete, statement => SetupStandardStatementOptions(
                statement,
                transaction,
                commandTimeout,
                entityMappingOverride));
        }

        /// <summary>
        /// Deletes a record by its primary keys.
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToDelete">The entity you wish to remove.</param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>True if the entity was found and successfully deleted.</returns>
        [Obsolete("Will be removed in a future version", false)]
        public static Task<bool> DeleteAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityToDelete,
            IDbTransaction transaction,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return connection.DeleteAsync(entityToDelete, statement => SetupStandardStatementOptions(
                statement,
                transaction,
                commandTimeout,
                entityMappingOverride));
        }

        /// <summary>
        /// Returns an SQL builder helpful for constructing verbatim SQL queries.
        /// This will just forward the call to <see cref="OrmConfiguration.GetSqlBuilder{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entityMappingOverride">If NULL, de default entity mapping will be used.</param>
        /// <param name="connection">Not used.</param>
        [Obsolete("Please use OrmConfiguration.GetSqlBuilder<TEntity> instead",false)]
        public static ISqlBuilder GetSqlBuilder<TEntity>(this IDbConnection connection, EntityMapping<TEntity> entityMappingOverride = null)
        {
            return OrmConfiguration.GetSqlBuilder(entityMappingOverride);
        }

        private static void SetupRangedStatementOptions<TEntity>(
            IRangedBatchSqlStatementOptionsOptionsBuilder<TEntity> optionsOptionsBuilder,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null,
            FormattableString whereClause = null,
            object queryParameters = null,
            FormattableString orderClause = null,
            long? skipRowsCount = null,
            long? limitRowsCount = null,
            bool streamResults = false
            )
        {
            if (transaction != null)
                optionsOptionsBuilder.AttachToTransaction(transaction);
            if (commandTimeout != null)
                optionsOptionsBuilder.WithTimeout(commandTimeout.Value);
            if (entityMappingOverride != null)
                optionsOptionsBuilder.WithEntityMappingOverride(entityMappingOverride);
            if (whereClause != null)
                optionsOptionsBuilder.Where(whereClause);
            if (queryParameters != null)
                optionsOptionsBuilder.WithParameters(queryParameters);
            if (orderClause != null)
                optionsOptionsBuilder.OrderBy(orderClause);
            if (skipRowsCount.HasValue)
                optionsOptionsBuilder.Skip(skipRowsCount.Value);
            if (limitRowsCount.HasValue)
                optionsOptionsBuilder.Top(limitRowsCount.Value);
            if (streamResults)
                optionsOptionsBuilder.StreamResults();
        }

        private static void SetupConditionalStatementOptions<TEntity>(
            IConditionalSqlStatementOptionsBuilder<TEntity> optionsBuilder,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null,
            FormattableString whereClause = null,
            object queryParameters = null
            )
        {
            if (transaction != null)
                optionsBuilder.AttachToTransaction(transaction);
            if (commandTimeout != null)
                optionsBuilder.WithTimeout(commandTimeout.Value);
            if (entityMappingOverride != null)
                optionsBuilder.WithEntityMappingOverride(entityMappingOverride);
            if (whereClause != null)
                optionsBuilder.Where(whereClause);
            if (queryParameters != null)
                optionsBuilder.WithParameters(queryParameters);
        }

        private static void SetupStandardStatementOptions<TEntity>(
            IStandardSqlStatementOptionsBuilder<TEntity> optionsBuilder,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null
            )
        {
            if (transaction != null)
                optionsBuilder.AttachToTransaction(transaction);
            if (commandTimeout != null)
                optionsBuilder.WithTimeout(commandTimeout.Value);
            if (entityMappingOverride != null)
                optionsBuilder.WithEntityMappingOverride(entityMappingOverride);
        }
    }
}
