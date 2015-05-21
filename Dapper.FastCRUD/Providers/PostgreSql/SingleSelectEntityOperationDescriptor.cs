namespace Dapper.FastCrud.Providers.PostgreSql
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
        private readonly string _sqlQuery;

        public SingleSelectEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
            _sqlQuery = string.Format(
                CultureInfo.InvariantCulture,
                "SELECT {0} FROM {1} WHERE {2}",
                string.Join(",", this.EntityDescriptor.SelectPropertyDescriptors.Select(propInfo => propInfo.Name)),
                this.EntityDescriptor.TableName,
                string.Join(
                    " and ",
                    this.EntityDescriptor.KeyPropertyDescriptors.Select(
                        (propInfo, index) => string.Format(CultureInfo.InvariantCulture, "{0}=@{1}", propInfo.Name, propInfo.Name))));
        }

        public TEntity Execute(
            IDbConnection connection,
            TEntity keyEntity,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null)
        {

            return connection.Query<TEntity>(
                _sqlQuery,
                keyEntity,
                transaction: transaction,
                commandTimeout: (int?)commandTimeout?.TotalSeconds).SingleOrDefault();
        }
    }
}
