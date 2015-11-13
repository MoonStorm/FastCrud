namespace Dapper.FastCrud.SqlStatements
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.SqlBuilders;

    internal class GenericSqlStatements<TEntity>: ISqlStatements<TEntity>
    {
        private readonly IStatementSqlBuilder _sqlBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GenericSqlStatements(IStatementSqlBuilder sqlBuilder)
        {
            _sqlBuilder = sqlBuilder;
        }

        public IStatementSqlBuilder SqlBuilder => this._sqlBuilder;

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEntity SelectById(IDbConnection connection, TEntity keyEntity, IStandardSqlStatementOptionsGetter statementOptions)
        {
            return connection.Query<TEntity>(
                _sqlBuilder.ConstructFullSingleSelectStatement(),
                keyEntity,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds).SingleOrDefault();
        }

        /// <summary>
        /// Performs an async SELECT operation on a single entity, using its keys
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TEntity> SelectByIdAsync(IDbConnection connection, TEntity keyEntity, IStandardSqlStatementOptionsGetter statementOptions)
        {
            return (await connection.QueryAsync<TEntity>(
                _sqlBuilder.ConstructFullSingleSelectStatement(),
                keyEntity,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds)).SingleOrDefault();
        }


        /// <summary>
        /// Performs an INSERT operation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(IDbConnection connection, TEntity entity, IStandardSqlStatementOptionsGetter statementOptions)
        {
            if (_sqlBuilder.InsertDatabaseGeneratedProperties.Length > 0)
            {
                var insertedEntity =
                    connection.Query<TEntity>(
                        _sqlBuilder.ConstructFullInsertStatement(),
                        entity,
                        transaction: statementOptions.Transaction,
                        commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds).FirstOrDefault();

                // copy all the database generated props back onto our entity
                this.CopyEntity(insertedEntity, entity, _sqlBuilder.InsertDatabaseGeneratedProperties);
            }
            else
            {
                connection.Execute(
                    _sqlBuilder.ConstructFullInsertStatement(),
                    entity,
                    transaction: statementOptions.Transaction,
                    commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
            }
        }

        /// <summary>
        /// Performs an INSERT operation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task InsertAsync(IDbConnection connection, TEntity entity, IStandardSqlStatementOptionsGetter statementOptions)
        {
            if (_sqlBuilder.InsertDatabaseGeneratedProperties.Length > 0)
            {
                var insertedEntity =
                    (await
                     connection.QueryAsync<TEntity>(
                         _sqlBuilder.ConstructFullInsertStatement(),
                         entity,
                         transaction: statementOptions.Transaction,
                         commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds)).FirstOrDefault();
                // copy all the database generated props back onto our entity
                this.CopyEntity(insertedEntity, entity, _sqlBuilder.InsertDatabaseGeneratedProperties);
            }
            else
            {
                connection.Execute(
                    _sqlBuilder.ConstructFullInsertStatement(),
                    entity,
                    transaction: statementOptions.Transaction,
                    commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
            }
        }

        /// <summary>
        /// Performs an UPDATE opration on an entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateById(IDbConnection connection, TEntity keyEntity, IStandardSqlStatementOptionsGetter statementOptions)
        {
            return connection.Execute(
                _sqlBuilder.ConstructFullSingleUpdateStatement(), 
                keyEntity, 
                transaction: 
                statementOptions.Transaction, 
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        /// Performs an UPDATE opration on an entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> UpdateByIdAsync(
            IDbConnection connection,
            TEntity keyEntity,
            IStandardSqlStatementOptionsGetter statementOptions)
        {
            return (await connection.ExecuteAsync(
                _sqlBuilder.ConstructFullSingleUpdateStatement(), 
                keyEntity, 
                transaction: statementOptions.Transaction, 
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds)) > 0;
        }

        /// <summary>
        /// Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BatchUpdate(IDbConnection connection, TEntity entity, IConditionalSqlStatementOptionsGetter statementOptions)
        {
            return connection.Execute(
                _sqlBuilder.ConstructFullBatchUpdateStatement(statementOptions.WhereClause),
                entity,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> BatchUpdateAsync(IDbConnection connection, TEntity entity, IConditionalSqlStatementOptionsGetter statementOptions)
        {
            return connection.ExecuteAsync(
                _sqlBuilder.ConstructFullBatchUpdateStatement(statementOptions.WhereClause),
                entity,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DeleteById(IDbConnection connection, TEntity keyEntity, IStandardSqlStatementOptionsGetter statementOptions)
        {
            return connection.Execute(
                _sqlBuilder.ConstructFullSingleDeleteStatement(),
                keyEntity,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        /// Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> DeleteByIdAsync(IDbConnection connection, TEntity keyEntity, IStandardSqlStatementOptionsGetter statementoptions)
        {
            return await connection.ExecuteAsync(
                _sqlBuilder.ConstructFullSingleDeleteStatement(),
                keyEntity,
                transaction: statementoptions.Transaction,
                commandTimeout: (int?)statementoptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BatchDelete(IDbConnection connection, IConditionalSqlStatementOptionsGetter statementOptions)
        {
            return connection.Execute(
                _sqlBuilder.ConstructFullBatchDeleteStatement(statementOptions.WhereClause),
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout:(int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> BatchDeleteAsync(IDbConnection connection, IConditionalSqlStatementOptionsGetter statementOptions)
        {
            return connection.ExecuteAsync(
                _sqlBuilder.ConstructFullBatchDeleteStatement(statementOptions.WhereClause),
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(IDbConnection connection, IConditionalSqlStatementOptionsGetter statementOptions)
        {
            return connection.ExecuteScalar<int>(
                _sqlBuilder.ConstructFullCountStatement(statementOptions.WhereClause),
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> CountAsync(IDbConnection connection, IConditionalSqlStatementOptionsGetter statementOptions)
        {
            return connection.ExecuteScalarAsync<int>(
                _sqlBuilder.ConstructFullCountStatement(statementOptions.WhereClause),
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TEntity> BatchSelect(IDbConnection connection, IRangedConditionalResultsSqlStatementGetter statementoptions)
        {
            return connection.Query<TEntity>(
                _sqlBuilder.ConstructFullBatchSelectStatement(
                    whereClause: statementoptions.WhereClause,
                    orderClause: statementoptions.OrderClause,
                    skipRowsCount: statementoptions.SkipResults,
                    limitRowsCount: statementoptions.LimitResults),
                statementoptions.Parameters,
                buffered: !statementoptions.ForceStreamResults,
                transaction: statementoptions.Transaction,
                commandTimeout: (int?)statementoptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<TEntity>> BatchSelectAsync(IDbConnection connection, IRangedConditionalResultsSqlStatementGetter statementoptions)
        {
            return connection.QueryAsync<TEntity>(
                _sqlBuilder.ConstructFullBatchSelectStatement(
                    whereClause: statementoptions.WhereClause,
                    orderClause: statementoptions.OrderClause,
                    skipRowsCount: statementoptions.SkipResults,
                    limitRowsCount: statementoptions.LimitResults),
                statementoptions.Parameters,
                transaction: statementoptions.Transaction,
                commandTimeout: (int?)statementoptions.CommandTimeout?.TotalSeconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopyEntity(TEntity source, TEntity destination, PropertyMapping[] properties)
        {
            foreach (var propMapping in properties)
            {
                var propDescriptor = propMapping.Descriptor;
                var updatedKeyValue = propDescriptor.GetValue(source);
                propDescriptor.SetValue(destination, updatedKeyValue);
            }
        }
    }
}
