namespace Dapper.FastCrud
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions;
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
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        public static TEntity Get<TEntity>(
            this IDbConnection connection,
            TEntity entityKeys,
            Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new StandardSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return OrmConfiguration.GetSqlStatements<TEntity>(options.EntityMappingOverride)
                                   .SelectById(connection, entityKeys, options);
        }

        /// <summary>
        /// Queries the database for a single record based on its primary keys.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityKeys">The entity from which the primary keys will be extracted and used for filtering.</param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>Returns a single entity by a single id from table or NULL if none could be found.</returns>
        public static Task<TEntity> GetAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityKeys,
            Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new StandardSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return OrmConfiguration.GetSqlStatements<TEntity>(options.EntityMappingOverride)
                                   .SelectByIdAsync(connection, entityKeys, options);
        }

        /// <summary>
        /// Inserts an entity into the database, updating its properties based on the database generated fields.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        public static void Insert<TEntity>(
            this IDbConnection connection,
            TEntity entityToInsert,
            Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new StandardSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            OrmConfiguration.GetSqlStatements<TEntity>(options.EntityMappingOverride)
                            .Insert(connection, entityToInsert, options);
        }

        /// <summary>
        /// Inserts an entity into the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        public static Task InsertAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityToInsert,
            Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new StandardSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return OrmConfiguration.GetSqlStatements<TEntity>(options.EntityMappingOverride)
                                   .InsertAsync(connection, entityToInsert, options);
        }

        /// <summary>
        /// Updates a record in the database.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToUpdate">The entity you wish to update.</param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>True if the item was updated.</returns>
        public static bool Update<TEntity>(
            this IDbConnection connection,
            TEntity entityToUpdate,
            Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new StandardSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return OrmConfiguration.GetSqlStatements<TEntity>(options.EntityMappingOverride)
                                   .UpdateById(connection, entityToUpdate, options);
        }

        /// <summary>
        /// Updates a record in the database.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToUpdate">
        /// The entity you wish to update.
        /// For partial updates use an entity mapping override.
        /// </param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>True if the item was updated.</returns>
        public static Task<bool> UpdateAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityToUpdate,
            Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new StandardSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return OrmConfiguration.GetSqlStatements<TEntity>(options.EntityMappingOverride)
                                   .UpdateByIdAsync(connection, entityToUpdate, options);
        }

        /// <summary>
        /// Updates a number of records in the database.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="updateData">
        /// The data used to update the records. 
        /// The primary keys will be ignored.
        /// For partial updates use an entity mapping override.
        /// </param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>The number of records updated.</returns>
        public static int BatchUpdate<TEntity>(
            this IDbConnection connection,
            TEntity updateData,
            Action<IConditionalSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new ConditionalSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return OrmConfiguration.GetSqlStatements<TEntity>(options.EntityMappingOverride)
                                   .BatchUpdate(connection, updateData, options);
        }

        /// <summary>
        /// Updates a number of records in the database.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <param name="updateData">
        /// The data used to update the records. 
        /// The primary keys will be ignored.
        /// For partial updates use an entity mapping override.
        /// </param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>The number of records updated.</returns>
        public static Task<int> BatchUpdateAsync<TEntity>(
            this IDbConnection connection,
            TEntity updateData,
            Action<IConditionalSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new ConditionalSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return OrmConfiguration.GetSqlStatements<TEntity>(options.EntityMappingOverride)
                                   .BatchUpdateAsync(connection, updateData, options);
        }

        ///// <summary>
        ///// Returns all the records in the database.
        ///// </summary>
        ///// <typeparam name="TEntity">Entity type</typeparam>
        ///// <param name="connection"></param>
        ///// <param name="streamResult">If set to true, the resulting list of entities is not entirely loaded in memory. This is useful for processing large result sets.</param>
        ///// <param name="transaction">Transaction to attach the query to.</param>
        ///// <param name="commandTimeout">The command timeout.</param>
        ///// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        ///// <returns>Gets a list of all entities</returns>
        //public static IEnumerable<TEntity> Get<TEntity>(
        //    this IDbConnection connection,
        //    bool streamResult = false,
        //    IDbTransaction transaction = null,
        //    TimeSpan? commandTimeout = null, 
        //    EntityMapping<TEntity> entityMappingOverride = null)
        //{
        //    return OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).BatchSelect(
        //        connection,
        //        streamResults: streamResult,
        //        transaction: transaction,
        //        commandTimeout: commandTimeout);
        //}

        ///// <summary>
        ///// Returns all the records in the database.
        ///// </summary>
        ///// <typeparam name="TEntity">Entity type</typeparam>
        ///// <param name="connection"></param>
        ///// <param name="transaction">Transaction to attach the query to.</param>
        ///// <param name="commandTimeout">The command timeout.</param>
        ///// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        ///// <returns>Gets a list of all entities</returns>
        //public static async Task<IEnumerable<TEntity>> GetAsync<TEntity>(
        //    this IDbConnection connection,
        //    IDbTransaction transaction = null,
        //    TimeSpan? commandTimeout = null,
        //    EntityMapping<TEntity> entityMappingOverride = null)
        //{
        //    return await OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).BatchSelectAsync(
        //        connection,
        //        transaction: transaction,
        //        commandTimeout: commandTimeout);
        //}

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
                whereClause: whereClause,
                orderClause: orderClause,
                skipRowsCount: skipRowsCount,
                limitRowsCount: limitRowsCount,
                queryParameters: queryParameters,
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
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="whereClause">Where clause (e.g. $"{nameof(User.Name)} = @UserName and {nameof(User.LoggedIn)} = @UserLoggedIn" )</param>
        /// <param name="orderClause">Order clause (e.g. $"{nameof(User.Name)} ASC, {nameof(User.LoggedIn)} DESC" )</param>
        /// <param name="skipRowsCount">Number of rows to skip.</param>
        /// <param name="limitRowsCount">Maximum number of rows to return.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>Gets a list of all entities</returns>
        public static async Task<IEnumerable<TEntity>> FindAsync<TEntity>(
            this IDbConnection connection,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return await OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).BatchSelectAsync(
                connection,
                whereClause: whereClause,
                orderClause: orderClause,
                skipRowsCount: skipRowsCount,
                limitRowsCount: limitRowsCount,
                queryParameters: queryParameters,
                transaction: transaction,
                commandTimeout: commandTimeout);
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
        public static int Count<TEntity>(
            this IDbConnection connection,
            FormattableString whereClause = null,
            object queryParameters = null,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).Count(
                connection,
                whereClause: whereClause,
                queryParameters: queryParameters,
                transaction:transaction,
                commandTimeout: commandTimeout);
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
        public static async Task<int> CountAsync<TEntity>(
            this IDbConnection connection,
            FormattableString whereClause = null,
            object queryParameters = null,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return await OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).CountAsync(
                connection,
                whereClause: whereClause,
                queryParameters: queryParameters,
                transaction:transaction,
                commandTimeout: commandTimeout);
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
        /// Deletes an entity by its primary keys.
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToDelete">The entity you wish to remove.</param>
        /// <param name="transaction">Transaction to attach the query to.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="entityMappingOverride">Overrides the default entity mapping for this call.</param>
        /// <returns>True if the entity was found and successfully deleted.</returns>
        public static async Task<bool> DeleteAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityToDelete,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null,
            EntityMapping<TEntity> entityMappingOverride = null)
        {
            return await OrmConfiguration.GetSqlStatements<TEntity>(entityMappingOverride).SingleDeleteAsync(connection, entityToDelete, transaction: transaction, commandTimeout: commandTimeout);
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
            return OrmConfiguration.GetSqlBuilder<TEntity>(entityMappingOverride);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static ISqlStatementOptionsGetter ResolveOptions<TEntity, TOptionsBuilderInterface>(TOptionsBuilderInterface options, Action<TOptionsBuilderInterface> optionsSetter)
        //    where TOptionsBuilderInterface: class, IStandardSqlStatementOptionsSetter<TEntity,TOptionsBuilderInterface>, ISqlStatementOptionsGetter
        //{
        //    optionsSetter?.Invoke(options);
        //    return options;
        //}
    }
}
