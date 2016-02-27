namespace Dapper.FastCrud.SqlBuilders.Relationships
{
    using System.Collections.Generic;

    /// <summary>
    /// SQL builder responsible for 
    /// </summary>
    internal class EntityRelationshipSqlBuilder
    {
        private LinkedList<GenericStatementSqlBuilder> _involvedEntitySqlBuilders;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityRelationshipSqlBuilder()
        {
            _involvedEntitySqlBuilders = new LinkedList<GenericStatementSqlBuilder>();
        }

        /// <summary>
        /// Registers a new sql builder associated with an entity involved in the relationship.  
        /// </summary>
        public void Add(GenericStatementSqlBuilder entitySqlBuilder)
        {
            
        }
    }
}
