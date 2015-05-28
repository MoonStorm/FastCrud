namespace Dapper.FastCrud.Mappings
{
    using System.ComponentModel;

    internal class PropertyMapping
    {
        private readonly PropertyMappingOptions _options;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PropertyMapping(PropertyMappingOptions options, PropertyDescriptor descriptor, string databaseColumn = null)
        {
            _options = options;
            this.DatabaseColumn = databaseColumn??descriptor.Name;
            this.Descriptor = descriptor;
        }

        public bool IsKey => (_options & PropertyMappingOptions.KeyProperty) == PropertyMappingOptions.KeyProperty;
        public bool IsDatabaseGenerated => (_options & PropertyMappingOptions.DatabaseGeneratedProperty) == PropertyMappingOptions.DatabaseGeneratedProperty;
        public bool IsExcludedFromUpdates => (_options & PropertyMappingOptions.ExcludedFromUpdates) == PropertyMappingOptions.ExcludedFromUpdates;
        public string DatabaseColumn { get; private set; }
        public PropertyDescriptor Descriptor { get; private set; }
        public string PropertyName => Descriptor.Name;

        protected bool Equals(PropertyMapping other)
        {
            return this.PropertyName.Equals(other.PropertyName);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((PropertyMapping)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.PropertyName.GetHashCode();
        }

        public static bool operator ==(PropertyMapping left, PropertyMapping right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PropertyMapping left, PropertyMapping right)
        {
            return !Equals(left, right);
        }
    }
}
