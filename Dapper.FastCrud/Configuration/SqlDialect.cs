// ReSharper disable once CheckNamespace (the namespace is intentionally not in sync with the file location)
namespace Dapper.FastCrud
{
    /// <summary>
    /// SQL dialect enumeration
    /// </summary>
    public enum SqlDialect
    {
        /// <summary>
        /// MS SQL Server 2012 and later
        /// </summary>
        MsSql,

        /// <summary>
        /// MS SQL Server 2008
        /// </summary>
        MsSql2008,

        /// <summary>
        /// MySql
        /// </summary>
        MySql,

        /// <summary>
        /// SQLite
        /// </summary>
        SqLite,

        /// <summary>
        /// PostgreSql
        /// </summary>
        PostgreSql
    }
}
