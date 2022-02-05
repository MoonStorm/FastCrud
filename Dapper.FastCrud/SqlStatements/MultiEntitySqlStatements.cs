namespace Dapper.FastCrud.SqlStatements
{
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// SQL statements responsible for serving requests concerning a single main entity JOINed with several others.
    /// </summary>
    internal class MultiEntitySqlStatements<TMainEntity>
        : ISqlStatements<TMainEntity>
    {
        private readonly ISqlStatements<TMainEntity> _mainEntitySqlStatements;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultiEntitySqlStatements(ISqlStatements<TMainEntity> mainEntitySqlStatements)
        {
            Requires.NotNull(mainEntitySqlStatements, nameof(mainEntitySqlStatements));

            _mainEntitySqlStatements = mainEntitySqlStatements;
        }
    }
}
