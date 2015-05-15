namespace Dapper.FastCrud.Providers.MsSql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class BatchSelectEntityOperationDescriptor<TEntity> : EntityOperationDescriptor<EntityDescriptor<TEntity>,  TEntity>, IBatchSelectEntityOperationDescriptor<TEntity>
    {
        public BatchSelectEntityOperationDescriptor(EntityDescriptor<TEntity> entityDescriptor)
            : base(entityDescriptor)
        {
        }

        public IEnumerable<TEntity> Execute(
            IDbConnection connection,
            FormattableString whereClause,
            FormattableString orderClause,
            int? skipRowsCount,
            int? limitRowsCount,
            object queryParameters = null,
            bool streamResults = false,
            IDbTransaction transaction = null,
            TimeSpan? commandTimeout = null)
        {

            var sqlQueryBuilder = new StringBuilder();
            sqlQueryBuilder.AppendFormat(
                CultureInfo.InvariantCulture,
                "SELECT {0} FROM {1} ",
                this.EntityDescriptor.SelectPropertiesColumnQuery,
                this.EntityDescriptor.TableName);
            if (whereClause!=null)
            {
                sqlQueryBuilder.Append(" WHERE ");
                sqlQueryBuilder.Append(whereClause.ToString(CultureInfo.InvariantCulture));
            }
            if (orderClause != null)
            {
                sqlQueryBuilder.Append(" ORDER BY ");
                sqlQueryBuilder.Append(orderClause.ToString(CultureInfo.InvariantCulture));
            }
            if (skipRowsCount.HasValue)
            {
                sqlQueryBuilder.AppendFormat(CultureInfo.InvariantCulture, " OFFSET {0} ROWS", skipRowsCount);
            }
            if (limitRowsCount.HasValue)
            {
                sqlQueryBuilder.AppendFormat(CultureInfo.InvariantCulture, " FETCH NEXT {0} ROWS ONLY", limitRowsCount);
            }

            return connection.Query<TEntity>(
                sqlQueryBuilder.ToString(),
                queryParameters,
                buffered: !streamResults,
                transaction: transaction,
                commandTimeout: (int?)commandTimeout?.TotalSeconds);
        }
    }
}
