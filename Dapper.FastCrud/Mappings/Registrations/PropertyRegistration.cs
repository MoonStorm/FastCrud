namespace Dapper.FastCrud.Mappings.Registrations
{
    using Dapper.FastCrud.Validations;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Holds mapping information for a property.
    /// </summary>
    internal class PropertyRegistration
    {
        private PropertyMappingOptions _options;
        private string _databaseColumnName;
        private int _columnOrder = -1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal PropertyRegistration(EntityRegistration entityMapping, PropertyDescriptor descriptor)
        {
            _options = PropertyMappingOptions.None;
            _databaseColumnName = descriptor.Name;
            this.EntityMapping = entityMapping;
            this.Descriptor = descriptor;
        }

        /// <summary>
        /// Gets the entity mapping this property mapping is attached to.
        /// </summary>
        public EntityRegistration EntityMapping { get; }

        /// <summary>
        /// Gets or sets a flag indicating the property is mapped to a primary key.
        /// </summary>
        public bool IsPrimaryKey
        {
            get
            {
                return (_options & PropertyMappingOptions.KeyProperty) == PropertyMappingOptions.KeyProperty;
            }
            set
            {
                this.ValidateState();

                if (value)
                {
                    _options |= PropertyMappingOptions.KeyProperty;
                    this.IsExcludedFromUpdates = true;
                }
                else
                {
                    _options &= ~PropertyMappingOptions.KeyProperty;
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating the property is refreshed after an INSERT.
        /// </summary>
        public bool IsRefreshedOnInserts
        {
            get
            {
                return (_options & PropertyMappingOptions.RefreshPropertyOnInserts) == PropertyMappingOptions.RefreshPropertyOnInserts;
            }
            set
            {
                this.ValidateState();

                if (value)
                {
                    _options |= PropertyMappingOptions.RefreshPropertyOnInserts;
                }
                else
                {
                    _options &= ~PropertyMappingOptions.RefreshPropertyOnInserts;
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating the property is refreshed after an UPDATE.
        /// </summary>
        public bool IsRefreshedOnUpdates
        {
            get
            {
                return (_options & PropertyMappingOptions.RefreshPropertyOnUpdates) == PropertyMappingOptions.RefreshPropertyOnUpdates;
            }
            set
            {
                this.ValidateState();

                if (value)
                {
                    _options |= PropertyMappingOptions.RefreshPropertyOnUpdates;
                }
                else
                {
                    _options &= ~PropertyMappingOptions.RefreshPropertyOnUpdates;
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates the curent property will be excluded from updates.
        /// </summary>
        public bool IsExcludedFromInserts
        {
            get
            {
                return (_options & PropertyMappingOptions.ExcludedFromInserts) == PropertyMappingOptions.ExcludedFromInserts;
            }
            set
            {
                this.ValidateState();

                if (value)
                {
                    _options |= PropertyMappingOptions.ExcludedFromInserts;
                }
                else
                {
                    _options &= ~PropertyMappingOptions.ExcludedFromInserts;
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates the curent property will be excluded from updates.
        /// </summary>
        public bool IsExcludedFromUpdates
        {
            get
            {
                return (_options & PropertyMappingOptions.ExcludedFromUpdates) == PropertyMappingOptions.ExcludedFromUpdates;
            }
            set
            {
                this.ValidateState();

                if (value)
                {
                    _options |= PropertyMappingOptions.ExcludedFromUpdates;
                }
                else
                {
                    _options &= ~PropertyMappingOptions.ExcludedFromUpdates;
                }
            }
        }

        /// <summary>
        /// Gets or sets the database column name.
        /// </summary>
        public string DatabaseColumnName
        {
            get
            {
                return _databaseColumnName;
            }
            set
            {
                this.ValidateState();

                Validate.NotNullOrEmpty(value, nameof(this.DatabaseColumnName));
                _databaseColumnName = value;
            }
        }

        /// <summary>
        /// Gets the property descriptor of the property attached to the entity type.
        /// </summary>
        public PropertyDescriptor Descriptor { get; private set; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName => this.Descriptor.Name;

        /// <summary>
        /// Gets or sets the full set of options.
        /// </summary>
        internal PropertyMappingOptions Options
        {
            get
            {
                return _options;
            }
        }

        /// <summary>
        /// Gets or sets the column order, normally used for matching foreign keys with the primary composite keys.
        /// </summary>
        public int ColumnOrder
        {
            get
            {
                return _columnOrder;
            }
            set
            {
                this.ValidateState();

                _columnOrder = value;
            }
        }

        internal PropertyRegistration Clone(EntityRegistration newEntityMapping)
        {
            var clonedPropertyMapping = new PropertyRegistration(newEntityMapping, this.Descriptor)
                {
                    _options = _options,
                    _databaseColumnName = this._databaseColumnName,
                    _columnOrder =  _columnOrder

                };
            return clonedPropertyMapping;
        }

        /// <summary>
        /// Checks if two property mappings are equal.
        /// </summary>
        protected bool Equals(PropertyRegistration other)
        {
            return this.EntityMapping.Equals(other.EntityMapping) && this.PropertyName.Equals(other.PropertyName);
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
            return this.Equals((PropertyRegistration)obj);
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

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(PropertyRegistration left, PropertyRegistration right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(PropertyRegistration left, PropertyRegistration right)
        {
            return !Equals(left, right);
        }

        private void ValidateState()
        {
            if (this.EntityMapping.IsFrozen)
            {
                throw new InvalidOperationException("No further modifications are allowed for this entity mapping. Please clone the entity mapping instead.");
            }
        }

    }
}
