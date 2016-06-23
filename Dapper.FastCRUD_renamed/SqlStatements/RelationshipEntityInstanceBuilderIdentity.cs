namespace Dapper.FastCrud.SqlStatements
{
    using System.Linq;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Entity identity used by the entity builder.
    /// </summary>
    internal sealed class RelationshipEntityInstanceBuilderIdentity
    {
        private readonly object[] _keyPropertyValues;
        private int _hashCode;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RelationshipEntityInstanceBuilderIdentity(EntityMapping entityMapping, object entity)
        {
            _keyPropertyValues = entityMapping.PropertyMappings.Values.Where(propMapping => propMapping.IsPrimaryKey).Select(
                propMapping =>
                {
                    var propDescriptor = propMapping.Descriptor;
                    var propValue = propDescriptor.GetValue(entity);

                    // assumes that all values for the entity keys provide proper hash keys
                    // 'rotating hash' is fast due to the use of bit operation, provides a good distribution and doesn't cause overflows
                    // http://eternallyconfuzzled.com/tuts/algorithms/jsw_tut_hashing.aspx
                    _hashCode = (_hashCode << 4) ^ (_hashCode >> 28) ^ (propValue?.GetHashCode() ?? 0);
                    return propValue;
                }).ToArray();
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="other">The object to compare with the current object. </param>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var otherIdentity = other as RelationshipEntityInstanceBuilderIdentity;
            if (ReferenceEquals(otherIdentity, null))
            {
                return false;
            }
            return Equals(otherIdentity);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        public static bool operator ==(RelationshipEntityInstanceBuilderIdentity left, RelationshipEntityInstanceBuilderIdentity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RelationshipEntityInstanceBuilderIdentity left, RelationshipEntityInstanceBuilderIdentity right)
        {
            return !Equals(left, right);
        }

        protected bool Equals(RelationshipEntityInstanceBuilderIdentity other)
        {
            if (_keyPropertyValues.Length != other._keyPropertyValues.Length)
            {
                return false;
            }

            for (var keyValueIndex = 0; keyValueIndex < _keyPropertyValues.Length; keyValueIndex++)
            {
                var currentKeyValue = _keyPropertyValues[keyValueIndex];
                var otherKeyValue = other._keyPropertyValues[keyValueIndex];

                if (!object.Equals(currentKeyValue, otherKeyValue))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
