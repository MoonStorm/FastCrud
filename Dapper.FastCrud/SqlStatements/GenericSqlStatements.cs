namespace Dapper.FastCrud.SqlStatements
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;

    internal class GenericSqlStatements<TEntity>: ISqlStatements<TEntity>
    {
        private readonly GenericStatementSqlBuilder _sqlBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GenericSqlStatements(GenericStatementSqlBuilder sqlBuilder)
        {
            _sqlBuilder = sqlBuilder;
        }
        
        /// <summary>
        /// Gets the publicly accessible SQL builder.
        /// </summary>
        public GenericStatementSqlBuilder SqlBuilder => _sqlBuilder;

        /// <summary>
        /// Combines the current instance with a joined entity.
        /// </summary>
        public ISqlStatements<TEntity> CombineWith<TJoinedEntity>(ISqlStatements<TJoinedEntity> joinedEntitySqlStatements)
        {
            return new TwoEntitiesRelationshipSqlStatements<TEntity,TJoinedEntity>(this, joinedEntitySqlStatements.SqlBuilder);
        }

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEntity SelectById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
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
        public async Task<TEntity> SelectByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
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
        public void Insert(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var insertStatement = _sqlBuilder.ConstructFullInsertStatement();
            if (_sqlBuilder.RefreshOnInsertProperties.Length > 0)
            {
                var insertedEntity =
                    connection.Query<TEntity>(
                        insertStatement,
                        entity,
                        transaction: statementOptions.Transaction,
                        commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds).FirstOrDefault();

                // copy all the database generated props back onto our entity
                this.CopyEntity(insertedEntity, entity, _sqlBuilder.RefreshOnInsertProperties);
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
        public async Task InsertAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var insertStatement = _sqlBuilder.ConstructFullInsertStatement();
            if (_sqlBuilder.RefreshOnInsertProperties.Length > 0)
            {
                var insertedEntity =
                    (await
                     connection.QueryAsync<TEntity>(
                         insertStatement,
                         entity,
                         transaction: statementOptions.Transaction,
                         commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds)).FirstOrDefault();
                // copy all the database generated props back onto our entity
                this.CopyEntity(insertedEntity, entity, _sqlBuilder.RefreshOnInsertProperties);
            }
            else
            {
                await connection.ExecuteAsync(
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
        public bool UpdateById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var singleUpdateStatement = _sqlBuilder.ConstructFullSingleUpdateStatement();
            if (_sqlBuilder.RefreshOnUpdateProperties.Length > 0)
            {
                var updatedEntity = connection.Query<TEntity>(
                    singleUpdateStatement,
                    keyEntity,
                    transaction:
                        statementOptions.Transaction,
                    commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds).FirstOrDefault();

                if (updatedEntity != null)
                {
                    // copy all the database generated props back onto our entity
                    this.CopyEntity(updatedEntity, keyEntity, _sqlBuilder.RefreshOnUpdateProperties);
                }

                return updatedEntity != null;
            }

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
        public async Task<bool> UpdateByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var singleUpdateStatement = _sqlBuilder.ConstructFullSingleUpdateStatement();
            if (_sqlBuilder.RefreshOnUpdateProperties.Length > 0)
            {
                var updatedEntity = (await connection.QueryAsync<TEntity>(
                    singleUpdateStatement,
                    keyEntity,
                    transaction: statementOptions.Transaction,
                    commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds)).FirstOrDefault();

                if (updatedEntity != null)
                {
                    // copy all the database generated props back onto our entity
                    this.CopyEntity(updatedEntity, keyEntity, _sqlBuilder.RefreshOnUpdateProperties);
                }

                return updatedEntity != null;
            }

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
        public int BulkUpdate(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var batchUpdateStatement = _sqlBuilder.ConstructFullBatchUpdateStatement(statementOptions.WhereClause);
            var combinedParameters = new DynamicParameters();
            combinedParameters.AddDynamicParams(entity);
            if (statementOptions.Parameters != null)
            {
                combinedParameters.AddDynamicParams(statementOptions.Parameters);
            }
            return connection.Execute(
                batchUpdateStatement,
                combinedParameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs an UPDATE operation on multiple entities identified by an optional WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> BulkUpdateAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var batchUpdateStatement = _sqlBuilder.ConstructFullBatchUpdateStatement(statementOptions.WhereClause);
            var combinedParameters = new DynamicParameters();
            combinedParameters.AddDynamicParams(entity);
            if (statementOptions.Parameters != null)
            {
                combinedParameters.AddDynamicParams(statementOptions.Parameters);
            }
            return connection.ExecuteAsync(
                batchUpdateStatement,
                combinedParameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DeleteById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var singleDeleteStatement = _sqlBuilder.ConstructFullSingleDeleteStatement();
            return connection.Execute(
                singleDeleteStatement,
                keyEntity,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        /// Performs a DELETE operation on a single entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> DeleteByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions<TEntity> statementoptions)
        {
            var singleDeleteStatement = _sqlBuilder.ConstructFullSingleDeleteStatement();
            return await connection.ExecuteAsync(
                singleDeleteStatement,
                keyEntity,
                transaction: statementoptions.Transaction,
                commandTimeout: (int?)statementoptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BulkDelete(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var bulkDeleteStatement = _sqlBuilder.ConstructFullBatchDeleteStatement(statementOptions.WhereClause);
            return connection.Execute(
                bulkDeleteStatement,
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout:(int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> BulkDeleteAsync(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var bulkDeleteStatement = _sqlBuilder.ConstructFullBatchDeleteStatement(statementOptions.WhereClause);
            return connection.ExecuteAsync(
                bulkDeleteStatement,
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var countStatement = _sqlBuilder.ConstructFullCountStatement(statementOptions.WhereClause);
            return connection.ExecuteScalar<int>(
                countStatement,
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a COUNT on a range of items.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<int> CountAsync(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            var countStatement = _sqlBuilder.ConstructFullCountStatement(statementOptions.WhereClause);
            return connection.ExecuteScalarAsync<int>(
                countStatement,
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TEntity> BatchSelect(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            //validation removed, up to the engine to fail
            //Requires.Argument(
            //    (statementOptions.LimitResults==null && statementOptions.SkipResults==null)
            //    ||(statementOptions.OrderClause!=null),nameof(statementOptions), "When using Top or Skip, you must provide an OrderBy clause.");

            var selectStatement = _sqlBuilder.ConstructFullBatchSelectStatement(
                whereClause: statementOptions.WhereClause,
                orderClause: statementOptions.OrderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);
            return connection.Query<TEntity>(
                selectStatement,
                statementOptions.Parameters,
                buffered: !statementOptions.ForceStreamResults,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IEnumerable<TEntity>> BatchSelectAsync(IDbConnection connection, AggregatedSqlStatementOptions<TEntity> statementOptions)
        {
            //validation removed, let to the engine to fail
            //Requires.Argument(
            //    (statementOptions.LimitResults == null && statementOptions.SkipResults == null)
            //    || (statementOptions.OrderClause != null), nameof(statementOptions), "When using Top or Skip, you must provide an OrderBy clause.");

            var selectStatement = _sqlBuilder.ConstructFullBatchSelectStatement(
                whereClause: statementOptions.WhereClause,
                orderClause: statementOptions.OrderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);
            return connection.QueryAsync<TEntity>(
                selectStatement,
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CopyEntity(TEntity source, TEntity destination, PropertyRegistration[] properties)
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
