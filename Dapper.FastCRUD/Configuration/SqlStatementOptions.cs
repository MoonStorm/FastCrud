namespace Dapper.FastCrud.Configuration
{
    using System;

    /// <summary>
    /// Stores basic command options.
    /// This class can be inherited and set as default at <see cref="OrmConfiguration.DefaultSqlStatementOptions"/>
    /// </summary>
    public class SqlStatementOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected internal SqlStatementOptions()
        {            
        }

        /// <summary>
        /// Gets a timeout for the command being executed.
        /// </summary>
        public TimeSpan? CommandTimeout
        {
            get; set;
        }
    }
}
