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
        private readonly string _sqlQuery;

        public SingleUpdateEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
            _sqlQuery = string.Format(
                CultureInfo.InvariantCulture,
                "UPDATE {0} SET {1} WHERE {2}",
                this.EntityDescriptor.TableName,
                this.EntityDescriptor.UpdatePropertiesColumnSetterQuery,
                this.EntityDescriptor.KeyPropertiesWhereClause);
        }

        public bool Execute(IDbConnection connection, TEntity keyEntity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null)
        {
            return connection.Execute(_sqlQuery, keyEntity, transaction: transaction, commandTimeout: (int?)commandTimeout?.TotalSeconds) > 0;
        }
    }
}
