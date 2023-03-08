namespace Dapper.FastCrud.SqlStatements
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Dapper.FastCrud.Configuration.StatementOptions.Aggregated;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.SqlStatements.MultiEntity;
    using Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers;
    using Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Containers;
    using Dapper.FastCrud.Validations;
    using System;

    /// <summary>
    /// Holds the main statement implementations.
    /// </summary>
    internal class GenericSqlStatements<TEntity>: ISqlStatements<TEntity>
    {
        private readonly GenericStatementSqlBuilder _sqlBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GenericSqlStatements(GenericStatementSqlBuilder sqlBuilder)
        {
            Validate.NotNull(sqlBuilder, nameof(sqlBuilder));

            _sqlBuilder = sqlBuilder;
        }

        /// <summary>
        /// Gets the SQL builder.
        /// </summary>
        public GenericStatementSqlBuilder SqlBuilder => _sqlBuilder;

        /// <summary>
        /// Performs a SELECT operation on a single entity, using its keys
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEntity? SelectById(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions)
        {
            if (statementOptions.MainEntityAlias != null || statementOptions.Joins.Count > 0)
            {
                // this is no longer a simple entity selection, turn it into a full blown batch select
                statementOptions.WhereClause = $"{_sqlBuilder.ConstructKeysWhereClause(statementOptions.MainEntityAlias??statementOptions.EntityRegistration.TableName)}";
                statementOptions.Parameters = keyEntity;
                return this.BatchSelect(connection, statementOptions).SingleOrDefault();
            }

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
        public async Task<TEntity?> SelectByIdAsync(IDbConnection connection, TEntity keyEntity, AggregatedSqlStatementOptions statementOptions)
        {
            if (statementOptions.MainEntityAlias != null || statementOptions.Joins.Count > 0)
            {
                // this is no longer a simple entity selection, turn it into a full blown batch select
                statementOptions.WhereClause = $"{_sqlBuilder.ConstructKeysWhereClause(statementOptions.MainEntityAlias??statementOptions.EntityRegistration.TableName)}";
                statementOptions.Parameters = keyEntity;
                return (await this.BatchSelectAsync(connection, statementOptions)).SingleOrDefault();
            }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Performs an UPDATE opration on an entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                       singleUpdateStatement,
                       keyEntity,
                       transaction:
                       statementOptions.Transaction,
                       commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds) > 0;
        }

        /// <summary>
        /// Performs an UPDATE opration on an entity identified by its keys.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BulkUpdate(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions)
        {
            var whereClause = _sqlBuilder.ConstructWhereClause(statementOptions.StatementFormatter, statementOptions.WhereClause);
            var batchUpdateStatement = _sqlBuilder.ConstructFullBatchUpdateStatement(whereClause);
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
        public Task<int> BulkUpdateAsync(IDbConnection connection, TEntity entity, AggregatedSqlStatementOptions statementOptions)
        {
            var whereClause = _sqlBuilder.ConstructWhereClause(statementOptions.StatementFormatter, statementOptions.WhereClause);
            var batchUpdateStatement = _sqlBuilder.ConstructFullBatchUpdateStatement(whereClause);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BulkDelete(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
        {
            var whereClause = _sqlBuilder.ConstructWhereClause(statementOptions.StatementFormatter, statementOptions.WhereClause);
            var bulkDeleteStatement = _sqlBuilder.ConstructFullBatchDeleteStatement(whereClause);
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
        public Task<int> BulkDeleteAsync(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
        {
            var whereClause = _sqlBuilder.ConstructWhereClause(statementOptions.StatementFormatter, statementOptions.WhereClause);
            var bulkDeleteStatement = _sqlBuilder.ConstructFullBatchDeleteStatement(whereClause);
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
        public int Count(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
        {
            var joins = this.AnalyzeStatementJoins(statementOptions);
            var countStatement = (joins == null && statementOptions.WhereClause == null && statementOptions.MainEntityAlias == null)
                                     ? _sqlBuilder.ConstructFullCountStatement(null, null, false, null) // gain a small performance boost
                                     : _sqlBuilder.ConstructFullCountStatement(
                                         _sqlBuilder.ConstructFromClause(statementOptions.StatementFormatter, statementOptions.MainEntityAlias, joins),
                                         _sqlBuilder.ConstructWhereClause(statementOptions.StatementFormatter, statementOptions.WhereClause, joins),
                                         joins!=null,
                                         statementOptions.MainEntityAlias);
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
        public Task<int> CountAsync(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
        {
            var joins = this.AnalyzeStatementJoins(statementOptions);
            var countStatement = (joins == null && statementOptions.WhereClause == null && statementOptions.MainEntityAlias == null)
                                     ? _sqlBuilder.ConstructFullCountStatement(null, null, false, null) // gain a small performance boost
                                     : _sqlBuilder.ConstructFullCountStatement(
                                         _sqlBuilder.ConstructFromClause(statementOptions.StatementFormatter, statementOptions.MainEntityAlias, joins),
                                         _sqlBuilder.ConstructWhereClause(statementOptions.StatementFormatter, statementOptions.WhereClause, joins),
                                         joins != null,
                                         statementOptions.MainEntityAlias);

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
        public IEnumerable<TEntity> BatchSelect(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
        {
            //validation removed, up to the engine to fail
            //Requires.Argument(
            //    (statementOptions.LimitResults==null && statementOptions.SkipResults==null)
            //    ||(statementOptions.OrderClause!=null),nameof(statementOptions), "When using Top or Skip, you must provide an OrderBy clause.");

            var joins = this.AnalyzeStatementJoins(statementOptions);
            var selectClause = _sqlBuilder.ConstructColumnEnumerationForSelect(statementOptions.MainEntityAlias, joins);
            var fromClause = _sqlBuilder.ConstructFromClause(statementOptions.StatementFormatter, statementOptions.MainEntityAlias, joins);
            var whereClause = _sqlBuilder.ConstructWhereClause(statementOptions.StatementFormatter, statementOptions.WhereClause, joins);
            var orderClause = _sqlBuilder.ConstructOrderClause(statementOptions.StatementFormatter, statementOptions.OrderClause, joins);
            IEnumerable<TEntity> results;

            var selectStatement = _sqlBuilder.ConstructFullBatchSelectStatement(
                selectClause: selectClause,
                fromClause: fromClause,
                whereClause: whereClause,
                orderClause: orderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);

             if (joins == null)
            {
                results = connection.Query<TEntity>(
                    selectStatement,
                    statementOptions.Parameters,
                    buffered: !statementOptions.ForceStreamResults,
                    transaction: statementOptions.Transaction,
                    commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);

            }
            else
            {
                var joinsWithResultSetMappings = joins?.Where(join => join.RequiresResultMapping).ToArray() ?? Array.Empty<SqlStatementJoin>();
                var splitOn = _sqlBuilder.ConstructSplitOnExpression(joinsWithResultSetMappings);
                var mainEntityJoinParser = new MainEntityResultSetParser<TEntity>(joinsWithResultSetMappings);
                var rowInstanceWrappers = new EntityInstanceWrapper[joinsWithResultSetMappings.Length + 1];

                connection.Query<MainEntityResultSetParser<TEntity>>(
                    selectStatement,
                    new[]{statementOptions.EntityRegistration.EntityType}
                        .Concat(joinsWithResultSetMappings.Select(join => join.ReferencedEntityRegistration.EntityType))
                        .ToArray(),
                    (object[] rowInstances) =>
                    {
                        for (var rowInstanceIndex = 0; rowInstanceIndex < rowInstances.Length; rowInstanceIndex++)
                        {
                            var rowInstanceRegistration = rowInstanceIndex == 0
                                                              ? statementOptions.EntityRegistration
                                                              : joinsWithResultSetMappings[rowInstanceIndex - 1].ReferencedEntityRegistration;
                            rowInstanceWrappers[rowInstanceIndex] = new EntityInstanceWrapper(rowInstanceRegistration, rowInstances[rowInstanceIndex]);
                        }
                        mainEntityJoinParser.Execute(null, rowInstanceWrappers);
                        return mainEntityJoinParser;
                    },
                    statementOptions.Parameters,
                    buffered: true, //TODO: look into allowing streaming results with this method
                    transaction: statementOptions.Transaction,
                    commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds,
                    splitOn: splitOn);

                    return mainEntityJoinParser.MainEntityCollection;
            }

            return results;
        }

        /// <summary>
        /// Performs a common SELECT 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IEnumerable<TEntity>> BatchSelectAsync(IDbConnection connection, AggregatedSqlStatementOptions statementOptions)
        {
            //validation removed, up to the engine to fail
            //Requires.Argument(
            //    (statementOptions.LimitResults==null && statementOptions.SkipResults==null)
            //    ||(statementOptions.OrderClause!=null),nameof(statementOptions), "When using Top or Skip, you must provide an OrderBy clause.");

            var joins = this.AnalyzeStatementJoins(statementOptions);
            var selectClause = _sqlBuilder.ConstructColumnEnumerationForSelect(statementOptions.MainEntityAlias, joins);
            var fromClause = _sqlBuilder.ConstructFromClause(statementOptions.StatementFormatter, statementOptions.MainEntityAlias, joins);
            var whereClause = _sqlBuilder.ConstructWhereClause(statementOptions.StatementFormatter, statementOptions.WhereClause, joins);
            var orderClause = _sqlBuilder.ConstructOrderClause(statementOptions.StatementFormatter, statementOptions.OrderClause, joins);
            IEnumerable<TEntity> results;

            var selectStatement = _sqlBuilder.ConstructFullBatchSelectStatement(
                selectClause: selectClause,
                fromClause: fromClause,
                whereClause: whereClause,
                orderClause: orderClause,
                skipRowsCount: statementOptions.SkipResults,
                limitRowsCount: statementOptions.LimitResults);

            if (joins == null)
            {
                results = await connection.QueryAsync<TEntity>(
                    selectStatement,
                    statementOptions.Parameters,
                    transaction: statementOptions.Transaction,
                    commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds);
            }
            else
            {
                var joinsWithResultSetMappings = joins?.Where(join => join.RequiresResultMapping).ToArray() ?? Array.Empty<SqlStatementJoin>();
                var splitOn = _sqlBuilder.ConstructSplitOnExpression(joinsWithResultSetMappings);
                var mainEntityJoinParser = new MainEntityResultSetParser<TEntity>(joinsWithResultSetMappings);
                var rowInstanceWrappers = new EntityInstanceWrapper[joinsWithResultSetMappings.Length + 1];

                await connection.QueryAsync<MainEntityResultSetParser<TEntity>>(
                    selectStatement,
                    new[] { statementOptions.EntityRegistration.EntityType }
                        .Concat(joinsWithResultSetMappings.Select(join => join.ReferencedEntityRegistration.EntityType))
                        .ToArray(),
                    (object[] rowInstances) =>
                    {
                        for (var rowInstanceIndex = 0; rowInstanceIndex < rowInstances.Length; rowInstanceIndex++)
                        {
                            var rowInstanceRegistration = rowInstanceIndex == 0
                                                              ? statementOptions.EntityRegistration
                                                              : joinsWithResultSetMappings[rowInstanceIndex - 1].ReferencedEntityRegistration;
                            rowInstanceWrappers[rowInstanceIndex] = new EntityInstanceWrapper(rowInstanceRegistration, rowInstances[rowInstanceIndex]);
                        }
                        mainEntityJoinParser.Execute(null, rowInstanceWrappers);
                        return mainEntityJoinParser;
                    },
                    statementOptions.Parameters,
                    transaction: statementOptions.Transaction,
                    commandTimeout: (int?)statementOptions.CommandTimeout?.TotalSeconds,
                    splitOn: splitOn);

                return mainEntityJoinParser.MainEntityCollection;
            }

            return results;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SqlStatementJoin[]? AnalyzeStatementJoins(AggregatedSqlStatementOptions statementOptions)
        {
            if (statementOptions?.Joins?.Count > 0)
            {
                return statementOptions.Joins.Select(joinOptions => new SqlStatementJoin(statementOptions, joinOptions)).ToArray();
            }

            return null;
        }

    }
}
