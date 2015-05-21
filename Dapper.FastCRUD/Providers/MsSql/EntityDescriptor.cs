namespace Dapper.FastCrud.Providers.MsSql
{
    using System.Globalization;
    using System.Linq;

    internal class EntityDescriptor<TEntity>: FastCrud.EntityDescriptor<TEntity>
    {
        private readonly string _tableName;

        public EntityDescriptor()
        {
            if (string.IsNullOrEmpty(this.TableDescriptor.Schema))
            {
                this._tableName = string.Format(CultureInfo.InvariantCulture, "[{0}]", this.TableDescriptor.Name);
            }
            else
            {
                this._tableName = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", this.TableDescriptor.Schema, this.TableDescriptor.Name);
            }

            this.SingleDeleteOperation = new SingleDeleteEntityOperationDescriptor<TEntity>(this);
            this.SingleInsertOperation = new SingleInsertEntityOperationDescriptor<TEntity>(this);
            this.SingleUpdateOperation = new SingleUpdateEntityOperationDescriptor<TEntity>(this);
            this.BatchSelectOperation = new BatchSelectEntityOperationDescriptor<TEntity>(this);
            this.SingleSelectOperation = new SingleSelectEntityOperationDescriptor<TEntity>(this);        
        }

        public override string TableName
        {
            get
            {
                return _tableName;
            }
        }
    }
}
