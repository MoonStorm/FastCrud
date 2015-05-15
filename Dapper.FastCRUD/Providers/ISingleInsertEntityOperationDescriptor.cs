namespace Dapper.FastCrud.Providers
{
    using System;
    using System.Data;

    internal interface ISingleInsertEntityOperationDescriptor<TEntity>:IOperationDescriptor<TEntity>
    {
        void Execute(IDbConnection connection, TEntity entity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null);
    }
}