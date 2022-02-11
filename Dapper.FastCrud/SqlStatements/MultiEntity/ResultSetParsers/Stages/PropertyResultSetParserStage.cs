﻿namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Stages
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A result set that sets up a collection.
    /// </summary>
    internal class PropertyResultSetParserStage : ResultSetParserStage
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PropertyResultSetParserStage(EntityContainer entityContainer, SqlStatementJoin joinDefinition, int dataSetRowColumnIndex)
            : base(entityContainer, joinDefinition, dataSetRowColumnIndex)
        {
        }

        /// <summary>
        /// Executes the stage provided an input entity instance, the next following entity, and returns the actual next entity used.
        /// </summary>
        protected override EntityInstanceWrapper Execute(
            EntityInstanceWrapper referencingEntityInstance,
            EntityInstanceWrapper referencedEntityInstance)
        {
            var referencingProperty = this.JoinDefinition.ReferencingNavigationProperty;
            var isReferencingPropertyCollection = this.JoinDefinition.ReferencingNavigationPropertyIsCollection;
            if (referencingProperty!=null)
            {
                if (isReferencingPropertyCollection)
                {
                    referencedEntityInstance = this.ApplyForCollectionProperty(
                        referencedEntityInstance,
                        referencingEntityInstance,
                        referencingProperty);
                }
                else
                {
                    this.ApplyForSimpleProperty(
                        referencedEntityInstance, 
                        referencingEntityInstance,
                        referencingProperty);
                }
            }

            var referencedProperty = this.JoinDefinition.ReferencedNavigationProperty;
            var isReferencedPropertyCollection = this.JoinDefinition.ReferencedNavigationPropertyIsCollection;
            if (referencedProperty != null)
            {
                if (isReferencedPropertyCollection)
                {
                    this.ApplyForCollectionProperty(
                        referencingEntityInstance,
                        referencedEntityInstance,
                        referencedProperty);
                }
                else
                {
                    this.ApplyForSimpleProperty(
                        referencingEntityInstance,
                        referencedEntityInstance,
                        referencedProperty);
                }
            }

            return referencedEntityInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EntityInstanceWrapper ApplyForCollectionProperty(
            EntityInstanceWrapper source,
            EntityInstanceWrapper target,
            PropertyDescriptor targetCollectionProperty)
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
                    targetCollectionOfSources = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(source.EntityRegistration.EntityType));
                    targetCollectionProperty.SetValue(target.EntityInstance, targetCollectionOfSources);
                }

                if (!ReferenceEquals(null, source.EntityInstance))
                {
                    actualSourceUsed = this.EntityContainer.GetOrAddToLocalCollection(
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
            EntityInstanceWrapper source,
            EntityInstanceWrapper target,
            PropertyDescriptor targetSimpleProperty)
        {
            if (!ReferenceEquals(null, target.EntityInstance) && !ReferenceEquals(null, source.EntityInstance))
            {
                targetSimpleProperty.SetValue(target.EntityInstance, source.EntityInstance);
            }
        }
    }
}
