namespace Dapper.FastCrud.Providers.MsSql
{
    using System.Globalization;
    using System.Linq;

    internal class EntityDescriptor<TEntity>: FastCrud.EntityDescriptor<TEntity>
    {
        private string _tableName;

        public EntityDescriptor()
        {
            this.SelectPropertiesColumnQuery = string.Join(",", this.SelectPropertyDescriptors.Select(propInfo => propInfo.Name));
            this.OutputInsertedKeyPropertiesQuery = string.Join(",",
                this.KeyPropertyDescriptors.Select(
                    propInfo => string.Format(CultureInfo.InvariantCulture, "inserted.{0}", propInfo.Name)));
            this.OutputDeletedKeyPropertiesQuery = string.Join(",",
                this.KeyPropertyDescriptors.Select(
                    propInfo => string.Format(CultureInfo.InvariantCulture, "deleted.{0}", propInfo.Name)));
            this.UpdatePropertiesColumnQuery = string.Join(",",
                this.UpdateablePropertyDescriptors.Select(propInfo => propInfo.Name));
            this.UpdatePropertyValuesColumnQuery = string.Join(",",
                this.UpdateablePropertyDescriptors.Select(
                    propInfo => string.Format(CultureInfo.InvariantCulture, "@{0}", propInfo.Name)));
            this.KeyPropertiesColumnQuery = string.Join(",", this.SelectPropertyDescriptors.Select(propInfo => propInfo.Name));
            this.KeyPropertiesWhereClause = string.Join(
                " and ",
                this.KeyPropertyDescriptors.Select(
                    (propInfo, index) =>
                        string.Format(CultureInfo.InvariantCulture, "{0} = @{1}", propInfo.Name, propInfo.Name)));

            this.RegisterOperation<ISingleDeleteEntityOperationDescriptor<TEntity>>(new SingleDeleteEntityOperationDescriptor<TEntity>(this));
            this.RegisterOperation<ISingleInsertEntityOperationDescriptor<TEntity>>(new SingleInsertEntityOperationDescriptor<TEntity>(this));
            this.RegisterOperation<ISingleUpdateEntityOperationDescriptor<TEntity>>(new SingleUpdateEntityOperationDescriptor<TEntity>(this));
            this.RegisterOperation<IBatchSelectEntityOperationDescriptor<TEntity>>(new BatchSelectEntityOperationDescriptor<TEntity>(this));

            if (string.IsNullOrEmpty(this.TableDescriptor.Schema))
            {
                this._tableName = string.Format(CultureInfo.InvariantCulture, "[{0}]", this.TableDescriptor.Name);
            }
            else
            {
                this._tableName = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", this.TableDescriptor.Schema, this.TableDescriptor.Name);
            }

        }

        public string SelectPropertiesColumnQuery { get; private set; }
        public string KeyPropertiesWhereClause { get; private set; }
        public string KeyPropertiesColumnQuery { get; private set; }
        public string UpdatePropertyValuesColumnQuery { get; private set; }
        public string UpdatePropertiesColumnQuery { get; private set; }
        public string OutputDeletedKeyPropertiesQuery { get; private set; }
        public string OutputInsertedKeyPropertiesQuery { get; private set; }

        public override string TableName
        {
            get
            {
                return _tableName;
            }
        }
    }
}
