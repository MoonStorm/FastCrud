namespace Dapper.FastCrud.Mappings
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Reeturns mapping information held for a particular property.
    /// </summary>
    public class PropertyMapping
    {
        private PropertyMappingOptions _options;
        private readonly EntityMapping _entityMapping;
        private string _databaseColumnName;
        private readonly int _order;
        private string _foreignKeyPropertyName;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PropertyMapping(EntityMapping entityMapping, int order,  PropertyMappingOptions options, PropertyDescriptor descriptor, string databaseColumn = null, string foreignKeyPropertyName = null)
        {
            _order = order;
            _entityMapping = entityMapping;
            _options = options;
            this.ForeignKeyPropertyName = foreignKeyPropertyName;
            _databaseColumnName = databaseColumn??(((_options&PropertyMappingOptions.ReferencingForeignEntity)==PropertyMappingOptions.ReferencingForeignEntity)?null: descriptor.Name);
            this.Descriptor = descriptor;
        }

        /// <summary>
        /// Gets or sets the property name that represents the foreign key to be used for fetching the referenced entity. 
        /// This can only be set on properties that have an entity type.
        /// </summary>
        public string ForeignKeyPropertyName
        {
            get
            {
                return _foreignKeyPropertyName;
            }
            set
            {
                this.ValidateState();

                this._foreignKeyPropertyName = value;
                if (string.IsNullOrEmpty(_foreignKeyPropertyName))
                {
                    _options &= ~PropertyMappingOptions.ReferencingForeignEntity;
                }
                else
                {
                    _options |= PropertyMappingOptions.ReferencingForeignEntity;
                    _options |= PropertyMappingOptions.ExcludedFromInserts;
                    _options |= PropertyMappingOptions.ExcludedFromUpdates;
                }
            }
        }

        /// <summary>
        /// Gets a flag indicating the property is referencing a foreign entity.
        /// One can be set via <see cref="ForeignKeyPropertyName"/>.
        /// </summary>
        public bool IsReferencingForeignEntity
        {
            get
            {
                return (_options & PropertyMappingOptions.ReferencingForeignEntity) == PropertyMappingOptions.ReferencingForeignEntity;
            }
            set
            {
                this.ValidateState();

                if (value)
                {
                    throw new InvalidOperationException($"'{nameof(IsReferencingForeignEntity)}' cannot be set. Please use '{nameof(ForeignKeyPropertyName)}' instead.");
                }

                this.ForeignKeyPropertyName = null;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating the property is mapped to a primary key.
        /// </summary>
        public bool IsKey
        {
            get
            {
                return (_options & PropertyMappingOptions.KeyProperty) == PropertyMappingOptions.KeyProperty;
            }
            set
            {
                this.ValidateState();

                if (value)
                    _options |= PropertyMappingOptions.KeyProperty;
                else
                {
                    _options&=~PropertyMappingOptions.KeyProperty;
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating the property is mapped to a database generated field.
        /// </summary>
        public bool IsDatabaseGenerated
        {
            get
            {
                return (_options & PropertyMappingOptions.DatabaseGeneratedProperty) == PropertyMappingOptions.DatabaseGeneratedProperty;
            }
            set
            {
                this.ValidateState();

                if (value)
                {
                    _options |= PropertyMappingOptions.DatabaseGeneratedProperty;
                }
                else
                {
                    _options &= ~PropertyMappingOptions.DatabaseGeneratedProperty;
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
        /// Gets the currently assigned order.
        /// </summary>
        public int Order
        {
            get
            {
                return _order;
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

                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(DatabaseColumnName));
                }

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
        public string PropertyName => Descriptor.Name;

        /// <summary>
        /// Gets or sets the full set of options.
        /// </summary>
        public PropertyMappingOptions Options
        {
            get
            {
                return _options;
            }
            set
            {
                this.ValidateState();

                _options = value;
            }
        }

        /// <summary>
        /// Removes the current property mapping.
        /// </summary>
        public void Remove()
        {
            this.ValidateState();

            _entityMapping.RemoveProperty(this.PropertyName);
        }

        protected bool Equals(PropertyMapping other)
        {
            return this._entityMapping.Equals(other._entityMapping) && this.PropertyName.Equals(other.PropertyName);
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

        private void ValidateState()
        {
            if (_entityMapping.IsFrozen)
            {
                throw new InvalidOperationException("No further modifications are allowed for this entity mapping. Please clone the entity mapping instead.");
            }
        }

    }
}
