using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Dapper.FastCrud.Tests")]

namespace Dapper.FastCrud
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Builders;

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
            Action<ISelectSqlSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new SelectSqlSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().SelectById(connection, entityKeys, options);
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
            Action<ISelectSqlSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new SelectSqlSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().SelectByIdAsync(connection, entityKeys, options);
        }

        /// <summary>
        /// Inserts a row into the database, updating its properties based on the database generated fields.
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
            options.SqlStatementsFactoryChain().Insert(connection, entityToInsert, options);
        }

        /// <summary>
        /// Inserts a row into the database.
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
            return options.SqlStatementsFactoryChain().InsertAsync(connection, entityToInsert, options);
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
            return options.SqlStatementsFactoryChain().UpdateById(connection, entityToUpdate, options);
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
            return options.SqlStatementsFactoryChain().UpdateByIdAsync(connection, entityToUpdate, options);
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
        public static int BulkUpdate<TEntity>(
            this IDbConnection connection,
            TEntity updateData,
            Action<IConditionalBulkSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new ConditionalBulkSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().BulkUpdate(connection, updateData, options);
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
        public static Task<int> BulkUpdateAsync<TEntity>(
            this IDbConnection connection,
            TEntity updateData,
            Action<IConditionalBulkSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new ConditionalBulkSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().BulkUpdateAsync(connection, updateData, options);
        }

        /// <summary>
        /// Deletes an entity by its primary keys.
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToDelete">The entity you wish to remove.</param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>True if the entity was found and successfully deleted.</returns>
        public static bool Delete<TEntity>(
            this IDbConnection connection,
            TEntity entityToDelete,
            Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new StandardSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().DeleteById(connection, entityToDelete, options);
        }

        /// <summary>
        /// Deletes an entity by its primary keys.
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="entityToDelete">The entity you wish to remove.</param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>True if the entity was found and successfully deleted.</returns>
        public static Task<bool> DeleteAsync<TEntity>(
            this IDbConnection connection,
            TEntity entityToDelete,
            Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new StandardSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().DeleteByIdAsync(connection, entityToDelete, options);
        }

        /// <summary>
        /// Deletes all the records in the table or a range of records if a conditional clause was set up in the statement options.
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>The number of records deleted.</returns>
        public static int BulkDelete<TEntity>(
            this IDbConnection connection,
            Action<IConditionalBulkSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new ConditionalBulkSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().BulkDelete(connection, options);
        }

        /// <summary>
        /// Deletes all the records in the table or a range of records if a conditional clause was set up in the statement options.
        /// </summary>
        /// <typeparam name="TEntity">Entity Type</typeparam>
        /// <param name="connection">Database connection.</param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>The number of records deleted.</returns>
        public static Task<int> BulkDeleteAsync<TEntity>(
            this IDbConnection connection,
            Action<IConditionalBulkSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new ConditionalBulkSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().BulkDeleteAsync(connection, options);
        }

        /// <summary>
        /// Counts all the records in a table or a range of records if a conditional clause was set up in the statement options.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>The record count</returns>
        public static int Count<TEntity>(
            this IDbConnection connection,
            Action<IConditionalSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new ConditionalSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().Count(connection, options);
        }

        /// <summary>
        /// Counts all the records in a table or a range of records if a conditional clause was set up in the statement options.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>The record count</returns>
        public static Task<int> CountAsync<TEntity>(
            this IDbConnection connection,
            Action<IConditionalSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new ConditionalSqlStatementOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().CountAsync(connection, options);
        }

        /// <summary>
        /// Queries the database for a set of records.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>The record count</returns>
        public static IEnumerable<TEntity> Find<TEntity>(
            this IDbConnection connection,
            Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new RangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().BatchSelect(connection, options);
        }

        /// <summary>
        /// Queries the database for a set of records.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="connection"></param>
        /// <param name="statementOptions">Optional statement options (usage: statement => statement.SetTimeout().AttachToTransaction()...)</param>
        /// <returns>The record count</returns>
        public static Task<IEnumerable<TEntity>> FindAsync<TEntity>(
            this IDbConnection connection,
            Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> statementOptions = null)
        {
            var options = new RangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>();
            statementOptions?.Invoke(options);
            return options.SqlStatementsFactoryChain().BatchSelectAsync(connection, options);
        }
    }
}