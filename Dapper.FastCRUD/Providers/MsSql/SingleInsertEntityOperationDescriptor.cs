namespace Dapper.FastCrud.Providers.MsSql
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
        public SingleInsertEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
        }

        public void Execute(IDbConnection connection, TEntity entity, IDbTransaction transaction = null, TimeSpan? commandTimeout = null)
        {
            var sqlQuery = string.Format(
                CultureInfo.InvariantCulture,
                "INSERT INTO {0} ({1}) OUTPUT {3} VALUES ({2}) ", 
                this.EntityDescriptor.TableName,
                this.EntityDescriptor.UpdatePropertiesColumnQuery,
                this.EntityDescriptor.UpdatePropertyValuesColumnQuery,
                this.EntityDescriptor.OutputDatabaseGeneratedPropertiesQuery);

            //var parameters = this.EntityDescriptor.CreateParameters(this.EntityDescriptor.UpdateablePropertyDescriptors, entity);
            //var insertedEntity = connection.Query<TEntity>(sqlQuery, parameters, transaction: transaction, commandTimeout: (int?)commandTimeout?.TotalSeconds).First();

            var insertedEntity = connection.Query<TEntity>(sqlQuery, entity, transaction: transaction, commandTimeout: (int?)commandTimeout?.TotalSeconds).First();

            // copy all the key properties back onto our entity
            foreach (var propDescriptor in this.EntityDescriptor.KeyPropertyDescriptors)
            {
                var updatedKeyValue = propDescriptor.GetValue(insertedEntity);
                propDescriptor.SetValue(entity, updatedKeyValue);
            };
        }
    }
}
