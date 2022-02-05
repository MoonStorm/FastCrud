namespace Dapper.FastCrud.SqlStatements.SingleEntity
{
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// Holds the main statement implementations for a single entity.
    /// </summary>
    internal class SingleEntitySqlStatements<TEntity>
    {
        private readonly GenericStatementSqlBuilder _sqlBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SingleEntitySqlStatements(GenericStatementSqlBuilder sqlBuilder)
        {
            _sqlBuilder = sqlBuilder;
        }

    }
}
