namespace Dapper.FastCrud.Providers.MsSql
{
    using System;
    using System.Data;
    using System.Globalization;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class SingleUpdateEntityOperationDescriptor<TEntity> : EntityOperationDescriptor<EntityDescriptor<TEntity>, TEntity>, ISingleUpdateEntityOperationDescriptor<TEntity>
    {
        public SingleUpdateEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
        }

        public bool Execute(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null)
        {
            var sqlQuery = string.Format(
                CultureInfo.InvariantCulture,
                "UPDATE {0} SET {1} WHERE {2}",
                this.EntityDescriptor.TableName,
                this.EntityDescriptor.UpdatePropertiesColumnSetterQuery,
                this.EntityDescriptor.KeyPropertiesWhereClause);

            return connection.Execute(sqlQuery, keyEntity, transaction: transaction, commandTimeout: (int?)commandTimeout?.TotalSeconds) > 0;
        }
    }
}
