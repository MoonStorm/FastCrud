namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using System.Data;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// SQL statement options.
    /// </summary>
    internal interface ISqlStatementOptionsGetter
    {
        /// <summary>
        /// The transaction to be used by the statement.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// The entity mapping override to be used for the main entity.
        /// </summary>
        EntityMapping EntityMappingOverride { get; }

        /// <summary>
        /// Gets a timeout for the command being executed.
        /// </summary>
        TimeSpan? CommandTimeout { get; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        FormattableString WhereClause { get; }

        /// <summary>
        /// Parameters used by the statement.
        /// </summary>
        object Parameters { get; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        FormattableString OrderClause { get; set; }

        /// <summary>
        /// Gets or sets a limit on the number of rows returned.
        /// </summary>
        long? LimitResults { get; set; }

        /// <summary>
        /// Gets or sets a number of rows to be skipped.
        /// </summary>
        long? SkipResults { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating the results should be streamed.
        /// </summary>
        bool ForceStreamResults { get; set; }
    }
}
