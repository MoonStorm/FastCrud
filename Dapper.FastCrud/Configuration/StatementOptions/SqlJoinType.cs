namespace Dapper.FastCrud.Configuration.StatementOptions
{
    /// <summary>
    /// Specifies the type of join
    /// </summary>
    public enum SqlJoinType
    {
        /// <summary>
        /// Not set
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Left outer join
        /// </summary>
        LeftOuterJoin,

        /// <summary>
        /// Inner join
        /// </summary>
        InnerJoin
    }
}
