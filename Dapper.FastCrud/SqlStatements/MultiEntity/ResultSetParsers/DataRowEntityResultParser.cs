namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers
{
    using Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Containers;

    /// <summary>
    /// A result set parser made up of multiple multiple result sets
    /// </summary>
    internal class DataRowEntityResultParser : BasicResultSetParser
    {
        private readonly int _rowEntityIndex;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DataRowEntityResultParser(EntityContainer sharedContainer, int rowEntityIndex)
            : base(sharedContainer)
        {
            _rowEntityIndex = rowEntityIndex;
        }

        /// <summary>
        /// Executes the current stage and produces the next instance.
        /// </summary>
        protected override EntityInstanceWrapper ProduceNextInstance(EntityInstanceWrapper? _, EntityInstanceWrapper[] originalEntityRow)
        {
            var nextInstance = originalEntityRow[_rowEntityIndex];
            return nextInstance;
        }
    }
}
