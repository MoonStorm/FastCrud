namespace Dapper.FastCrud.Mappings.Registrations
{
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

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

        private readonly List<PropertyRegistration> _propertyMappings;
        private readonly List<EntityRelationshipRegistration> _entityRelationships;

        private PropertyRegistration[]? _frozenOrderedPropertyMappings;
        private PropertyRegistration[]? _frozenOrderedPrimaryKeysPropertyMappings;
        private Dictionary<string, PropertyRegistration>? _frozenPropertyMappingsMap;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal EntityRegistration(Type entityType)
        {
            this.EntityType = entityType;

            _dialect = OrmConfiguration.DefaultDialect;
            _tableName = entityType.Name;
            _propertyMappings = new List<PropertyRegistration>();
            _entityRelationships = new List<EntityRelationshipRegistration>();
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
        /// Adds a new entity relationship or updates an existing one.
        /// </summary>
        public EntityRelationshipRegistration SetRelationship(
            EntityRelationshipType relationshipType,
            Type referencedEntity,
            string[] referencedColumnProperties,
            string[] referencingColumnProperties,
            PropertyDescriptor? referencingNavigationProperty)
        {
            Requires.NotNull(referencedEntity, nameof(referencedEntity));
            Requires.NotNull(referencedColumnProperties, nameof(referencedColumnProperties));
            Requires.NotNull(referencingColumnProperties, nameof(referencingColumnProperties));

            this.ValidateState();
            
            // try to locate an existing relationship to update or create a new one in case none was found
            var relationshipRegistration = this.TryLocateRelationshipThrowWhenMultipleAreFound(
                referencedEntity,
                relationshipType,
                referencedColumnProperties,
                referencingColumnProperties,
                referencingNavigationProperty);

            if (relationshipRegistration != null)
            {
                _entityRelationships.Remove(relationshipRegistration);
                relationshipRegistration = new EntityRelationshipRegistration(
                    relationshipType,
                    referencedEntity,
                    (referencedColumnProperties ?? Array.Empty<string>())
                        .Concat(relationshipRegistration.ReferencedColumnProperties ?? Array.Empty<string>())
                        .Distinct()
                        .ToArray(),
                    (referencingColumnProperties ?? Array.Empty<string>())
                        .Concat(relationshipRegistration.ReferencingColumnProperties ?? Array.Empty<string>())
                        .Distinct()
                        .ToArray(),
                    referencingNavigationProperty ?? relationshipRegistration.ReferencingNavigationProperty);
            }
            else
            {
                relationshipRegistration = new EntityRelationshipRegistration(
                    relationshipType,
                    referencedEntity,
                    referencedColumnProperties,
                    referencingColumnProperties,
                    referencingNavigationProperty);
            }

            _entityRelationships.Add(relationshipRegistration);

            return relationshipRegistration;
        }

        /// <summary>
        /// Removes all relationships.
        /// </summary>
        public void RemoveAllRelationships()
        {
            this.ValidateState();
            _entityRelationships.Clear();
        }

        /// <summary>
        /// Removes an existing relationship.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to locate the requested relationship.</exception>
        public EntityRelationshipRegistration RemoveRelationship(
            Type referencedEntity,
            EntityRelationshipType? relationshipType,
            string[]? referencedColumnProperties,
            string[]? referencingColumnProperties,
            PropertyDescriptor? referencingNavigationProperty)
        {
            Requires.NotNull(referencedEntity, nameof(referencedEntity));
            this.ValidateState();

            // try to locate an existing relationship to update or create a new one in case none was found
            var relationshipRegistration = this.TryLocateRelationshipThrowWhenMultipleAreFound(
                referencedEntity,
                relationshipType,
                referencedColumnProperties,
                referencingColumnProperties,
                referencingNavigationProperty);

            if (relationshipRegistration == null)
            {
                throw new InvalidOperationException("Unable to locate the requested relationship");
            }

            _entityRelationships.Remove(relationshipRegistration);
            return relationshipRegistration;
        }


        /// <summary>
        /// Removes a set of property mappings.
        /// </summary>
        public void RemoveProperties(IEnumerable<string> paramNames, bool exclude)
        {
            Requires.NotNull(paramNames, nameof(paramNames));
            this.ValidateState();

            var propNamesToRemove = _propertyMappings
                .Where(propMapping => 
                           (exclude && !paramNames.Contains(propMapping.PropertyName))
                            || (!exclude && paramNames.Contains(propMapping.PropertyName)))
                .Select(propMapping => propMapping.PropertyName)
                .ToArray();

            foreach (var propName in propNamesToRemove)
            {
                this.RemoveProperty(propName);
            }
        }

        /// <summary>
        /// Removes a property mapping.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no properties having the name were found</exception>
        public void RemoveProperty(string propertyName)
        {
            this.ValidateState();

            if (_propertyMappings.RemoveAll(propMapping => propMapping.PropertyName == propertyName) == 0)
            {
                throw new InvalidOperationException($"The mapping for the property name '{propertyName}' could not be found");
            }
        }

        /// <summary>
        /// Prepares a new property mapping. 
        /// </summary>
        public PropertyRegistration SetProperty(string propertyName)
        {
            this.ValidateState();

            var propDescriptor = TypeDescriptor.GetProperties(this.EntityType).OfType<PropertyDescriptor>().Single(propInfo => propInfo.Name == propertyName);
            return this.SetProperty(propDescriptor);
        }

        /// <summary>
        /// Registers a property mapping. 
        /// </summary>
        public PropertyRegistration SetProperty(PropertyDescriptor property)
        {
            this.ValidateState();

            return this.SetProperty(new PropertyRegistration(this, property));
        }

        /// <summary>
        /// Registers a property mapping.
        /// </summary>
        public PropertyRegistration SetProperty(PropertyRegistration propertyMapping)
        {
            Requires.Argument(propertyMapping.EntityMapping==this, nameof(propertyMapping), "Unable to add a property mapping that is not assigned to the current entity mapping");
            this.ValidateState();

            _propertyMappings.Remove(propertyMapping);
            _propertyMappings.Add(propertyMapping);

            return propertyMapping;
        }

        /// <summary>
        /// Clones the current mapping set, allowing for further modifications.
        /// </summary>
        public EntityRegistration Clone()
        {
            var clonedEntityRegistration = new EntityRegistration(this.EntityType)
                                 {
                                     Dialect = this.Dialect,
                                     SchemaName = this.SchemaName,
                                     TableName = this.TableName,
                                     DatabaseName = this.DatabaseName
                                 };
            foreach (var clonedPropMapping in this._propertyMappings.Select(propMapping => propMapping.Clone(clonedEntityRegistration)))
            {
                clonedEntityRegistration.SetProperty(clonedPropMapping);
            }

            foreach (var relationship in _entityRelationships)
            {
                clonedEntityRegistration.SetRelationship(
                    relationship.RelationshipType,
                    relationship.ReferencedEntity,
                    relationship.ReferencedColumnProperties,
                    relationship.ReferencingColumnProperties,
                    relationship.ReferencingNavigationProperty);
            }

            return clonedEntityRegistration;
        }

        /// <summary>
        /// Gets the frozen property registration, before the entity mapping gets frozen.
        /// </summary>
        public PropertyRegistration[] GetAllPropertyRegistrationsBeforeFreezing()
        {
            this.ValidateState();
            return this._propertyMappings.ToArray();
        }

        /// <summary>
        /// Gets the frozen property registration, ordered by <seealso cref="PropertyRegistration.ColumnOrder"/>.
        /// </summary>
        public PropertyRegistration[] GetAllOrderedFrozenPropertyRegistrations()
        {
            this.EnsureMappingsFrozen();
            return _frozenOrderedPropertyMappings!;
        }

        /// <summary>
        /// Gets the frozen primary key property registration, ordered by <seealso cref="PropertyRegistration.ColumnOrder"/>.
        /// </summary>
        public PropertyRegistration[] GetAllOrderedFrozenPrimaryKeyRegistrations()
        {
            this.EnsureMappingsFrozen();
            return _frozenOrderedPrimaryKeysPropertyMappings!;
        }

        /// <summary>
        /// Attempts to locate a frozen property registration by property name and throws an exception if one was not found
        /// </summary>
        public PropertyRegistration GetOrThrowFrozenPropertyRegistrationByPropertyName(string propertyName)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));

            var propertyRegistration = this.TryGetFrozenPropertyRegistrationByPropertyName(propertyName);
            if (propertyRegistration == null)
            {
                throw new InvalidOperationException($"Unable to locate the property '{propertyName}' on the entity '{this.EntityType}'");
            }

            return propertyRegistration;
        }

        /// <summary>
        /// Attempts to locate a frozen property registration by property name.
        /// </summary>
        public PropertyRegistration? TryGetFrozenPropertyRegistrationByPropertyName(string propertyName)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));

            this.EnsureMappingsFrozen();
            if (_frozenPropertyMappingsMap!.TryGetValue(propertyName, out PropertyRegistration propertyRegistration))
            {
                return propertyRegistration;
            }

            return null;

        }

        /// <summary>
        /// Tries to locate an entity relationship.
        /// </summary>
        public EntityRelationshipRegistration? TryLocateRelationshipThrowWhenMultipleAreFound(
            Type referencedEntityToFind,
            EntityRelationshipType? relationshipTypeToFind = null,
            string[]? referencedColumnPropertiesToFind = null,
            string[]? referencingColumnPropertiesToFind = null,
            PropertyDescriptor? referencingNavigationPropertyToFind = null)
        {
            var locatedRelationships = _entityRelationships
                                       .Where(existingRelationship => (relationshipTypeToFind == null
                                                                       || existingRelationship.RelationshipType == relationshipTypeToFind.Value)
                                                                      && existingRelationship.ReferencedEntity == referencedEntityToFind)
                                       .ToArray();

            if (locatedRelationships.Length > 0 && referencingNavigationPropertyToFind != null)
            {
                locatedRelationships = locatedRelationships
                                       .Where(existingRelationship => referencingNavigationPropertyToFind == existingRelationship.ReferencingNavigationProperty)
                                       .ToArray();
            }

            if (locatedRelationships.Length > 0 && referencedColumnPropertiesToFind != null && referencedColumnPropertiesToFind.Length > 0)
            {
                locatedRelationships = locatedRelationships
                                       .Where(existingRelationship => existingRelationship.ReferencedColumnProperties != null
                                                                      && referencedColumnPropertiesToFind.All(
                                                                          propertyToFind => existingRelationship.ReferencedColumnProperties.Any(existingProperty => existingProperty == propertyToFind)))
                                       .ToArray();
            }

            if (locatedRelationships.Length > 0 && referencingColumnPropertiesToFind != null && referencingColumnPropertiesToFind.Length > 0)
            {
                locatedRelationships = locatedRelationships
                                       .Where(existingRelationship => existingRelationship.ReferencingColumnProperties != null
                                                                      && referencingColumnPropertiesToFind.All(
                                                                          propertyToFind => existingRelationship.ReferencingColumnProperties.Any(existingProperty => existingProperty == propertyToFind)))
                                       .ToArray();
            }

            if (locatedRelationships.Length > 1)
            {
                string message = $"More than one relationship has been found from '{this.EntityType}': ";

                foreach (var locatedRelationship in locatedRelationships)
                {
                    switch (locatedRelationship.RelationshipType)
                    {
                        case EntityRelationshipType.ChildToParent:
                            message = $"{message} child [{string.Join(", ", locatedRelationship.ReferencingColumnProperties)}] to parent '{locatedRelationship.ReferencedEntity}'; ";
                            break;
                        case EntityRelationshipType.ParentToChildren:
                            message = $"{message} parent to child '{locatedRelationship.ReferencedEntity}' [[{string.Join(", ", locatedRelationship.ReferencedColumnProperties)}]]; ";
                            break;
                        default:
                            message = $"{message} '{this.EntityType}' and '{locatedRelationship.ReferencedEntity}'; ";
                            break;
                    }
                }
                throw new InvalidOperationException(message );
            }

            return locatedRelationships.SingleOrDefault();
        }


        /// <summary>
        /// Freezes changes to the property mappings.
        /// </summary>
        private void EnsureMappingsFrozen()
        {
            if (!_isFrozen)
            {
                // check again, this time in a locked context
                lock (this)
                {
                    if (!_isFrozen)
                    {
                        // if we have any columns, otherwise max will fail
                        if (_propertyMappings.Any())
                        {
                            // sort out the column order
                            var maxColumnOrder = _propertyMappings.Select(propMapping => propMapping.ColumnOrder).Max();
                            foreach (var propMapping in _propertyMappings)
                            {
                                if (propMapping.ColumnOrder < 0)
                                {
                                    propMapping.ColumnOrder = ++maxColumnOrder;
                                }
                            }
                        }

                        // now set up the collections
                        _frozenOrderedPrimaryKeysPropertyMappings = _propertyMappings
                                                                    .Where(prop => prop.IsPrimaryKey)
                                                                    .OrderBy(propMapping => propMapping.ColumnOrder, RelationshipOrderComparer.Default)
                                                                    .ToArray();
                        // for the ordered properties, we want the first set to always be the primary keys
                        _frozenOrderedPropertyMappings = _frozenOrderedPrimaryKeysPropertyMappings
                            .Concat(_propertyMappings
                                    .Where(prop => !prop.IsPrimaryKey)
                                    .OrderBy(propMapping => propMapping.ColumnOrder, RelationshipOrderComparer.Default))
                            .ToArray();
                        _frozenPropertyMappingsMap = _propertyMappings
                            .ToDictionary(propMapping => propMapping.PropertyName, propMapping => propMapping);

                        _isFrozen = true;
                    }
                }
            }
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
