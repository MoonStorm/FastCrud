namespace Dapper.FastCrud.Mappings
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Holds information about table mapped properties for a particular entity type.
    /// Multiple instances of such mappings can be active for a single entity type.
    /// </summary>
    public abstract class EntityMapping
    {
        private volatile bool _isFrozen;
        private IDictionary<string, PropertyMapping> _propertyMappings;
        //private static long _currentGlobalId = long.MinValue;
        //private readonly long _id;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected EntityMapping(Type entityType)
        {
            //this._id = Interlocked.Increment(ref _currentGlobalId);
            this.EntityType = entityType;
            this._propertyMappings = new Dictionary<string, PropertyMapping>();
            this.TableName = entityType.Name;
            this.Dialect = OrmConfiguration.DefaultDialect;
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
        public bool IsFrozen
        {
            get
            {
                return _isFrozen;
            }
            set
            {
                if (_isFrozen && !value)
                {
                    throw new ArgumentException("The entity mappings can't have the frozen flag reset", nameof(IsFrozen));
                }
                _isFrozen = value;
            }
        }

        /// <summary>
        /// Current Sql dialect in use for the current entity.
        /// </summary>
        public SqlDialect Dialect { get; protected set; }

        /// <summary>
        /// Entity type.
        /// </summary>
        public Type EntityType { get; private set; }

        internal IDictionary<string, PropertyMapping> PropertyMappings
        {
            get
            {
                return _propertyMappings;
            }
        }

        protected void ValidateState()
        {
            if (this.IsFrozen)
            {
                throw new InvalidOperationException("No further modifications are allowed for this entity mapping. Please clone the entity mapping instead.");
            }
        }

        protected void RemoveProperties(IEnumerable<string> paramNames, bool exclude)
        {
            var propNamesMappingsToRemove = new List<string>(PropertyMappings.Count);
            propNamesMappingsToRemove.AddRange(from propMapping in this.PropertyMappings where (exclude && !paramNames.Contains(propMapping.Value.PropertyName)) || (!exclude && paramNames.Contains(propMapping.Value.PropertyName)) select propMapping.Key);

            foreach (var propName in propNamesMappingsToRemove)
            {
                this.RemoveProperty(propName);
            }
        }

        internal void RemoveProperty(string propertyName)
        {
            PropertyMappings.Remove(propertyName);
        }

        protected PropertyMapping SetPropertyInternal(string propertyName)
        {
            var propDescriptor = TypeDescriptor.GetProperties(this.EntityType).OfType<PropertyDescriptor>().Single(propInfo => propInfo.Name == propertyName);
            return this.SetPropertyInternal(propDescriptor);
        }

        protected PropertyMapping SetPropertyInternal(PropertyDescriptor property)
        {
            return this.SetPropertyInternal(new PropertyMapping(this, this.PropertyMappings.Count, property));
        }

        protected PropertyMapping SetPropertyInternal(PropertyMapping propertyMapping)
        {
            return this.PropertyMappings[propertyMapping.PropertyName] = propertyMapping;
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
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

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
            Requires.NotNull(propertySetupFct, nameof(propertySetupFct));

            var propName = ((MemberExpression)property.Body).Member.Name;
            var propMapping = this.SetPropertyInternal(propName);
            if (propertySetupFct != null)
            {
                propertySetupFct(propMapping);
            }
            return this;
        }

        /// <summary>
        /// Sets the mapping options for a property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        /// <param name="options">Column options</param>
        /// <param name="databaseColumnName">Optional database column name override.</param>
        [Obsolete("This method is marked as obsolete and will be removed in future versions.")]
        public EntityMapping<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, TProperty>> property,
            PropertyMappingOptions options,
            string databaseColumnName = null)
        {
            Requires.NotNull(property, nameof(property));

            var propName = ((MemberExpression)property.Body).Member.Name;
            return this.SetProperty(propName, options, databaseColumnName);
        }

        /// <summary>
        /// Returns all the property mappings, optionally filtered by their options.
        /// </summary>
        public PropertyMapping[] GetProperties(params PropertyMappingOptions[] includeFilter)
        {
            return this.PropertyMappings.Values
                .Where(propInfo => (includeFilter.Length == 0 || includeFilter.Any(options => (options & propInfo.Options) == options)))
                .OrderBy(propInfo => propInfo.Order)
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
                .OrderBy(propInfo => propInfo.Order)
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
                .OrderBy(propInfo => propInfo.Order)
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
            PropertyMappings.TryGetValue(propertyName.ToString(CultureInfo.InvariantCulture), out propertyMapping);
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

        /// <summary>
        /// Sets the mapping options for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        /// <param name="options">Column options</param>
        /// <param name="databaseColumnName">Optional database column name override.</param>
        [Obsolete("This method is marked as obsolete and will be removed in future versions.")]
        public EntityMapping<TEntity> SetProperty(string propertyName, PropertyMappingOptions options, string databaseColumnName = null)
        {
            this.ValidateState();
            Requires.NotNull(propertyName, nameof(propertyName));

            var propMapping = this.SetPropertyInternal(propertyName);
            if (!string.IsNullOrEmpty(databaseColumnName))
            {
                propMapping.DatabaseColumnName = databaseColumnName;
            }
            if ((options & PropertyMappingOptions.DatabaseGeneratedProperty) == PropertyMappingOptions.DatabaseGeneratedProperty)
            {
                propMapping.IsDatabaseGenerated = true;
            }
            if ((options & PropertyMappingOptions.KeyProperty) == PropertyMappingOptions.KeyProperty)
            {
                propMapping.IsPrimaryKey = true;
            }
            if ((options & PropertyMappingOptions.ExcludedFromInserts) == PropertyMappingOptions.ExcludedFromInserts)
            {
                propMapping.IsExcludedFromInserts = true;
            }
            if ((options & PropertyMappingOptions.ExcludedFromUpdates) == PropertyMappingOptions.ExcludedFromUpdates)
            {
                propMapping.IsExcludedFromUpdates = true;
            }
            if ((options & PropertyMappingOptions.ReferencingForeignEntity) == PropertyMappingOptions.ReferencingForeignEntity)
            {
                throw new NotSupportedException("It is not possible to set up foreign keys via this method.");
            }
            return this;
        }

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
