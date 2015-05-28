namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.SqlBuilders;

    internal class GenericSqlStatements<TEntity>: ISqlStatements<TEntity>
    {
        private readonly IStatementSqlBuilder _sqlBuilder;
        private readonly string _singleInsertSql;
        private readonly string _singleUpdateSql;
        private readonly string _singleDeleteSql;
        private readonly string _singleSelectSql;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GenericSqlStatements(IStatementSqlBuilder sqlBuilder)
        {
            _sqlBuilder = sqlBuilder;
            _singleInsertSql = sqlBuilder.ConstructFullInsertStatement();
            _singleUpdateSql = sqlBuilder.ConstructFullUpdateStatement();
            _singleDeleteSql = sqlBuilder.ConstructFullDeleteStatement();
            _singleSelectSql = sqlBuilder.ConstructFullSingleSelectStatement();
        }

        public ISqlBuilder SqlBuilder
        {
            get
            {
                return this._sqlBuilder;
            }
        }

        public void SingleInsert(IDbConnection connection, TEntity entity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null)
        {
            var insertedEntity = connection.Query<TEntity>(this._singleInsertSql, entity, transaction: transaction, commandTimeout: (int?)commandTimeout?.TotalSeconds).FirstOrDefault();

            if (insertedEntity != null)
            {
                // copy all the key properties back onto our entity
                foreach (var propMapping in _sqlBuilder.DatabaseGeneratedProperties)
                {
                    var propDescriptor = propMapping.Descriptor;
                    var updatedKeyValue = propDescriptor.GetValue(insertedEntity);
                    propDescriptor.SetValue(entity, updatedKeyValue);
                }
            }
        }

        public IEnumerable<TEntity> BatchSelect(
            IDbConnection connection,
            FormattableString whereClause,
            FormattableString orderClause,
            int? skipRowsCount,
            int? limitRowsCount,
            object queryParameters,
            bool streamResults,
            IDbTransaction transaction,
            TimeSpan? commandTimeout)
        {
            return connection.Query<TEntity>(
                _sqlBuilder.ConstructFullBatchSelectStatement(
                    whereClause:whereClause,
                    orderClause:orderClause,
                    skipRowsCount:skipRowsCount,
                    limitRowsCount:limitRowsCount),
                queryParameters,
                buffered: !streamResults,
                transaction: transaction,
                commandTimeout: (int?)commandTimeout?.TotalSeconds);
        }

        public bool SingleDelete(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null)
        {
            return connection.Execute(
                _singleDeleteSql,
                keyEntity,
                transaction: transaction,
                commandTimeout: (int?)commandTimeout?.TotalSeconds) > 0;
        }

        public TEntity SingleSelect(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null)
        {
            return connection.Query<TEntity>(
                _singleSelectSql,
                keyEntity,
                transaction: transaction,
                commandTimeout: (int?)commandTimeout?.TotalSeconds).SingleOrDefault();
        }

        public bool SingleUpdate(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null)
        {
            return connection.Execute(_singleUpdateSql, keyEntity, transaction: transaction, commandTimeout: (int?)commandTimeout?.TotalSeconds) > 0;
        }
    }
}
