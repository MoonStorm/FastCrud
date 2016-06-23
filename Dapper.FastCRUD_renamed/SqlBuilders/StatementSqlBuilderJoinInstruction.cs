namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using Dapper.FastCrud.Configuration.StatementOptions;

    /// <summary>
    /// Instructions for the statement sql builder for creating a join statement.
    /// </summary>
    internal class StatementSqlBuilderJoinInstruction
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StatementSqlBuilderJoinInstruction(
            GenericStatementSqlBuilder sqlBuilder, 
            SqlJoinType joinType,
            FormattableString whereClause, 
            FormattableString orderClause)
        {
            this.SqlBuilder = sqlBuilder;
            this.JoinType = joinType;
            this.WhereClause = whereClause;
            this.OrderClause = orderClause;
        }

        /// <summary>
        /// Gets or sets the SQL builder.
        /// </summary>
        public GenericStatementSqlBuilder SqlBuilder { get; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString WhereClause { get; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString OrderClause { get; }

        /// <summary>
        /// Gets or sets the join type.
        /// </summary>
        public SqlJoinType JoinType { get; }
    }
}
