// ReSharper disable once CheckNamespace (the namespace is intentionally not in sync with the file location) 
namespace Dapper.FastCrud
{
    /// <summary>
    /// Stores database options linked to a particular dialect.
    /// </summary>
    public class SqlDatabaseOptions
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SqlDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = string.Empty;
        }

        /// <summary>
        /// Gets the start delimiter used for SQL identifiers.
        /// </summary>
        public string StartDelimiter { get; protected set; }

        /// <summary>
        /// Gets the end delimiter used for SQL identifiers.
        /// </summary>
        public string EndDelimiter { get; protected set; }

        /// <summary>
        /// Gets a flag indicating the database is using schemas.
        /// </summary>
        public bool IsUsingSchemas { get; protected set; }
    }
}
