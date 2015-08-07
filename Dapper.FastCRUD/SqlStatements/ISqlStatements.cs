namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Dapper.FastCrud.SqlBuilders;

    internal interface ISqlStatements
    {
        IStatementSqlBuilder SqlBuilder { get; }
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

        Task SingleInsertAsync(IDbConnection connection, TEntity entity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        Task<IEnumerable<TEntity>> BatchSelectAsync(
            IDbConnection connection,
            FormattableString whereClause = null,
            FormattableString orderClause = null,
            int? skipRowsCount = null,
            int? limitRowsCount = null,
            object queryParameters = null,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null);

        Task<bool> SingleDeleteAsync(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        Task<TEntity> SingleSelectAsync(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        Task<bool> SingleUpdateAsync(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);

        Task<int> CountAsync(IDbConnection connection, FormattableString whereClause = null, object queryParameters = null, TimeSpan? commandTimeout = null);

    }
}
