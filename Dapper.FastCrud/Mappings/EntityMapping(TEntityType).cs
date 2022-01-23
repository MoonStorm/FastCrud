namespace Dapper.FastCrud.Mappings
{
    using Dapper.FastCrud.Validations;
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Used to easily register entities.
    /// Multiple instances of mappings for a single entity can be active at any time for a single entity type,
    /// but also multiple entities pointing to the same database table having different mappings.
    /// </summary>
    public class EntityMapping<TEntity>
    {
        private readonly EntityMapping _entityRegistration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal EntityMapping(EntityMapping entityRegistration)
        {
            Requires.NotNull(entityRegistration, nameof(entityRegistration));
            _entityRegistration = entityRegistration;
        }

        /// <summary>
        /// Returns the underlying entity registration.
        /// </summary>
        internal EntityMapping Registration => _entityRegistration;

        /// <summary>
        /// Sets the database table associated with your entity.
        /// </summary>
        /// <param name="tableName">Table name</param>
        public EntityMapping<TEntity> SetTableName(string tableName)
        {
            Requires.NotNullOrWhiteSpace(tableName, nameof(tableName));
            _entityRegistration.TableName = tableName;
            return this;
        }

        /// <summary>
        /// Sets or resets the database schema associated with your entity.
        /// </summary>
        /// <param name="schemaName">Schema name</param>
        public EntityMapping<TEntity> SetSchemaName(string? schemaName)
        {
            _entityRegistration.SchemaName = schemaName;
            return this;
        }

        /// <summary>
        /// Sets or resets the database associated with your entity.
        /// </summary>
        /// <param name="databaseName">Database name</param>
        public EntityMapping<TEntity> SetDatabaseName(string? databaseName)
        {
            _entityRegistration.DatabaseName = databaseName;
            return this;
        }

        /// <summary>
        /// You can override the default dialect used for the schema.
        /// However, if plan on using the same dialect for all your db operations, it's best to use <see cref="OrmConfiguration.DefaultDialect"/> instead.
        /// </summary>
        /// <param name="dialect">Sql dialect</param>
        public EntityMapping<TEntity> SetDialect(SqlDialect dialect)
        {
            _entityRegistration.Dialect = dialect;
            return this;
        }

        /// <summary>
        /// Registers a regular property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        public EntityMapping<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            return this.SetProperty(property, null);
        }

        /// <summary>
        /// Sets the mapping options for a property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        /// <param name="propertySetupFct">A callback which will be called for setting up the property mapping.</param>
        public EntityMapping<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, TProperty>> property,
            Action<PropertyMapping<TEntity>>? propertySetupFct)
        {
            Requires.NotNull(property, nameof(property));

            var propName = ((MemberExpression)property.Body).Member.Name;
            var propMapping = _entityRegistration.SetProperty(propName);
            if (propertySetupFct != null)
            {
                propertySetupFct(new PropertyMapping<TEntity>(propMapping));
            }
            return this;
        }

        /// <summary>
        /// Returns all the property mappings, optionally filtered by their options.
        /// </summary>
        public PropertyMapping<TEntity>[] GetProperties(params PropertyMappingOptions[] includeFilter)
        {
            return _entityRegistration.PropertyMappings.Values
                .Where(propInfo => (includeFilter.Length == 0 || includeFilter.Any(options => (options & propInfo.Options) == options)))
                //.OrderBy(propInfo => propInfo.Order)
                .Select(propMapping => new PropertyMapping<TEntity>(propMapping))
                .ToArray();
        }

        /// <summary>
        /// Gives an option for updating all the property mappings, optionally filtered by their options.
        /// </summary>
        public EntityMapping<TEntity> UpdateProperties(Action<PropertyMapping<TEntity>> updateFct, params PropertyMappingOptions[] includeFilter)
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
        public PropertyMapping<TEntity>[] GetPropertiesExcluding(params PropertyMappingOptions[] excludeFilter)
        {
            return _entityRegistration.PropertyMappings.Values
                .Where(propInfo => (excludeFilter.Length == 0 || excludeFilter.All(options => (options & propInfo.Options) != options)))
                //.OrderBy(propInfo => propInfo.Order)
                .Select(propMapping => new PropertyMapping<TEntity>(propMapping))
                .ToArray();
        }

        /// <summary>
        /// Gives an option for updating all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public EntityMapping<TEntity> UpdatePropertiesExcluding(Action<PropertyMapping<TEntity>> updateFct, params PropertyMappingOptions[] excludeFilter)
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
        public PropertyMapping<TEntity>[] GetPropertiesExcluding(params string[] propNames)
        {
            return _entityRegistration.PropertyMappings.Values
                .Where(propInfo => (propNames.Length == 0 || !propNames.Contains(propInfo.PropertyName)))
                //.OrderBy(propInfo => propInfo.Order)
                .Select(propMapping => new PropertyMapping<TEntity>(propMapping))
                .ToArray();
        }

        /// <summary>
        /// Returns all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public EntityMapping<TEntity> UpdatePropertiesExcluding(Action<PropertyMapping<TEntity>> updateFct, params string[] propNames)
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
        public PropertyMapping<TEntity> GetProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            var propName = ((MemberExpression)property.Body).Member.Name;
            return this.GetProperty(propName);
        }

        /// <summary>
        /// Returns property mapping information for a particular property.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public PropertyMapping<TEntity> GetProperty(string propertyName)
        {
            var propMapping = _entityRegistration.PropertyMappings[propertyName];
            return new PropertyMapping<TEntity>(propMapping);
        }

        /// <summary>
        /// Removes the mapping for a property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        public EntityMapping<TEntity> RemoveProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            var propName = ((MemberExpression)property.Body).Member.Name;
            _entityRegistration.RemoveProperty(propName);
            return this;
        }

        /// <summary>
        /// Removes the mapping for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public EntityMapping<TEntity> RemoveProperty(params string[] propertyName)
        {
            _entityRegistration.RemoveProperties(propertyName, false);
            return this;
        }

        /// <summary>
        /// Removes all the property mappings with the exception of the provided list.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public EntityMapping<TEntity> RemoveAllPropertiesExcluding(params string[] propertyName)
        {
            _entityRegistration.RemoveProperties(propertyName, true);
            return this;
        }

        /// <summary>
        /// Clones the current mapping set, allowing for further modifications.
        /// </summary>
        public EntityMapping<TEntity> Clone()
        {
            return new EntityMapping<TEntity>(_entityRegistration.Clone());
        }
    }
}
