namespace Dapper.FastCrud.Providers.PostgreSql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class BatchSelectEntityOperationDescriptor<TEntity> : EntityOperationDescriptor<EntityDescriptor<TEntity>,  TEntity>, IBatchSelectEntityOperationDescriptor<TEntity>
    {
        private string _baseSql;

        public BatchSelectEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
            _baseSql = string.Format(
                CultureInfo.InvariantCulture,
                "SELECT {0} FROM {1}",
                string.Join(",", this.EntityDescriptor.SelectPropertyDescriptors.Select(propInfo => propInfo.Name)),
                this.EntityDescriptor.TableName);

        }

        public IEnumerable<TEntity> Execute(
            IDbConnection connection,
            FormattableString whereClause,
            FormattableString orderClause,
            int? skipRowsCount,
            int? limitRowsCount,
            object queryParameters,
            bool streamResults,
            IDbTransaction transaction,
            TimeSpan? commandTimeout)
        {
            var sql = _baseSql;
            if (whereClause != null)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " WHERE {0}", whereClause);
            }
            if (orderClause != null)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " ORDER BY {0}", orderClause);
            }
            if (limitRowsCount.HasValue)
            {
                sql += string.Format(CultureInfo.InvariantCulture, " LIMIT {0}", limitRowsCount);
            }
            if (skipRowsCount.HasValue)
            {
                sql+=string.Format(CultureInfo.InvariantCulture, " OFFSET {0}", skipRowsCount);
            }

            return connection.Query<TEntity>(
                sql,
                queryParameters,
                buffered: !streamResults,
                transaction: transaction,
                commandTimeout: (int?)commandTimeout?.TotalSeconds);
        }
    }
}
