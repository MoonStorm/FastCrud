namespace Dapper.FastCrud.Configuration
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
            // The ANSI standard delimeter is a double quote mark
            this.StartDelimiter = this.EndDelimiter = "\"";
            this.ParameterPrefix = "@";
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

        /// <summary>
        /// Gets the prefix used for named parameters
        /// </summary>
        public string ParameterPrefix { get; protected set; }
    }
}
