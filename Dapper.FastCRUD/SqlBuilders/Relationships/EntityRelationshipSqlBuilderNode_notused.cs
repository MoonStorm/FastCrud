namespace Dapper.FastCrud.SqlBuilders.Relationships
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Stores a hierarchy of parent-child relationship sql builders.
    /// </summary>
    internal class EntityRelationshipSqlBuilderNode
    {
        private GenericStatementSqlBuilder _sqlBuilder;
        private EntityRelationshipSqlBuilderNode _parentNode;
        private LinkedList<EntityRelationshipSqlBuilderNode> _childNodes;
        private bool _isIncluded;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityRelationshipSqlBuilderNode(GenericStatementSqlBuilder sqlBuilder, bool isIncluded)
        {
            _sqlBuilder = sqlBuilder;
            _childNodes = new LinkedList<EntityRelationshipSqlBuilderNode>();
            _isIncluded = isIncluded;
        }

        public EntityRelationshipSqlBuilderNode Add(GenericStatementSqlBuilder sqlBuilder)
        {
            if (sqlBuilder.EntityMapping.EntityType == _sqlBuilder.EntityMapping.EntityType)
            {
                _sqlBuilder = sqlBuilder;
                _linkOnly = false;
                return this;
            }
        }

        private EntityRelationshipSqlBuilderNode Add(Type newEntityType, Func<GenericStatementSqlBuilder> newSqlBuilderFunc, bool isIncluded)
        {
            if (newEntityType == _sqlBuilder.EntityMapping.EntityType)
            {
                if (isIncluded)
                {
                    _sqlBuilder = newSqlBuilderFunc();
                    _isIncluded = true;
                }
                return this;
            }

            // check for parents
            var newSqlBuilder = newSqlBuilderFunc();
            foreach (var parentEntityType in newSqlBuilder.EntityMapping.ParentChildRelationships.Keys)
            {
                var 
            }
        }
    }
}
