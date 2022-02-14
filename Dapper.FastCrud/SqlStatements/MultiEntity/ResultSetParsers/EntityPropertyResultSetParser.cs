namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers
{
    using Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Containers;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A result set that sets up a collection.
    /// </summary>
    internal class EntityPropertyResultSetParser : BasicResultSetParser
    {
        private readonly SqlStatementJoinRelationship _joinRelationship;
        private readonly int _newDataRowIndex;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityPropertyResultSetParser(EntityContainer sharedEntityContainer,
                                             SqlStatementJoinRelationship joinRelationship,
                                             int newDataRowIndex)
            : base(sharedEntityContainer)
        {
            _joinRelationship = joinRelationship;
            _newDataRowIndex = newDataRowIndex;
        }

        /// <summary>
        /// Executes the current stage and produces the next instance.
        /// </summary>
        protected override EntityInstanceWrapper ProduceNextInstance(EntityInstanceWrapper? referencingEntity, EntityInstanceWrapper[] originalEntityRow)
        {
            var referencedEntity = originalEntityRow[_newDataRowIndex];
            if (referencingEntity != null)
            {
                if (_joinRelationship.ReferencingNavigationProperty != null)
                {
                    if (_joinRelationship.ReferencingNavigationPropertyIsCollection)
                    {
                        this.ApplyForCollectionProperty(
                            referencingEntity,
                            _joinRelationship.ReferencingNavigationProperty,
                            referencedEntity);
                    }
                    else
                    {
                        this.ApplyForSimpleProperty(
                            referencingEntity,
                            _joinRelationship.ReferencingNavigationProperty,
                            referencedEntity);
                    }
                }

                if (_joinRelationship.ReferencedNavigationProperty != null)
                {
                    if (_joinRelationship.ReferencedNavigationPropertyIsCollection)
                    {
                        this.ApplyForCollectionProperty(
                            referencedEntity,
                            _joinRelationship.ReferencedNavigationProperty,
                            referencingEntity);
                    }
                    else
                    {
                        this.ApplyForSimpleProperty(
                            referencedEntity,
                            _joinRelationship.ReferencedNavigationProperty,
                            referencingEntity);
                    }
                }
            }

            return referencedEntity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EntityInstanceWrapper ApplyForCollectionProperty(
            EntityInstanceWrapper target,
            PropertyDescriptor targetCollectionProperty,
            EntityInstanceWrapper source)
        {
            EntityInstanceWrapper actualSourceUsed;
            if (ReferenceEquals(null, target.EntityInstance))
            {
                actualSourceUsed = source;
            }
            else
            {
                var targetCollectionOfSources = targetCollectionProperty.GetValue(target.EntityInstance) as IList;
                if (targetCollectionOfSources == null)
                {
                    targetCollectionOfSources = OrmConfiguration.Conventions.CreateEntityCollection(target.EntityInstance, targetCollectionProperty, source.EntityRegistration.EntityType);
                }

                if (!ReferenceEquals(null, source.EntityInstance))
                {
                    actualSourceUsed = this.SharedContainer[source.EntityRegistration.EntityType].GetOrAddToLocalCollection(
                        target.EntityInstance,
                        targetCollectionProperty,
                        targetCollectionOfSources,
                        source);
                    // targetCollectionOfSources.Add(source.EntityInstance);
                }
                else
                {
                    actualSourceUsed = source;
                }
            }

            return actualSourceUsed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyForSimpleProperty(
            EntityInstanceWrapper target,
            PropertyDescriptor targetSimpleProperty,
            EntityInstanceWrapper source)
        {
            if (!ReferenceEquals(null, target.EntityInstance) && !ReferenceEquals(null, source.EntityInstance))
            {
                targetSimpleProperty.SetValue(target.EntityInstance, source.EntityInstance);
            }
        }
    }
}
