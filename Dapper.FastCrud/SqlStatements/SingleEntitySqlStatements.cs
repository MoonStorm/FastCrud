namespace Dapper.FastCrud.SqlStatements
{
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// SQL statements responsible for serving requests concerning a single entity.
    /// This class is sealed as it's being cached.
    /// Extending the functionality is done via implementing the <seealso cref="ISqlStatements"/> interface and proxying the calls to this class.
    /// <seealso cref="MultiEntitySqlStatements{TMainEntity}"/> uses this pattern for multi-entities statements.
    /// </summary>
    internal sealed class SingleEntitySqlStatements<TEntity>: ISqlStatements<TEntity>
    {
        private readonly GenericStatementSqlBuilder _sqlBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SingleEntitySqlStatements(GenericStatementSqlBuilder sqlBuilder)
        {
            Requires.NotNull(sqlBuilder, nameof(sqlBuilder));

            _sqlBuilder = sqlBuilder;
        }

        /// <summary>
        /// Gets the publicly accessible SQL builder.
        /// </summary>
        public GenericStatementSqlBuilder SqlBuilder => _sqlBuilder;

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        public TEntity SelectById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions)
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
        public async Task<TEntity> SelectByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions)
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
        public void Insert(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions)
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
        public async Task InsertAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions)
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
        /// Performs an UPDATE operation on an entity identified by its keys.
        /// </summary>
        public bool UpdateById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions)
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
        public async Task<bool> UpdateByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions)
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
        public int BulkUpdate(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions)
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
        public Task<int> BulkUpdateAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions)
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
        public bool DeleteById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions)
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
        public async Task<bool> DeleteByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementoptions)
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
        public int BulkDelete(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
        {
            var bulkDeleteStatement = _sqlBuilder.ConstructFullBatchDeleteStatement(statementOptions.WhereClause);
            return connection.Execute(
                bulkDeleteStatement,
                statementOptions.Parameters,
                transaction: statementOptions.Transaction,
                commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
        }

        /// <summary>
        /// Performs a DELETE operation using a WHERE clause.
        /// </summary>
        public Task<int> BulkDeleteAsync(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
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
        public int Count(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
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
        public Task<int> CountAsync(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
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
        public IEnumerable<TEntity> BatchSelect(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
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
        public Task<IEnumerable<TEntity>> BatchSelectAsync(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
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
