namespace Dapper.FastCrud.Providers.PostgreSql
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class SingleInsertEntityOperationDescriptor<TEntity> : EntityOperationDescriptor<EntityDescriptor<TEntity>, TEntity>, ISingleInsertEntityOperationDescriptor<TEntity>
    {
        private readonly string _sqlQuery;

        public SingleInsertEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {

            string outputQuery = this.EntityDescriptor.DatabaseGeneratedPropertyDescriptors.Length > 0
                                     ? string.Format(
                                         CultureInfo.InvariantCulture,
                                         "RETURNING {0}",
                                         string.Join(
                                             ",",
                                             this.EntityDescriptor.DatabaseGeneratedPropertyDescriptors.Select(
                                                 propInfo => propInfo.Name)))
                                     : string.Empty;

            _sqlQuery = string.Format(
                CultureInfo.InvariantCulture,
                "INSERT INTO {0} ({1}) VALUES ({2}) {3}",
                this.EntityDescriptor.TableName,
                string.Join(",", this.EntityDescriptor.InsertPropertyDescriptors.Select(propInfo => propInfo.Name)),
                string.Join(",", this.EntityDescriptor.InsertPropertyDescriptors.Select(propInfo => string.Format(CultureInfo.InvariantCulture, "@{0}", propInfo.Name))),
                outputQuery);
        }

        public void Execute(IDbConnection connection, TEntity entity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null)
        {
            var insertedEntity = connection.Query<TEntity>(_sqlQuery, entity, transaction: transaction, commandTimeout: (int?)commandTimeout?.TotalSeconds).FirstOrDefault();

            if (insertedEntity != null)
            {
                // copy all the key properties back onto our entity
                foreach (var propDescriptor in this.EntityDescriptor.DatabaseGeneratedPropertyDescriptors)
                {
                    var updatedKeyValue = propDescriptor.GetValue(insertedEntity);
                    propDescriptor.SetValue(entity, updatedKeyValue);
                }
            }
        }
    }
}
