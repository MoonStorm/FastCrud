namespace Dapper.FastCrud.Mappings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Holds information about table mapped properties for a particular entity type.
    /// Multiple instances of such mappings can be active for a single entity type.
    /// </summary>
    public abstract class EntityMapping
    {
        private volatile bool _isFrozen;
        private readonly Dictionary<string, PropertyMapping> _propertyNameMappingsMap;
        private readonly List<PropertyMapping> _propertyMappings;
        private Dictionary<Type, EntityMappingRelationship> _childParentRelationships;
        private Dictionary<Type, EntityMappingRelationship> _parentChildRelationships;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected EntityMapping(Type entityType)
        {
            this.EntityType = entityType;
            this.TableName = entityType.Name;
            this.Dialect = OrmConfiguration.DefaultDialect;

            _propertyMappings = new List<PropertyMapping>();
            _propertyNameMappingsMap = new Dictionary<string, PropertyMapping>();
        }

        /// <summary>
        /// The table associated with the entity.
        /// </summary>
        public string TableName { get; protected set; }

        /// <summary>
        /// The schema associated with the entity.
        /// </summary>
        public string SchemaName { get; protected set; }

        /// <summary>
        /// If the entity mapping was already registered, this flag will return true. You can have multiple mappings which can be obtained by cloning this instance.
        /// </summary>
        public bool IsFrozen => _isFrozen;

        /// <summary>
        /// Current Sql dialect in use for the current entity.
        /// </summary>
        public SqlDialect Dialect { get; protected set; }

        /// <summary>
        /// Entity type.
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// Gets the property mapping asscoiated with the entity.
        /// </summary>
        internal IReadOnlyDictionary<string, PropertyMapping> PropertyMappings => _propertyNameMappingsMap;

        /// <summary>
        /// Gets all the child-parent relationships.
        /// </summary>
        internal IReadOnlyDictionary<Type, EntityMappingRelationship> ChildParentRelationships => _childParentRelationships;

        /// <summary>
        /// Gets all the parent-child relationships.
        /// </summary>
        internal IReadOnlyDictionary<Type, EntityMappingRelationship> ParentChildRelationships => _parentChildRelationships;

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

        /// <summary>
        /// Throws an exception if entity mappings cannot be changed.
        /// </summary>
        protected void ValidateState()
        {
            if (this.IsFrozen)
            {
                throw new InvalidOperationException("No further modifications are allowed for this entity mapping. Please clone the entity mapping instead.");
            }
        }

        /// <summary>
        /// Removes a set of property mappings.
        /// </summary>
        protected void RemoveProperties(IEnumerable<string> paramNames, bool exclude)
        {
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
            PropertyMapping propertyMapping;
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
        protected PropertyMapping SetPropertyInternal(string propertyName)
        {
            var propDescriptor = TypeDescriptor.GetProperties(this.EntityType).OfType<PropertyDescriptor>().Single(propInfo => propInfo.Name == propertyName);
            return this.SetPropertyInternal(propDescriptor);
        }

        /// <summary>
        /// Registers a property mapping. 
        /// </summary>
        protected PropertyMapping SetPropertyInternal(PropertyDescriptor property)
        {
            return this.SetPropertyInternal(new PropertyMapping(this, property));
        }

        /// <summary>
        /// Registers a property mapping.
        /// </summary>
        protected PropertyMapping SetPropertyInternal(PropertyMapping propertyMapping)
        {
            Requires.Argument(propertyMapping.EntityMapping==this, nameof(propertyMapping), "Unable to add a property mapping that is not assigned to the current entity mapping");
            _propertyMappings.Remove(propertyMapping);
            _propertyMappings.Add(propertyMapping);
            _propertyNameMappingsMap[propertyMapping.PropertyName] = propertyMapping;

            return propertyMapping;
        }

        private void ConstructChildParentEntityRelationships()
        {
            _childParentRelationships = _propertyMappings
                .Where(propertyMapping => propertyMapping.ChildParentRelationship!=null)
                .GroupBy(propertyMapping => propertyMapping.ChildParentRelationship.ReferencedEntityType)
                .ToDictionary(
                    groupedRelMappings => groupedRelMappings.Key,
                    groupedRelMappings =>
                    {
                        var referencingEntityPropertyNames = groupedRelMappings
                            .Select(propMapping => propMapping.ChildParentRelationship.ReferencingPropertyName)
                            .Where(propName => !string.IsNullOrEmpty(propName))
                            .Distinct()
                            .ToArray();

                        if (referencingEntityPropertyNames.Length > 1)
                        {
                            // Check for NotMapped attributes on the related Names
                            // and remove all the properties with NotMapped Attributes.
                            var finalprops = new List<String>();
                            foreach (var name in referencingEntityPropertyNames)
                            {
                                var prop = TypeDescriptor.GetProperties(this.EntityType).OfType<PropertyDescriptor>().FirstOrDefault(x => x.Name == name);

                                var attr = prop?.Attributes.OfType<NotMappedAttribute>().FirstOrDefault();
                                if (attr==null) 
                                {
                                    // Only add properties names which do not have NotMappedAttribute Set
                                    finalprops.Add(name);
                                }
                            }
                            referencingEntityPropertyNames = finalprops.ToArray();
                            if (referencingEntityPropertyNames.Length > 1)
                            {
                                throw new InvalidOperationException($"Multiple entity referencing properties were registered for the '{this.EntityType}' - '{groupedRelMappings.Key}' relationship");
                            }
                        }

                        var referencingEntityPropertyName = referencingEntityPropertyNames.Length == 0
                                                                ? null
                                                                : referencingEntityPropertyNames[0];
                        var referencingEntityPropertyDescriptor = referencingEntityPropertyName == null
                                                                      ? null
                                                                      : TypeDescriptor.GetProperties(this.EntityType)
                                                                                      .OfType<PropertyDescriptor>()
                                                                                      .SingleOrDefault(propDescriptor => propDescriptor.Name == referencingEntityPropertyName);


                        return new EntityMappingRelationship(groupedRelMappings.Key,groupedRelMappings.OrderBy(propMapping => propMapping.ColumnOrder).ToArray(), referencingEntityPropertyDescriptor);
                    });
        }

        private void ConstructParentChildEntityRelationships()
        {
            var selectedProps = TypeDescriptor.GetProperties(this.EntityType).OfType<PropertyDescriptor>()
                .Where(propDescriptor =>
                       {
                           var propInfo =
#if COREFX
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
    }

    /// <summary>
    /// Holds information about table mapped properties for a particular entity type.
    /// Multiple instances of such mappings can be active for a single entity type.
    /// </summary>
    public class EntityMapping<TEntity> : EntityMapping
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityMapping()
            : base(typeof(TEntity))
        {
        }

        /// <summary>
        /// Sets the database table associated with your entity.
        /// </summary>
        /// <param name="tableName">Table name</param>
        public EntityMapping<TEntity> SetTableName(string tableName)
        {
            Requires.NotNullOrWhiteSpace(tableName, nameof(tableName));
            this.ValidateState();

            this.TableName = tableName;
            return this;
        }

        /// <summary>
        /// Sets the database schema associated with your entity.
        /// </summary>
        /// <param name="schemaName">Shema name</param>
        public EntityMapping<TEntity> SetSchemaName(string schemaName)
        {
            this.ValidateState();

            this.SchemaName = schemaName;
            return this;
        }

        /// <summary>
        /// You can override the default dialect used for the schema.
        /// However, if plan on using the same dialect for all your db operations, it's best to use <see cref="OrmConfiguration.DefaultDialect"/> instead.
        /// </summary>
        /// <param name="dialect">Sql dialect</param>
        public EntityMapping<TEntity> SetDialect(SqlDialect dialect)
        {
            this.ValidateState();

            this.Dialect = dialect;
            return this;
        }

        /// <summary>
        /// Registers a regular property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        public EntityMapping<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            return this.SetProperty(property, (Action<PropertyMapping>)null);
        }

        /// <summary>
        /// Sets the mapping options for a property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        /// <param name="propertySetupFct">A callback which will be called for setting up the property mapping.</param>
        public EntityMapping<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, TProperty>> property,
            Action<PropertyMapping> propertySetupFct)
        {
            this.ValidateState();

            Requires.NotNull(property, nameof(property));
            //Requires.NotNull(propertySetupFct, nameof(propertySetupFct));   // it can be null

            var propName = ((MemberExpression)property.Body).Member.Name;
            var propMapping = this.SetPropertyInternal(propName);
            if (propertySetupFct != null)
            {
                propertySetupFct(propMapping);
            }
            return this;
        }

        ///// <summary>
        ///// Sets the mapping options for a property.
        ///// </summary>
        ///// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        ///// <param name="options">Column options</param>
        ///// <param name="databaseColumnName">Optional database column name override.</param>
        //[Obsolete("This method is marked as obsolete and will be removed in future versions.")]
        //public EntityMapping<TEntity> SetProperty<TProperty>(
        //    Expression<Func<TEntity, TProperty>> property,
        //    PropertyMappingOptions options,
        //    string databaseColumnName = null)
        //{
        //    Requires.NotNull(property, nameof(property));

        //    var propName = ((MemberExpression)property.Body).Member.Name;
        //    return this.SetProperty(propName, options, databaseColumnName);
        //}

        /// <summary>
        /// Returns all the property mappings, optionally filtered by their options.
        /// </summary>
        public PropertyMapping[] GetProperties(params PropertyMappingOptions[] includeFilter)
        {
            return this.PropertyMappings.Values
                .Where(propInfo => (includeFilter.Length == 0 || includeFilter.Any(options => (options & propInfo.Options) == options)))
                //.OrderBy(propInfo => propInfo.Order)
                .ToArray();
        }

        /// <summary>
        /// Gives an option for updating all the property mappings, optionally filtered by their options.
        /// </summary>
        public EntityMapping<TEntity> UpdateProperties(Action<PropertyMapping> updateFct, params PropertyMappingOptions[] includeFilter)
        {
            foreach (var propMapping in this.GetProperties(includeFilter))
            {
                updateFct(propMapping);
            }

            return this;
        }

        /// <summary>
        /// Returns all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public PropertyMapping[] GetPropertiesExcluding(params PropertyMappingOptions[] excludeFilter)
        {
            return this.PropertyMappings.Values
                .Where(propInfo => (excludeFilter.Length==0 || excludeFilter.All(options => (options & propInfo.Options) != options)))
                //.OrderBy(propInfo => propInfo.Order)
                .ToArray();
        }

        /// <summary>
        /// Gives an option for updating all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public EntityMapping<TEntity> UpdatePropertiesExcluding(Action<PropertyMapping> updateFct, params PropertyMappingOptions[] excludeFilter)
        {
            foreach (var propMapping in this.GetPropertiesExcluding(excludeFilter))
            {
                updateFct(propMapping);
            }

            return this;
        }

        /// <summary>
        /// Returns all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public PropertyMapping[] GetPropertiesExcluding(params string[] propNames)
        {
            return this.PropertyMappings.Values
                .Where(propInfo => (propNames.Length == 0 || !propNames.Contains(propInfo.PropertyName)))
                //.OrderBy(propInfo => propInfo.Order)
                .ToArray();
        }

        /// <summary>
        /// Returns all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public EntityMapping<TEntity> UpdatePropertiesExcluding(Action<PropertyMapping> updateFct,params string[] propNames)
        {
            foreach (var propMapping in this.GetPropertiesExcluding(propNames))
            {
                updateFct(propMapping);
            }

            return this;
        }

        /// <summary>
        /// Returns property mapping information for a particular property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        public PropertyMapping GetProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            var propName = ((MemberExpression)property.Body).Member.Name;
            return this.GetProperty(propName);
        }

        /// <summary>
        /// Returns property mapping information for a particular property.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public PropertyMapping GetProperty(string propertyName)
        {
            PropertyMapping propertyMapping = null;
            PropertyMappings.TryGetValue(propertyName, out propertyMapping);
            return propertyMapping;
        }

        /// <summary>
        /// Removes the mapping for a property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        public EntityMapping<TEntity> RemoveProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            this.ValidateState();

            var propName = ((MemberExpression)property.Body).Member.Name;
            this.RemoveProperties(new [] { propName}, false);
            return this;
        }

        /// <summary>
        /// Removes the mapping for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public EntityMapping<TEntity> RemoveProperty(params string[] propertyName)
        {
            this.ValidateState();

            this.RemoveProperties(propertyName, false);
            return this;
        }

        /// <summary>
        /// Removes all the property mappings with the exception of the provided list.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public EntityMapping<TEntity> RemoveAllPropertiesExcluding(params string[] propertyName)
        {
            this.ValidateState();

            this.RemoveProperties(propertyName, true);
            return this;
        }

        ///// <summary>
        ///// Sets the mapping options for a property.
        ///// </summary>
        ///// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        ///// <param name="options">Column options</param>
        ///// <param name="databaseColumnName">Optional database column name override.</param>
        //[Obsolete("This method is marked as obsolete and will be removed in future versions.")]
        //public EntityMapping<TEntity> SetProperty(string propertyName, PropertyMappingOptions options, string databaseColumnName = null)
        //{
        //    this.ValidateState();
        //    Requires.NotNull(propertyName, nameof(propertyName));

        //    var propMapping = this.SetPropertyInternal(propertyName);
        //    if (!string.IsNullOrEmpty(databaseColumnName))
        //    {
        //        propMapping.DatabaseColumnName = databaseColumnName;
        //    }
        //    if ((options & PropertyMappingOptions.DatabaseGeneratedProperty) == PropertyMappingOptions.DatabaseGeneratedProperty)
        //    {
        //        propMapping.IsDatabaseGenerated = true;
        //    }
        //    if ((options & PropertyMappingOptions.KeyProperty) == PropertyMappingOptions.KeyProperty)
        //    {
        //        propMapping.IsPrimaryKey = true;
        //    }
        //    if ((options & PropertyMappingOptions.ExcludedFromInserts) == PropertyMappingOptions.ExcludedFromInserts)
        //    {
        //        propMapping.IsExcludedFromInserts = true;
        //    }
        //    if ((options & PropertyMappingOptions.ExcludedFromUpdates) == PropertyMappingOptions.ExcludedFromUpdates)
        //    {
        //        propMapping.IsExcludedFromUpdates = true;
        //    }
        //    if ((options & PropertyMappingOptions.ReferencingForeignEntity) == PropertyMappingOptions.ReferencingForeignEntity)
        //    {
        //        throw new NotSupportedException("It is not possible to set up foreign keys via this method.");
        //    }
        //    return this;
        //}

        /// <summary>
        /// Clones the current mapping set, allowing for further modifications.
        /// </summary>
        public EntityMapping<TEntity> Clone()
        {
            var clonedMappings = new EntityMapping<TEntity>()
                .SetSchemaName(this.SchemaName)
                .SetTableName(this.TableName)
                .SetDialect(this.Dialect);
            foreach (var clonedPropMapping in this.PropertyMappings.Select(propNameMapping => propNameMapping.Value.Clone(clonedMappings)))
            {
                clonedMappings.SetPropertyInternal(clonedPropMapping);
            }

            return clonedMappings;
        }
    }
}
