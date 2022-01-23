namespace Dapper.FastCrud.Mappings
{
    using System;
    using System.ComponentModel;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Holds mapping information for a property.
    /// </summary>
    internal class PropertyMapping
    {
        private PropertyMappingOptions _options;
        private string _databaseColumnName;
        private int _columnOrder = -1;
        private PropertyMappingRelationship? _childParentRelationship;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal PropertyMapping(EntityMapping entityMapping, PropertyDescriptor descriptor)
        {
            _options = PropertyMappingOptions.None;
            _databaseColumnName = descriptor.Name;
            this.EntityMapping = entityMapping;
            this.Descriptor = descriptor;
        }

        /// <summary>
        /// Gets or sets a child-parent relationship.
        /// </summary>
        public PropertyMappingRelationship? ChildParentRelationship
        {
            get
            {
                return _childParentRelationship;
            }
            set
            {
                this.ValidateState();
                this._childParentRelationship = value;
            }
        }
        
        /// <summary>
        /// Gets the entity mapping this property mapping is attached to.
        /// </summary>
        public EntityMapping EntityMapping { get; }

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

                Requires.NotNullOrEmpty(value, nameof(this.DatabaseColumnName));
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

        internal PropertyMapping Clone(EntityMapping newEntityMapping)
        {
            var clonedPropertyMapping = new PropertyMapping(newEntityMapping, this.Descriptor)
                {
                    _options = _options,
                    _childParentRelationship = _childParentRelationship == null 
                                                   ? null 
                                                   : new PropertyMappingRelationship(
                                                       _childParentRelationship.ReferencedEntityType, 
                                                       _childParentRelationship.ReferencingParentEntityPropertyName, 
                                                       _childParentRelationship.ReferencingChildrenCollectionPropertyName),
                    _databaseColumnName = this._databaseColumnName,
                    _columnOrder =  _columnOrder

                };
            return clonedPropertyMapping;
        }

        /// <summary>
        /// Checks if two property mappings are equal.
        /// </summary>
        protected bool Equals(PropertyMapping other)
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

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(PropertyMapping left, PropertyMapping right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(PropertyMapping left, PropertyMapping right)
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
