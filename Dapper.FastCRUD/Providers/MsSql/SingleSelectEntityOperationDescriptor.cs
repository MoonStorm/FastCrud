namespace Dapper.FastCrud.Providers.MsSql
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
                this.EntityDescriptor.SelectPropertiesColumnQuery,
                this.EntityDescriptor.TableName,
                this.EntityDescriptor.KeyPropertiesWhereClause);
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
