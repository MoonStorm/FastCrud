namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Containers
{
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Represents an unknown entity instance, as retrieved by executing the statement.
    /// </summary>
    internal class EntityInstanceWrapper : IEquatable<EntityInstanceWrapper>
    {
        private readonly Lazy<object[]> _keyPropertyValues;
        private readonly Lazy<int> _hashCode;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityInstanceWrapper(EntityRegistration entityRegistration, object? entityInstance)
        {
            Validate.NotNull(entityRegistration, nameof(entityRegistration));

            this.EntityRegistration = entityRegistration;
            this.EntityInstance = entityInstance;

            _keyPropertyValues = new Lazy<object[]>(this.DiscoverKeyPropertyValues, LazyThreadSafetyMode.PublicationOnly);
            _hashCode = new Lazy<int>(this.ComputeHashCode, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the property registration attached to the instance.
        /// </summary>
        public EntityRegistration EntityRegistration { get; }

        /// <summary>
        /// Gets the underlying instance.
        /// </summary>
        public object? EntityInstance { get; }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(EntityInstanceWrapper? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.EntityRegistration.EntityType != other.EntityRegistration.EntityType)
            {
                return false;
            }

            var currentKeyProperties = _keyPropertyValues.Value;
            var otherKeyProperties = other._keyPropertyValues.Value;

            if (currentKeyProperties.Length != otherKeyProperties.Length)
            {
                return false;
            }

            for (var keyValueIndex = 0; keyValueIndex < currentKeyProperties.Length; keyValueIndex++)
            {
                var currentKeyPropValue = currentKeyProperties[keyValueIndex];
                var otherKeyPropValue = otherKeyProperties[keyValueIndex];

                if (!object.Equals(currentKeyPropValue, otherKeyPropValue))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return this.Equals(obj as EntityInstanceWrapper);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return _hashCode.Value;
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:Dapper.FastCrud.SqlStatements.MultiEntity.RelationshipEntityInstance" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(EntityInstanceWrapper? left, EntityInstanceWrapper? right)
        {
            return Equals(left, right);
        }

        /// <summary>Returns a value that indicates whether two <see cref="T:Dapper.FastCrud.SqlStatements.MultiEntity.RelationshipEntityInstance" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(EntityInstanceWrapper? left, EntityInstanceWrapper? right)
        {
            return !Equals(left, right);
        }

        private int ComputeHashCode()
        {
            var computedHash = 0.GetHashCode().CombineHash(_keyPropertyValues.Value.Select(keyPropValue => keyPropValue?.GetHashCode()).ToArray());
            return computedHash;
        }

        private object[] DiscoverKeyPropertyValues()
        {
            return this.EntityInstance == null
                       ? Array.Empty<object>() 
                       : this.EntityRegistration.GetAllOrderedFrozenPrimaryKeyRegistrations()
                       .Select(propRegistration =>
                       {
                           var propDescriptor = propRegistration.Descriptor;
                           try
                           {
                               var propKeyValue = propDescriptor.GetValue(this.EntityInstance);
                               return propKeyValue;
                           }
                           catch (Exception ex)
                           {
                               throw new InvalidOperationException($"Unable to extract the value for the key property '{propDescriptor.Name}' for the entity '{this.EntityRegistration.EntityType}'", ex);
                           }
                       })
                       .ToArray();
        }

    }
}
