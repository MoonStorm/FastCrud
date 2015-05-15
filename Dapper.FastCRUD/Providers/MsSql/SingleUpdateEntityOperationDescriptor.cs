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
                "UPDATE {0} ({1}) VALUES ({2}) WHERE {3}",
                this.EntityDescriptor.TableName,
                this.EntityDescriptor.UpdatePropertiesColumnQuery,
                this.EntityDescriptor.UpdatePropertyValuesColumnQuery,
                this.EntityDescriptor.KeyPropertiesWhereClause);

            return connection.Execute(sqlQuery, keyEntity, transaction: transaction, commandTimeout: (int?)commandTimeout?.TotalSeconds) > 0;
        }
    }
}
