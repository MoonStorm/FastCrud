namespace Dapper.FastCrud.Mappings.Registrations
{
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Holds information about table mapped properties for a particular entity type.
    /// Multiple instances of such mappings can be active for a single entity type.
    /// </summary>
    internal class EntityRegistration
    {
        private volatile bool _isFrozen;
        private string _tableName;
        private string? _schemaName;
        private string? _databaseName;
        private SqlDialect _dialect;

        private readonly Dictionary<string, PropertyRegistration> _propertyNameMappingsMap;
        private readonly List<PropertyRegistration> _propertyMappings;
        private Dictionary<Type, EntityMappingRelationship> _childParentRelationships;
        private Dictionary<Type, EntityMappingRelationship> _parentChildRelationships;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal EntityRegistration(Type entityType)
        {
            this.EntityType = entityType;

            _dialect = OrmConfiguration.DefaultDialect;
            _tableName = entityType.Name;
            _propertyMappings = new List<PropertyRegistration>();
            _propertyNameMappingsMap = new Dictionary<string, PropertyRegistration>();
        }

        /// <summary>
        /// The table associated with the entity.
        /// </summary>
        public string TableName
        {
            get => _tableName;
            set
            {
                Requires.NotNullOrWhiteSpace(value, nameof(this.TableName));
                this.ValidateState();
                _tableName = value;
            }
        }

        /// <summary>
        /// The schema associated with the entity.
        /// </summary>
        public string? SchemaName
        {
            get => _schemaName;
            set
            {
                this.ValidateState();
                _schemaName = value;
            }
        }
        
        /// <summary>
        /// The database associated with the entity.
        /// </summary>
        public string? DatabaseName
        {
            get => _databaseName;
            set
            {
                this.ValidateState();
                _databaseName = value;
            }
        }

        /// <summary>
        /// Current Sql dialect in use for the current entity.
        /// </summary>
        public SqlDialect Dialect
        {
            get => _dialect;
            set
            {
                this.ValidateState();
                _dialect = value;
            }
        }

        /// <summary>
        /// If the entity mapping was already registered, this flag will return true. You can have multiple mappings which can be obtained by cloning this instance.
        /// </summary>
        public bool IsFrozen => _isFrozen;

        /// <summary>
        /// Entity type.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the property mapping asscoiated with the entity.
        /// </summary>
        internal IReadOnlyDictionary<string, PropertyRegistration> PropertyMappings => _propertyNameMappingsMap;

        /// <summary>
        /// Gets all the child-parent relationships.
        /// </summary>
        internal IReadOnlyDictionary<Type, EntityMappingRelationship> ChildParentRelationships => _childParentRelationships;

        /// <summary>
        /// Gets all the parent-child relationships.
        /// </summary>
        internal IReadOnlyDictionary<Type, EntityMappingRelationship> ParentChildRelationships => _parentChildRelationships;

        /// <summary>
        /// Removes a set of property mappings.
        /// </summary>
        internal void RemoveProperties(IEnumerable<string> paramNames, bool exclude)
        {
            this.ValidateState();

            var propNamesMappingsToRemove = new List<string>(_propertyMappings.Count);
            propNamesMappingsToRemove.AddRange(from propMapping in this.PropertyMappings where (exclude && !paramNames.Contains(propMapping.Value.PropertyName)) || (!exclude && paramNames.Contains(propMapping.Value.PropertyName)) select propMapping.Key);

            foreach (var propName in propNamesMappingsToRemove)
            {
                this.RemoveProperty(propName);
            }
        }

        /// <summary>
        /// Removes a property mapping.
        /// </summary>
        internal void RemoveProperty(string propertyName)
        {
            this.ValidateState();

            PropertyRegistration propertyMapping;
            if (_propertyNameMappingsMap.TryGetValue(propertyName, out propertyMapping))
            {
                if (!_propertyNameMappingsMap.Remove(propertyName) || !_propertyMappings.Remove(propertyMapping))
                {
                    throw new InvalidOperationException($"Failure removing property '{propertyName}'");
                }
            } 
        }

        /// <summary>
        /// Prepares a new property mapping. 
        /// </summary>
        internal PropertyRegistration SetProperty(string propertyName)
        {
            this.ValidateState();

            var propDescriptor = TypeDescriptor.GetProperties(this.EntityType).OfType<PropertyDescriptor>().Single(propInfo => propInfo.Name == propertyName);
            return this.SetProperty(propDescriptor);
        }

        /// <summary>
        /// Registers a property mapping. 
        /// </summary>
        internal PropertyRegistration SetProperty(PropertyDescriptor property)
        {
            this.ValidateState();

            return this.SetProperty(new PropertyRegistration(this, property));
        }

        /// <summary>
        /// Registers a property mapping.
        /// </summary>
        internal PropertyRegistration SetProperty(PropertyRegistration propertyMapping)
        {
            Requires.Argument(propertyMapping.EntityMapping==this, nameof(propertyMapping), "Unable to add a property mapping that is not assigned to the current entity mapping");
            this.ValidateState();

            _propertyMappings.Remove(propertyMapping);
            _propertyMappings.Add(propertyMapping);
            _propertyNameMappingsMap[propertyMapping.PropertyName] = propertyMapping;

            return propertyMapping;
        }

        /// <summary>
        /// Clones the current mapping set, allowing for further modifications.
        /// </summary>
        public EntityRegistration Clone()
        {
            var clonedMappings = new EntityRegistration(this.EntityType)
                                 {
                                     Dialect = this.Dialect,
                                     SchemaName = this.SchemaName,
                                     TableName = this.TableName,
                                     DatabaseName = this.DatabaseName
                                 };
            foreach (var clonedPropMapping in this.PropertyMappings.Select(propNameMapping => propNameMapping.Value.Clone(clonedMappings)))
            {
                clonedMappings.SetProperty(clonedPropMapping);
            }

            return clonedMappings;
        }

        /// <summary>
        /// Freezes changes to the property mappings.
        /// </summary>
        internal void FreezeMapping()
        {
            if (!_isFrozen)
            {
                // check again, this time in a locked context
                lock (this)
                {
                    if (!_isFrozen)
                    {
                        var maxColumnOrder = _propertyMappings.Select(propMapping => propMapping.ColumnOrder).Max();
                        foreach (var propMapping in _propertyMappings)
                        {
                            if (propMapping.ColumnOrder < 0)
                            {
                                propMapping.ColumnOrder = ++maxColumnOrder;
                            }
                        }

                        this.ConstructChildParentEntityRelationships();
                        this.ConstructParentChildEntityRelationships();

                        _isFrozen = true;
                    }
                }
            }
        }

        private void ConstructChildParentEntityRelationships()
        {
            //_childParentRelationships = _propertyMappings
            //    .Where(propertyMapping => propertyMapping.ChildParentRelationship!=null)
            //    .GroupBy(propertyMapping => propertyMapping.ChildParentRelationship.ReferencedEntityType)
            //    .ToDictionary(
            //        groupedRelMappings => groupedRelMappings.Key,
            //        groupedRelMappings =>
            //        {
            //            var referencingEntityPropertyNames = groupedRelMappings
            //                .Select(propMapping => propMapping.ChildParentRelationship.ReferencingPropertyName)
            //                .Where(propName => !string.IsNullOrEmpty(propName))
            //                .Distinct()
            //                .ToArray();

            //            if (referencingEntityPropertyNames.Length > 1)
            //            {
            //                // Check for NotMapped attributes on the related Names
            //                // and remove all the properties with NotMapped Attributes.
            //                var finalprops = new List<String>();
            //                foreach (var name in referencingEntityPropertyNames)
            //                {
            //                    var prop = TypeDescriptor.GetProperties(this.EntityType).OfType<PropertyDescriptor>().FirstOrDefault(x => x.Name == name);

            //                    var attr = prop?.Attributes.OfType<NotMappedAttribute>().FirstOrDefault();
            //                    if (attr==null) 
            //                    {
            //                        // Only add properties names which do not have NotMappedAttribute Set
            //                        finalprops.Add(name);
            //                    }
            //                }
            //                referencingEntityPropertyNames = finalprops.ToArray();
            //                if (referencingEntityPropertyNames.Length > 1)
            //                {
            //                    throw new InvalidOperationException($"Multiple entity referencing properties were registered for the '{this.EntityType}' - '{groupedRelMappings.Key}' relationship");
            //                }
            //            }

            //            var referencingEntityPropertyName = referencingEntityPropertyNames.Length == 0
            //                                                    ? null
            //                                                    : referencingEntityPropertyNames[0];
            //            var referencingEntityPropertyDescriptor = referencingEntityPropertyName == null
            //                                                          ? null
            //                                                          : TypeDescriptor.GetProperties(this.EntityType)
            //                                                                          .OfType<PropertyDescriptor>()
            //                                                                          .SingleOrDefault(propDescriptor => propDescriptor.Name == referencingEntityPropertyName);


            //            return new EntityMappingRelationship(groupedRelMappings.Key,groupedRelMappings.OrderBy(propMapping => propMapping.ColumnOrder).ToArray(), referencingEntityPropertyDescriptor);
            //        });
        }

        private void ConstructParentChildEntityRelationships()
        {
            var selectedProps = TypeDescriptor.GetProperties(this.EntityType).OfType<PropertyDescriptor>()
                .Where(propDescriptor =>
                       {
                           var propInfo =
#if NETSTANDARD
                           propDescriptor.PropertyType.GetTypeInfo();
#else
                           propDescriptor.PropertyType;
#endif
                                    return propInfo.IsGenericType 
                                            && typeof(IEnumerable).IsAssignableFrom(propDescriptor.PropertyType)
                                            && propDescriptor.PropertyType.GetGenericArguments().Length == 1 
                                            && !propDescriptor.Attributes.OfType<NotMappedAttribute>().Any(); ;
                                });
                //.GroupBy(propDescriptor => propDescriptor.PropertyType)
            _parentChildRelationships = selectedProps.ToDictionary(
                    propDescriptor => propDescriptor.PropertyType.GetGenericArguments()[0],
                    propDescriptor =>
                    {
                        // get the keys and order them
                        var keyPropMappings = _propertyMappings.Where(propMapping => propMapping.IsPrimaryKey).OrderBy(propMapping => propMapping.ColumnOrder).ToArray();
                        return new EntityMappingRelationship(propDescriptor.PropertyType, keyPropMappings, propDescriptor);
                    });
        }

        /// <summary>
        /// Throws an exception if entity mappings cannot be changed.
        /// </summary>
        private void ValidateState()
        {
            if (this.IsFrozen)
            {
                throw new InvalidOperationException("No further modifications are allowed for this entity mapping. Please clone the entity mapping instead.");
            }
        }
    }

}
