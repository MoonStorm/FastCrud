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
        private readonly Dictionary<Type, EntityInstanceContainer> _entityInstanceContainers = new Dictionary<Type, EntityInstanceContainer>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RelationshipEntityInstanceBuilder(params EntityMapping[] entityMappings)
        {
            foreach (var entityMapping in entityMappings)
            {
                _entityInstanceContainers[entityMapping.EntityType] = new EntityInstanceContainer(entityMapping);
            }
        }

        /// <summary>
        /// Registers two related entity instances.
        /// </summary>
        public void Add<TMainEntity, TFirstJoinedEntity>(ref TMainEntity mainEntity, ref TFirstJoinedEntity firstJoinedEntity)
        {
            EntityInstanceContainer mainEntityInstanceContainer;
            var isMainEntityNew = this.Add(ref mainEntity, out mainEntityInstanceContainer);

            EntityInstanceContainer firstJoinedEntityContainer;
            var isFirstJoinedEntityNew = this.Add(ref firstJoinedEntity, out firstJoinedEntityContainer);

            if (isMainEntityNew || isFirstJoinedEntityNew)
            {
                Attach(mainEntityInstanceContainer.EntityMapping, mainEntity, firstJoinedEntityContainer.EntityMapping, firstJoinedEntity);
                Attach(firstJoinedEntityContainer.EntityMapping, firstJoinedEntity, mainEntityInstanceContainer.EntityMapping, mainEntity);
            }
        }

        /// <summary>
        /// Registers two related entity instances.
        /// </summary>
        public void Add<TMainEntity, TFirstJoinedEntity, TSecondJoinedEntity>(ref TMainEntity mainEntity, ref TFirstJoinedEntity firstJoinedEntity, ref TSecondJoinedEntity secondJoinedEntity)
        {
            EntityInstanceContainer mainEntityInstanceContainer;
            var isMainEntityNew = this.Add(ref mainEntity, out mainEntityInstanceContainer);

            EntityInstanceContainer firstJoinedEntityContainer;
            var isFirstJoinedEntityNew = this.Add(ref firstJoinedEntity, out firstJoinedEntityContainer);

            EntityInstanceContainer secondJoinedEntityContainer;
            var isSecondJoinedEntityNew = this.Add(ref secondJoinedEntity, out secondJoinedEntityContainer);

            if (isMainEntityNew || isFirstJoinedEntityNew)
            {
                Attach(mainEntityInstanceContainer.EntityMapping, mainEntity, firstJoinedEntityContainer.EntityMapping, firstJoinedEntity);
                Attach(firstJoinedEntityContainer.EntityMapping, firstJoinedEntity, mainEntityInstanceContainer.EntityMapping, mainEntity);
            }

            if (isFirstJoinedEntityNew || isSecondJoinedEntityNew)
            {
                Attach(firstJoinedEntityContainer.EntityMapping, firstJoinedEntity, secondJoinedEntityContainer.EntityMapping, secondJoinedEntity);
                Attach(secondJoinedEntityContainer.EntityMapping, secondJoinedEntity, firstJoinedEntityContainer.EntityMapping, firstJoinedEntity);
            }
        }

        /// <summary>
        /// Returns the instance container in case the entity could not be located
        /// The existing entity is also going to be updated.
        /// </summary>
        /// <returns>True in case the entity instance has never been encountered</returns>
        private bool Add<TEntity>(ref TEntity entity, out EntityInstanceContainer instanceContainer)
        {
            if (!_entityInstanceContainers.TryGetValue(typeof(TEntity), out instanceContainer))
            {
                throw new InvalidOperationException($"Type '{typeof(TEntity)}' could not be found in the list of registered instance containers.");
            }

            if (entity == null)
            {
                // we want to initialize the relationship, reason why for NULL values we'll pretend we haven't seen this instance
                return true;
            }

            object existingEntityInstance;
            var entityInstanceIdentity = new RelationshipEntityInstanceBuilderIdentity(instanceContainer.EntityMapping, entity);
            if (instanceContainer.KnownInstances.TryGetValue(entityInstanceIdentity, out existingEntityInstance))
            {
                entity = (TEntity)existingEntityInstance;
                return false;
            }

            instanceContainer.KnownInstances.Add(entityInstanceIdentity, entity);
            return true;
        }

        private static void Attach<TFirstEntity, TSecondEntity>(EntityMapping mainEntityMapping, TFirstEntity mainEntity, EntityMapping childEntityMapping, TSecondEntity childEntity)
        {
            if (mainEntity == null)
            {
                return;
            }

            // find the property we're gonna use to attach the entity
            var secondEntityType = typeof(TSecondEntity);

            EntityMappingRelationship entityRelationship;
            if (mainEntityMapping.ChildParentRelationships.TryGetValue(secondEntityType, out entityRelationship))
            {
                entityRelationship.ReferencingEntityProperty.SetValue(mainEntity, childEntity);
            }
            else if (mainEntityMapping.ParentChildRelationships.TryGetValue(secondEntityType, out entityRelationship))
            {
                var childCollection = entityRelationship.ReferencingEntityProperty.GetValue(mainEntity);
                var childCollectionList = childCollection as IList;
                if (childCollectionList == null)
                {
                    childCollectionList = new List<TSecondEntity>();
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
    }
}
