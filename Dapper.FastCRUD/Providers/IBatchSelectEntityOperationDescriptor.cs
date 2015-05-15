namespace Dapper.FastCrud.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    internal interface IBatchSelectEntityOperationDescriptor<TEntity>:IOperationDescriptor<TEntity>
    {
        IEnumerable<TEntity> Execute(
            IDbConnection connection,
            FormattableString whereClause,
            FormattableString orderClause,
            int? skipRowsCount,
            int? limitRowsCount,
            object queryParameters = null,
            bool streamResults = false,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null);
    }
}