namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Dapper.FastCrud.SqlBuilders;

    internal interface ISqlStatements
    {
        ISqlBuilder SqlBuilder { get; }
    }

    internal interface ISqlStatements<TEntity>: ISqlStatements
    {
        void SingleInsert(IDbConnection connection, TEntity entity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        IEnumerable<TEntity> BatchSelect(
            IDbConnection connection,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null,
            bool streamResults = false,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null);

        bool SingleDelete(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        TEntity SingleSelect(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        bool SingleUpdate(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);
    }
}
