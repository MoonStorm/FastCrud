namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers
{
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Holds unique entities of a specific type.
    /// </summary>
    internal class EntityContainer
    {
        private readonly Dictionary<EntityInstanceWrapper, EntityInstanceWrapper> _globalEntityInstanceWrappers;
        private readonly IList _globalEntityInstances;
        private readonly Dictionary<LocalEntityCollectionKey, Dictionary<EntityInstanceWrapper, EntityInstanceWrapper>> _localEntityInstances;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityContainer(Type entityType, IList? globalEntityInstances = null)
        {
            Requires.NotNull(entityType, nameof(entityType));

            this.EntityType = entityType;
            _globalEntityInstances = globalEntityInstances ?? new List<object>();
            _globalEntityInstanceWrappers = new Dictionary<EntityInstanceWrapper, EntityInstanceWrapper>();
            _localEntityInstances = new Dictionary<LocalEntityCollectionKey, Dictionary<EntityInstanceWrapper, EntityInstanceWrapper>>();
        }

        /// <summary>
        /// The entity type the container refers to.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets or adds an entity to the unique collection of entities.
        /// </summary>
        public EntityInstanceWrapper GetOrAdd(EntityInstanceWrapper entityInstance)
        {
            if (!_globalEntityInstanceWrappers.TryGetValue(entityInstance, out EntityInstanceWrapper actualEntityInstance))
            {
                _globalEntityInstanceWrappers.Add(entityInstance, entityInstance);
                _globalEntityInstances.Add(entityInstance.EntityInstance);
                actualEntityInstance = entityInstance;
            }

            return actualEntityInstance;
        }

        /// <summary>
        /// Registers a new entity to the local collection of entities.
        /// </summary>
        public EntityInstanceWrapper GetOrAdd(
            object realInstance, 
            PropertyDescriptor property, 
            IList propertyCollection,
            EntityInstanceWrapper entityInstance)
        {
            var localCollectionKey = new LocalEntityCollectionKey(realInstance, property);
            if (!_localEntityInstances.TryGetValue(localCollectionKey, out Dictionary<EntityInstanceWrapper, EntityInstanceWrapper> localInstanceWrapperCollection))
            {
                localInstanceWrapperCollection = new Dictionary<EntityInstanceWrapper, EntityInstanceWrapper>();
                _localEntityInstances.Add(localCollectionKey, localInstanceWrapperCollection);
            }

            if (!localInstanceWrapperCollection.TryGetValue(entityInstance, out EntityInstanceWrapper actualEntityInstance))
            {
                localInstanceWrapperCollection.Add(entityInstance, entityInstance);
                propertyCollection?.Add(entityInstance.EntityInstance);
                actualEntityInstance = entityInstance;
            }

            return actualEntityInstance;
        }

        private sealed class LocalEntityCollectionKey : IEquatable<LocalEntityCollectionKey>
        {
            private readonly object _instance;
            private readonly PropertyDescriptor _collectionPropertyDescriptor;

            public LocalEntityCollectionKey(object instance, PropertyDescriptor collectionPropertyDescriptor)
            {
                _instance = instance;
                _collectionPropertyDescriptor = collectionPropertyDescriptor;
            }

            /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
            public bool Equals(LocalEntityCollectionKey? other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                // enforce reference checks
                return ReferenceEquals(_instance, other._instance) && _collectionPropertyDescriptor.Equals(other._collectionPropertyDescriptor);
            }

            /// <summary>Determines whether the specified object is equal to the current object.</summary>
            /// <param name="obj">The object to compare with the current object.</param>
            /// <returns>
            /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
            public override bool Equals(object? obj)
            {
                return ReferenceEquals(this, obj) || obj is LocalEntityCollectionKey other && Equals(other);
            }

            /// <summary>Serves as the default hash function.</summary>
            /// <returns>A hash code for the current object.</returns>
            public override int GetHashCode()
            {
                return this._instance.GetHashCode().CombineHash(_collectionPropertyDescriptor.GetHashCode());
            }

            /// <summary>Returns a value that indicates whether the values of two <see cref="T:Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.EntityContainer.LocalEntityCollectionKey" /> objects are equal.</summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
            public static bool operator ==(LocalEntityCollectionKey? left, LocalEntityCollectionKey? right)
            {
                return Equals(left, right);
            }

            /// <summary>Returns a value that indicates whether two <see cref="T:Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.EntityContainer.LocalEntityCollectionKey" /> objects have different values.</summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
            public static bool operator !=(LocalEntityCollectionKey? left, LocalEntityCollectionKey? right)
            {
                return !Equals(left, right);
            }
        }
    }
}
