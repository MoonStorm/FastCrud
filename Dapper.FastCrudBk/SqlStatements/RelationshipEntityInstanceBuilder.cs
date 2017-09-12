namespace Dapper.FastCrud.SqlStatements
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Entity instance builder used in queries involving relationships.
    /// </summary>
    internal class RelationshipEntityInstanceBuilder
    {
        private static readonly Type _entityListType = typeof(List<>);
        private readonly Dictionary<Type, EntityInstanceContainer> _entityInstanceContainers;
        private readonly RelationshipEntityInstanceIdentity[] _currentRowEntityInstances;
        private int _currentRowParticipatingEntityCount;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RelationshipEntityInstanceBuilder(params EntityMapping[] entityMappings)
        {
            _entityInstanceContainers = new Dictionary<Type, EntityInstanceContainer>();
            _currentRowEntityInstances = new RelationshipEntityInstanceIdentity[entityMappings.Length];
            _currentRowParticipatingEntityCount = 0;

            for(var entityMappingIndex = 0; entityMappingIndex<entityMappings.Length;entityMappingIndex++)
            {
                var entityMapping = entityMappings[entityMappingIndex];
                _entityInstanceContainers[entityMapping.EntityType] = new EntityInstanceContainer(entityMapping);
            }
        }

        /// <summary>
        /// Registers a new entity instance for a row in the result set.
        /// Signal the end of the row in the result set with a call to <see cref="EndResultSetRow"/>.
        /// </summary>
        public RelationshipEntityInstanceIdentity<TEntity> RegisterResultSetRowInstance<TEntity>(TEntity entity)
        {
            EntityInstanceContainer instanceContainer;

            if (!_entityInstanceContainers.TryGetValue(typeof(TEntity), out instanceContainer))
            {
                throw new InvalidOperationException($"Type '{typeof(TEntity)}' could not be found in the list of registered instance containers.");
            }

            var instanceIdentity = new RelationshipEntityInstanceIdentity<TEntity>(instanceContainer.EntityMapping, entity);
            if (entity == null)
            {
                instanceIdentity.SetDuplicate(null);
            }
            else
            {
                object uniqueInstance;
                if (instanceContainer.KnownInstances.TryGetValue(instanceIdentity, out uniqueInstance))
                {
                    instanceIdentity.SetDuplicate(uniqueInstance);
                }
                else
                {
                    InitializeRelationships(instanceIdentity.EntityMapping, entity);
                    instanceContainer.KnownInstances.Add(instanceIdentity, entity);
                }
            }

            _currentRowEntityInstances[_currentRowParticipatingEntityCount++] = instanceIdentity;

            return instanceIdentity;
        }

        /// <summary>
        /// Ends a result row which has the effect of applying the necessary bindings.
        /// </summary>
        public void EndResultSetRow()
        {
            if (_currentRowParticipatingEntityCount != _currentRowEntityInstances.Length)
            {
                throw new InvalidOperationException("Invalid number of registered entity instances detected for a particular row in the result set");
            }

            for (var rowMainEntityIndex = 0; rowMainEntityIndex < _currentRowParticipatingEntityCount; rowMainEntityIndex++)
            {
                var rowMainEntityInstance = _currentRowEntityInstances[rowMainEntityIndex];

                for (var rowChildEntityIndex = rowMainEntityIndex + 1; rowChildEntityIndex < _currentRowParticipatingEntityCount; rowChildEntityIndex++)
                {
                    var rowChildEntityInstance = _currentRowEntityInstances[rowChildEntityIndex];

                    if (!rowMainEntityInstance.IsDuplicate || !rowChildEntityInstance.IsDuplicate)
                    {
                        Bind(rowMainEntityInstance.EntityMapping, rowMainEntityInstance.UniqueInstance, rowChildEntityInstance.EntityMapping, rowChildEntityInstance.UniqueInstance);
                        Bind(rowChildEntityInstance.EntityMapping, rowChildEntityInstance.UniqueInstance, rowMainEntityInstance.EntityMapping, rowMainEntityInstance.UniqueInstance);
                    }
                }
            }

            _currentRowParticipatingEntityCount = 0;
        }

        private static void Bind(EntityMapping mainEntityMapping, object mainEntity, EntityMapping childEntityMapping, object childEntity)
        {
            if (ReferenceEquals(mainEntity, null) || ReferenceEquals(childEntity, null))
            {
                return;
            }

            // find the property we're gonna use to attach the entity
            EntityMappingRelationship entityRelationship;
            var childEntityType = childEntityMapping.EntityType;
            if (mainEntityMapping.ChildParentRelationships.TryGetValue(childEntityType, out entityRelationship))
            {
                entityRelationship.ReferencingEntityProperty.SetValue(mainEntity, childEntity);
            }
            else if (mainEntityMapping.ParentChildRelationships.TryGetValue(childEntityType, out entityRelationship))
            {
                var childCollectionList = (IList)entityRelationship.ReferencingEntityProperty.GetValue(mainEntity);
                childCollectionList.Add(childEntity);
            }
        }

        private static void InitializeRelationships(EntityMapping entityMapping, object entity)
        {
            foreach (var parentChildRelationship in entityMapping.ParentChildRelationships)
            {
                var childCollectionList = (IList)Activator.CreateInstance(_entityListType.MakeGenericType(parentChildRelationship.Key));
                parentChildRelationship.Value.ReferencingEntityProperty.SetValue(entity, childCollectionList);
            }
        }

        private class EntityInstanceContainer
        {
            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public EntityInstanceContainer(EntityMapping entityMapping)
            {
                this.EntityMapping = entityMapping;
                this.KnownInstances = new Dictionary<RelationshipEntityInstanceIdentity, object>();
            }

            public EntityMapping EntityMapping { get; }
            public Dictionary<RelationshipEntityInstanceIdentity, object> KnownInstances { get; }
        }
    }
}
