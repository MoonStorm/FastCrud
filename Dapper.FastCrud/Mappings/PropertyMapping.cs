namespace Dapper.FastCrud.Mappings
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Reeturns mapping information held for a particular property.
    /// </summary>
    public class PropertyMapping
    {
        private PropertyMappingOptions _options;
        private string _databaseColumnName;
        private int _columnOrder = -1;
        private PropertyMappingRelationship _childParentRelationship;

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
        public PropertyMappingRelationship ChildParentRelationship
        {
            get
            {
                return _childParentRelationship;
            }
            set
            {
                this.ValidateState();
                _childParentRelationship = value;
            }
        }

        /// <summary>
        /// Sets up a foreign key relationship with another entity.
        /// </summary>
        /// <typeparam name="TRelatedEntityType">Foreign entity type.</typeparam>
        /// <param name="referencingEntityPropertyName">The name of the property on the current entity that would hold the referenced entity when instructed to do so in a JOIN statement.</param>
        public PropertyMapping SetChildParentRelationship<TRelatedEntityType>(string referencingEntityPropertyName)
        {
            return this.SetChildParentRelationship(typeof(TRelatedEntityType), referencingEntityPropertyName);
        }

        /// <summary>
        /// Sets up a foreign key relationship with another entity.
        /// </summary>
        /// <param name="relatedEntityType">Foreign entity type.</param>
        /// <param name="referencingEntityPropertyName">The name of the property on the current entity that would hold the referenced entity when instructed to do so in a JOIN statement.</param>
        internal PropertyMapping SetChildParentRelationship(Type relatedEntityType, string referencingEntityPropertyName)
        {
            this.ChildParentRelationship = new PropertyMappingRelationship(relatedEntityType, referencingEntityPropertyName);
            return this;
        }

        /// <summary>
        /// Removes a parent-child relationship. 
        /// </summary>
        public PropertyMapping RemoveChildParentRelationship()
        {
            this.ChildParentRelationship = null;
            return this;
        }

        /// <summary>
        /// Gets the entity mapping this property mapping is attached to.
        /// </summary>
        public EntityMapping EntityMapping { get; private set; }

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
        /// Marks the property as primary key.
        /// </summary>
        public PropertyMapping SetPrimaryKey(bool isPrimaryKey = true)
        {
            this.IsPrimaryKey = isPrimaryKey;
            return this;
        }

        /// <summary>
        /// Indicates that the property is mapped to a database generated field.
        /// This does not cover default values generated by the database (please use <see cref="ExcludeFromInserts"/> and <see cref="RefreshOnInserts"/> for this scenario).
        /// </summary>
        public PropertyMapping SetDatabaseGenerated(DatabaseGeneratedOption option)
        {
            switch (option)
            {
                case DatabaseGeneratedOption.Computed:
                    this.IsExcludedFromInserts = true;
                    this.IsExcludedFromUpdates = true;
                    this.IsRefreshedOnInserts = true;
                    this.IsRefreshedOnUpdates = true;
                    break;
                case DatabaseGeneratedOption.Identity:
                    this.IsExcludedFromInserts = true;
                    this.IsExcludedFromUpdates = true;
                    this.IsRefreshedOnInserts = true;
                    this.IsRefreshedOnUpdates = false;
                    break;
                case DatabaseGeneratedOption.None:
                    this.IsExcludedFromInserts = false;
                    this.IsExcludedFromUpdates = false;
                    this.IsRefreshedOnInserts = false;
                    this.IsRefreshedOnUpdates = false;
                    break;
                default:
                    throw new NotSupportedException($"Option {option} is not supported.");
            }
            return this;
        }
        /////// <summary>
        /////// Gets or sets a flag indicating the property is mapped to a database generated field.
        /////// </summary>
        ////public bool IsDatabaseGenerated
        ////{
        ////    get
        ////    {
        ////        return (_options & PropertyMappingOptions.DatabaseGeneratedProperty) == PropertyMappingOptions.DatabaseGeneratedProperty;
        ////    }
        ////    set
        ////    {
        ////        this.ValidateState();

        ////        if (value)
        ////        {
        ////            _options |= PropertyMappingOptions.DatabaseGeneratedProperty;
        ////            this.IsExcludedFromInserts = true;
        ////        }
        ////        else
        ////        {
        ////            _options &= ~PropertyMappingOptions.DatabaseGeneratedProperty;
        ////        }
        ////    }
        ////}

        /// <summary>
        /// Sets the column order, normally used for matching foreign keys with the primary composite keys.
        /// </summary>
        public PropertyMapping SetColumnOrder(int columnOrder)
        {
            this.ColumnOrder = columnOrder;
            return this;
        }

        /// <summary>
        /// Indicates that the property gets refreshed on INSERTs.
        /// </summary>
        public PropertyMapping RefreshOnInserts(bool refreshOnInsert = true)
        {
            this.IsRefreshedOnInserts = refreshOnInsert;
            return this;
        }

        /// <summary>
        /// Indicates that the property gets refreshed on UPDATEs.
        /// </summary>
        public PropertyMapping RefreshOnUpdates(bool refreshOnUpdate = true)
        {
            this.IsRefreshedOnUpdates = refreshOnUpdate;
            return this;
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
        /// The property will be included in insert operations.
        /// </summary>
        public PropertyMapping IncludeInInserts()
        {
            this.IsExcludedFromInserts = false;
            return this;
        }

        /// <summary>
        /// The property will be excluded from update operations.
        /// </summary>
        public PropertyMapping ExcludeFromInserts()
        {
            this.IsExcludedFromInserts = true;
            return this;
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
        /// The property will be included in update operations.
        /// </summary>
        public PropertyMapping IncludeInUpdates()
        {
            this.IsExcludedFromUpdates = false;
            return this;
        }

        /// <summary>
        /// The property will be excluded from update operations.
        /// </summary>
        public PropertyMapping ExcludeFromUpdates()
        {
            this.IsExcludedFromUpdates = true;
            return this;
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
        /// Sets the database column name.
        /// </summary>
        public PropertyMapping SetDatabaseColumnName(string dbColumnName)
        {
            this.DatabaseColumnName = dbColumnName;
            return this;
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

        /// <summary>
        /// Removes the current property mapping.
        /// </summary>
        public void Remove()
        {
            this.ValidateState();

            this.EntityMapping.RemoveProperty(this.PropertyName);
        }

        internal PropertyMapping Clone(EntityMapping newEntityMapping)
        {
            var clonedPropertyMapping = new PropertyMapping(newEntityMapping, this.Descriptor)
                {
                    _options = _options,
                    _childParentRelationship = _childParentRelationship == null ? null : new PropertyMappingRelationship(_childParentRelationship.ReferencedEntityType, _childParentRelationship.ReferencingPropertyName),
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
