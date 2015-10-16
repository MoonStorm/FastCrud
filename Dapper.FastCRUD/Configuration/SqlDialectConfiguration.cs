namespace Dapper.FastCrud.Configuration
{
    /// <summary>
    /// Used for setting various database options.
    /// </summary>
    public class SqlDialectConfiguration
    {
        private string _identifierStartDelimiter;
        private string _identifierEndDelimiter;
        private bool _isUsingSchemas;

        /// <summary>
        /// Gets or sets the start delimiter used in properly formatting SQL identifiers.
        /// </summary>
        public string IdentifierStartDelimiter
        {
            get
            {
                return _identifierStartDelimiter;
            }
            set
            {
                 _identifierStartDelimiter = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the end delimiter used in properly formatting SQL identifiers.
        /// </summary>
        public string IdentifierEndDelimiter
        {
            get
            {
                return _identifierEndDelimiter;
            }
            set
            {
                _identifierEndDelimiter = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Use schemas in the generation of SQL statements.
        /// </summary>
        public bool IsUsingSchemas
        {
            get
            {
                return _isUsingSchemas;
            }
            set
            {
                _isUsingSchemas = value;
            }
        }

        /// <summary>
        /// Clones the current instance of dialect configuration.
        /// </summary>
        public SqlDialectConfiguration Clone()
        {
            return new SqlDialectConfiguration()
                       {
                           _identifierStartDelimiter = this._identifierStartDelimiter,
                           _identifierEndDelimiter = this._identifierEndDelimiter,
                           _isUsingSchemas = this._isUsingSchemas
                       };
        }
    }
}
