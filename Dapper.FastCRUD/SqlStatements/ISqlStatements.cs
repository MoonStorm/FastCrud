namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Dapper.FastCrud.SqlBuilders;

    internal interface ISqlStatements
    {
        IStatementSqlBuilder StatementSqlBuilder { get; }
    }

    internal interface ISqlStatements<TEntity>: ISqlStatements
    {
        void SingleInsert(IDbConnection connection, TEntity entity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        IEnumerable<TEntity> BatchSelect(
            IDbConnection connection,
            FormattableString whereClause,
            FormattableString orderClause,
            int? skipRowsCount,
            int? limitRowsCount,
            object queryParameters,
            bool streamResults,
            IDbTransaction transaction,
            TimeSpan? commandTimeout);

        bool SingleDelete(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        TEntity SingleSelect(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        bool SingleUpdate(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);
    }
}
