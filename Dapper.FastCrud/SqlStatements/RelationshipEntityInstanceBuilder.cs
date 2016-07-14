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
        private readonly ParticipatingEntityInstance[] _currentRowEntityInstances;
        private int _currentRowParticipatingEntityCount;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RelationshipEntityInstanceBuilder(params EntityMapping[] entityMappings)
        {
            _entityInstanceContainers = new Dictionary<Type, EntityInstanceContainer>();
            _currentRowEntityInstances = new ParticipatingEntityInstance[entityMappings.Length];
            _currentRowParticipatingEntityCount = 0;

            for(var entityMappingIndex = 0; entityMappingIndex<entityMappings.Length;entityMappingIndex++)
            {
                var entityMapping = entityMappings[entityMappingIndex];
                _entityInstanceContainers[entityMapping.EntityType] = new EntityInstanceContainer(entityMapping);

                // the entity row instances act as a placeholders, to avoid constructing them for every single row
                _currentRowEntityInstances[entityMappingIndex] = new ParticipatingEntityInstance();
            }
        }

        /// <summary>
        /// Registers a new entity instance for a row in the result set.
        /// Signal the end of the row in the result set with a call to <see cref="EndResultSetRow"/>.
        /// </summary>
        public void RegisterResultSetRowInstance<TEntity>(ref TEntity entity)
        {
            EntityInstanceContainer instanceContainer;
            object genericEntityInstance;
            bool requiresBinding;

            if (!_entityInstanceContainers.TryGetValue(typeof(TEntity), out instanceContainer))
            {
                throw new InvalidOperationException($"Type '{typeof(TEntity)}' could not be found in the list of registered instance containers.");
            }

            if (entity == null)
            {
                // we want to initialize the relationship, reason why for NULL values we'll pretend we haven't seen this instance
                genericEntityInstance = entity;
                requiresBinding = true;
            }
            else
            {
                var entityInstanceIdentity = new RelationshipEntityInstanceBuilderIdentity(instanceContainer.EntityMapping, entity);
                if (instanceContainer.KnownInstances.TryGetValue(entityInstanceIdentity, out genericEntityInstance))
                {
                    entity = (TEntity)genericEntityInstance;
                    requiresBinding = false;
                }
                else
                {
                    genericEntityInstance = entity;
                    instanceContainer.KnownInstances.Add(entityInstanceIdentity, genericEntityInstance);
                    requiresBinding = true;
                }
            }

            var entityInstanceRowDetails = _currentRowEntityInstances[_currentRowParticipatingEntityCount++];
            entityInstanceRowDetails.EntityMapping = instanceContainer.EntityMapping;
            entityInstanceRowDetails.Instance = genericEntityInstance;
            entityInstanceRowDetails.RequiresBinding = requiresBinding;
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

                    if (rowMainEntityInstance.RequiresBinding || rowChildEntityInstance.RequiresBinding)
                    {
                        Bind(rowMainEntityInstance.EntityMapping, rowMainEntityInstance.Instance, rowChildEntityInstance.EntityMapping, rowChildEntityInstance.Instance);
                        Bind(rowChildEntityInstance.EntityMapping, rowChildEntityInstance.Instance, rowMainEntityInstance.EntityMapping, rowMainEntityInstance.Instance);
                    }
                }
            }

            _currentRowParticipatingEntityCount = 0;
        }

        private static void Bind(EntityMapping mainEntityMapping, object mainEntity, EntityMapping childEntityMapping, object childEntity)
        {
            if (mainEntity == null)
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
                var childCollection = entityRelationship.ReferencingEntityProperty.GetValue(mainEntity);
                var childCollectionList = childCollection as IList;
                if (childCollectionList == null)
                {
                    childCollectionList = (IList)Activator.CreateInstance(_entityListType.MakeGenericType(childEntityType));
                    entityRelationship.ReferencingEntityProperty.SetValue(mainEntity, childCollectionList);
                }

                if (childEntity != null)
                {
                    childCollectionList.Add(childEntity);
                }
            }
        }

        private class EntityInstanceContainer
        {
            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public EntityInstanceContainer(EntityMapping entityMapping)
            {
                this.EntityMapping = entityMapping;
                this.KnownInstances = new Dictionary<RelationshipEntityInstanceBuilderIdentity, object>();
            }

            public EntityMapping EntityMapping { get; }
            public Dictionary<RelationshipEntityInstanceBuilderIdentity, object> KnownInstances { get; }
        }

        private class ParticipatingEntityInstance
        {
            public object Instance { get; set;  }
            public bool RequiresBinding { get; set; }
            public EntityMapping EntityMapping { get; set; }
        }
    }
}
