namespace Dapper.FastCrud.Providers.MsSql
{
    using System;
    using System.Data;
    using System.Globalization;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class SingleDeleteEntityOperationDescriptor<TEntity> : EntityOperationDescriptor<EntityDescriptor<TEntity>, TEntity>, ISingleDeleteEntityOperationDescriptor<TEntity>
    {
        public SingleDeleteEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
        }

        public bool Execute(
            IDbConnection connection,
            TEntity keyEntity,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null)
        {
            var sqlQuery = string.Format(
                CultureInfo.InvariantCulture,
                "DELETE FROM {0} WHERE {1}",
                this.EntityDescriptor.TableName,
                this.EntityDescriptor.KeyPropertiesWhereClause);

            return connection.Execute(
                sqlQuery,
                keyEntity,
                transaction: transaction,
                commandTimeout: (int?)commandTimeout?.TotalSeconds) > 0;
        }
    }
}
