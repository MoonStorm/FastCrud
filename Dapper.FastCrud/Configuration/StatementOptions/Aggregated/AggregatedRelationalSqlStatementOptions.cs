namespace Dapper.FastCrud.Configuration.StatementOptions.Aggregated
{
    using System;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Groups together all the statement options related to a joined entity.
    /// </summary>
    internal abstract class AggregatedRelationalSqlStatementOptions
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        public AggregatedRelationalSqlStatementOptions()
        {
            this.JoinType = SqlJoinType.NotSpecified;
        }

        /// <summary>
        /// The entity mapping override to be used for the joined entity.
        /// </summary>
        public EntityMapping EntityMappingOverride { get; protected set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString WhereClause { get; protected set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString OrderClause { get; protected set; }

        /// <summary>
        /// Gets or sets the SQL join type.
        /// </summary>
        public SqlJoinType JoinType { get; protected set; }
    }
}