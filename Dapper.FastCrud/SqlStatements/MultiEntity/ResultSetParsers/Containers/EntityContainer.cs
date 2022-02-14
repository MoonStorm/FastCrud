namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Containers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Global entity container
    /// </summary>
    internal class EntityContainer
    {
        private readonly Dictionary<Type, TypedEntityContainer> _entityContainers = new Dictionary<Type, TypedEntityContainer>();

        /// <summary>
        /// Gets or creates a new entity container.
        /// </summary>
        public TypedEntityContainer this[Type entityType]
        {
            get
            {
                if (!_entityContainers.TryGetValue(entityType, out TypedEntityContainer entityContainer))
                {
                    entityContainer = new TypedEntityContainer(entityType);
                    _entityContainers.Add(entityType, entityContainer);
                }

                return entityContainer;
            }
        }
    }
}
