namespace Dapper.FastCrud.Providers
{
    using System;
    using System.Data;

    internal interface ISingleSelectEntityOperationDescriptor<TEntity>:IOperationDescriptor<TEntity>
    {
        TEntity Execute(
            IDbConnection connection,
            TEntity keyEntity,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null);
    }
}