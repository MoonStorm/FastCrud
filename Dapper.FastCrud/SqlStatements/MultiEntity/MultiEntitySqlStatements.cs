namespace Dapper.FastCrud.SqlStatements.MultiEntity
{
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// Holds the statement implementations for multiple entities.
    /// </summary>
    internal class MultiEntitySqlStatements<TEntity>
    {
        private readonly GenericStatementSqlBuilder _sqlBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultiEntitySqlStatements(GenericStatementSqlBuilder sqlBuilder)
        {
            _sqlBuilder = sqlBuilder;
        }

    }
}
