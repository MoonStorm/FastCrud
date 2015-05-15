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
            this.OutputDatabaseGeneratedPropertiesQuery = string.Join(",",
                this.DatabaseGeneratedPropertyDescriptors.Select(
                    propInfo => string.Format(CultureInfo.InvariantCulture, "inserted.{0}", propInfo.Name)));
            this.OutputDeletedKeyPropertiesQuery = string.Join(",",
                this.KeyPropertyDescriptors.Select(
                    propInfo => string.Format(CultureInfo.InvariantCulture, "deleted.{0}", propInfo.Name)));
            this.UpdatePropertiesColumnQuery = string.Join(",",
                this.UpdateablePropertyDescriptors.Select(propInfo => propInfo.Name));
            UpdatePropertiesColumnSetterQuery = string.Join(",",
                this.UpdateablePropertyDescriptors.Select(propInfo => string.Format(CultureInfo.InvariantCulture, "{0}=@{0}", propInfo.Name)));
            this.UpdatePropertyValuesColumnQuery = string.Join(",",
                this.UpdateablePropertyDescriptors.Select(
                    propInfo => string.Format(CultureInfo.InvariantCulture, "@{0}", propInfo.Name)));
            this.KeyPropertiesColumnQuery = string.Join(",", this.SelectPropertyDescriptors.Select(propInfo => propInfo.Name));
            this.KeyPropertiesWhereClause = string.Join(
                " and ",
                this.KeyPropertyDescriptors.Select(
                    (propInfo, index) =>
                        string.Format(CultureInfo.InvariantCulture, "{0}=@{1}", propInfo.Name, propInfo.Name)));

            this.SingleDeleteOperation = new SingleDeleteEntityOperationDescriptor<TEntity>(this);
            this.SingleInsertOperation = new SingleInsertEntityOperationDescriptor<TEntity>(this);
            this.SingleUpdateOperation = new SingleUpdateEntityOperationDescriptor<TEntity>(this);
            this.BatchSelectOperation = new BatchSelectEntityOperationDescriptor<TEntity>(this);
            this.SingleSelectOperation = new SingleSelectEntityOperationDescriptor<TEntity>(this);

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
        public string UpdatePropertiesColumnSetterQuery { get; private set; }
        public string OutputDeletedKeyPropertiesQuery { get; private set; }
        public string OutputDatabaseGeneratedPropertiesQuery { get; private set; }

        public override string TableName
        {
            get
            {
                return _tableName;
            }
        }
    }
}
