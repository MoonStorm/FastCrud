namespace Dapper.FastCrud.Providers.MySql
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class SingleInsertEntityOperationDescriptor<TEntity> : EntityOperationDescriptor<EntityDescriptor<TEntity>, TEntity>, ISingleInsertEntityOperationDescriptor<TEntity>
    {
        private readonly string _sqlQuery;

        public SingleInsertEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendFormat(
                CultureInfo.InvariantCulture,
                "INSERT INTO {0} ({1}) VALUES ({2});",
                this.EntityDescriptor.TableName,
                string.Join(
                    ",", 
                    this.EntityDescriptor.InsertPropertyDescriptors.Select(propInfo => propInfo.Name)),
                string.Join(
                    ",",
                    this.EntityDescriptor.InsertPropertyDescriptors.Select(propInfo => string.Format(CultureInfo.InvariantCulture, "@{0}", propInfo.Name))));

            if (this.EntityDescriptor.DatabaseGeneratedIdentityPropertyDescriptors.Length > 0)
            {
                // we have an identity column, so we can fetch the rest of them
                if (this.EntityDescriptor.DatabaseGeneratedIdentityPropertyDescriptors.Length == 1
                    && this.EntityDescriptor.DatabaseGeneratedPropertyDescriptors.Length == 1)
                {
                    // just one, this is going to be easy
                    sqlBuilder.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "SELECT LAST_INSERT_ID() as {0}",
                        this.EntityDescriptor.DatabaseGeneratedPropertyDescriptors[0].Name);
                }
                else
                {
                    sqlBuilder.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "SELECT {0} FROM {1} WHERE {2} = LAST_INSERT_ID()",
                        string.Join(",", this.EntityDescriptor.DatabaseGeneratedPropertyDescriptors.Select(propInfo => propInfo.Name)),
                        this.EntityDescriptor.TableName,
                        this.EntityDescriptor.DatabaseGeneratedIdentityPropertyDescriptors[0].Name);
                }
            }
            _sqlQuery = sqlBuilder.ToString();
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
