namespace Dapper.FastCrud.Providers.MsSql
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class SingleSelectEntityOperationDescriptor<TEntity> : EntityOperationDescriptor<EntityDescriptor<TEntity>, TEntity>, ISingleSelectEntityOperationDescriptor<TEntity>
    {
        public SingleSelectEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
        }

        public TEntity Execute(
            IDbConnection connection,
            TEntity keyEntity,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null)
        {
            var sqlQuery = string.Format(
                CultureInfo.InvariantCulture,
                "SELECT {0} FROM {1} WHERE {2}",
                this.EntityDescriptor.SelectPropertiesColumnQuery,
                this.EntityDescriptor.TableName,
                this.EntityDescriptor.KeyPropertiesWhereClause);

            return connection.Query<TEntity>(
                sqlQuery,
                keyEntity,
                transaction: transaction,
                commandTimeout: (int?)commandTimeout?.TotalSeconds).SingleOrDefault();
        }
    }
}
