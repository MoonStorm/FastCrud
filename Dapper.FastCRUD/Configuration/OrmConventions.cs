namespace Dapper.FastCrud.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Dapper.FastCrud.Configuration.DialectOptions;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Default conventions used by the library.
    /// </summary>
    public class OrmConventions
    {
        private readonly List<Tuple<Regex, string>> _pluralRegexMatchesConversion = new List<Tuple<Regex, string>>();

        private static readonly SqlDatabaseOptions _defaultMsSqlDatabaseOptions = new MsSqlDatabaseOptions();
        private static readonly SqlDatabaseOptions _defaultMySqlDatabaseOptions = new MySqlDatabaseOptions();
        private static readonly SqlDatabaseOptions _defaultPostgreSqlDatabaseOptions = new PostreSqlDatabaseOptions();
        private static readonly SqlDatabaseOptions _defaultGenericSqlDatabaseOptions = new SqlDatabaseOptions();

        private static readonly Type[] _simpleSqlTypes = new[]
        {
            typeof (byte),
            typeof (sbyte),
            typeof (short),
            typeof (ushort),
            typeof (int),
            typeof (uint),
            typeof (long),
            typeof (ulong),
            typeof (float),
            typeof (double),
            typeof (decimal),
            typeof (bool),
            typeof (string),
            typeof (char),
            typeof (Guid),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (byte[])
        };


        /// <summary>
        /// Default constructor
        /// </summary>
        protected internal OrmConventions()
        {
            // to avoid adding another dependency, we'll try to add the most used pluralization forms
            // borrowed from https://github.com/MehdiK/Humanizer/blob/cbe0495d258bee13a76f9be535017d8677554d6a/src/Humanizer/Inflections/Vocabularies.cs
            this.AddEntityToTableNameConversionRule("$", "s");
            this.AddEntityToTableNameConversionRule("s$", "s");
            this.AddEntityToTableNameConversionRule("(ax|test)is$", "$1es");
            this.AddEntityToTableNameConversionRule("(octop|vir|alumn|fung)us$", "$1i");
            this.AddEntityToTableNameConversionRule("(alias|status)$", "$1es");
            this.AddEntityToTableNameConversionRule("(bu)s$", "$1ses");
            this.AddEntityToTableNameConversionRule("(buffal|tomat|volcan)o$", "$1oes");
            this.AddEntityToTableNameConversionRule("([ti])um$", "$1a");
            this.AddEntityToTableNameConversionRule("sis$", "ses");
            this.AddEntityToTableNameConversionRule("(?:([^f])fe|([lr])f)$", "$1$2ves");
            this.AddEntityToTableNameConversionRule("(hive)$", "$1s");
            this.AddEntityToTableNameConversionRule("([^aeiouy]|qu)y$", "$1ies");
            this.AddEntityToTableNameConversionRule("(x|ch|ss|sh)$", "$1es");
            this.AddEntityToTableNameConversionRule("(matr|vert|ind)ix|ex$", "$1ices");
            this.AddEntityToTableNameConversionRule("([m|l])ouse$", "$1ice");
            this.AddEntityToTableNameConversionRule("^(ox)$", "$1en");
            this.AddEntityToTableNameConversionRule("(quiz)$", "$1zes");
            this.AddEntityToTableNameConversionRule("(campus)$", "$1es");
            this.AddEntityToTableNameConversionRule("^is$", "are");
        }

        /// <summary>
        /// Resolves an entity type name into a sql table name. 
        /// </summary>
        public virtual string GetTableName(Type entityType)
        {
            TableAttribute tableNameAttribute;
            if ((tableNameAttribute = this.GetEntityAttributes(entityType).OfType<TableAttribute>().FirstOrDefault()) != null)
            {
                return tableNameAttribute.Name;
            }

            var entityTypeName = entityType.Name;
            foreach (var tableNameConversionRule in _pluralRegexMatchesConversion)
            {
                var sqlTableName = tableNameConversionRule.Item1.Replace(entityTypeName, tableNameConversionRule.Item2);
                if (!ReferenceEquals(sqlTableName, entityTypeName))
                {
                    return sqlTableName;
                }
            }

            return entityTypeName;
        }

        /// <summary>
        /// Returns the schema name for an entity type. It can return null.
        /// In order for the schema to be used, you must also ensure that <see cref="SqlDatabaseOptions.IsUsingSchemas"/> for the dialect also returns <c>true</c>.
        /// </summary>
        public virtual string GetSchemaName(Type entityType)
        {
            return this.GetEntityAttributes(entityType).OfType<TableAttribute>().FirstOrDefault()?.Schema;
        }

        /// <summary>
        /// Returns various database specific options to be used by the sql builder for the specified dialect.
        /// </summary>
        public virtual SqlDatabaseOptions GetDatabaseOptions(SqlDialect dialect)
        {
            switch (dialect)
            {
                case SqlDialect.MsSql:
                    return _defaultMsSqlDatabaseOptions;
                case SqlDialect.PostgreSql:
                    return _defaultPostgreSqlDatabaseOptions;
                case SqlDialect.MySql:
                    return _defaultMySqlDatabaseOptions;
                default:
                    return _defaultGenericSqlDatabaseOptions;
            }
        }

        /// <summary>
        /// Gets the entity properties mapped to database columns.
        /// </summary>
        public virtual IEnumerable<PropertyDescriptor> GetEntityProperties(Type entityType)
        {
            return TypeDescriptor.GetProperties(entityType)
                .OfType<PropertyDescriptor>()
                .Where(propDesc => 
                    !propDesc.Attributes.OfType<NotMappedAttribute>().Any()
                    && !propDesc.IsReadOnly 
                    && propDesc.Attributes.OfType<EditableAttribute>().All(editableAttr => editableAttr.AllowEdit)
                    && this.IsSimpleSqlType(propDesc.PropertyType));
        }

        /// <summary>
        ///  Sets up an entity property mapping.
        /// </summary>
        public virtual void ConfigureEntityPropertyMapping(PropertyMapping propertyMapping)
        {
            // set the Id property to be the primary database generated key in case we don't find any orm attributes on the entity or on the properties
            if(string.Equals(propertyMapping.PropertyName, "id", StringComparison.OrdinalIgnoreCase)
                && this.GetEntityAttributes(propertyMapping.EntityMapping.EntityType).Length == 0
                && !this.GetEntityProperties(propertyMapping.EntityMapping.EntityType).Any(
                    propDesc => this.GetEntityPropertyAttributes(propertyMapping.EntityMapping.EntityType, propDesc).Any(
                        propAttr => propAttr is ColumnAttribute || propAttr is KeyAttribute || propAttr is DatabaseGeneratedAttribute)))
            {
                propertyMapping.SetPrimaryKey();
                propertyMapping.ExcludeFromInserts();
                propertyMapping.RefreshOnInserts();
                return;
            }

             //|| (propDesc.PropertyType.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(propDesc.PropertyType.GetGenericTypeDefinition()))))

            // solve the parent child relationships
            //if (propertyMapping.Descriptor.PropertyType.IsGenericType
            //    && typeof(IEnumerable<>).IsAssignableFrom(propertyMapping.Descriptor.PropertyType.GetGenericTypeDefinition()))
            //{
            //    var referencedType = propertyMapping.Descriptor.PropertyType.GetGenericArguments()[0];
            //    propertyMapping.SetParentChildRelationship(referencedType);
            //    return;
            //}

            var propertyAttributes = this.GetEntityPropertyAttributes(propertyMapping.EntityMapping.EntityType, propertyMapping.Descriptor);

            var columnAttribute = propertyAttributes.OfType<ColumnAttribute>().FirstOrDefault();

            var databaseColumnName = columnAttribute?.Name;
            if (!string.IsNullOrEmpty(databaseColumnName))
            {
                propertyMapping.SetDatabaseColumnName(databaseColumnName);
            }

            // used for matching relationships
            var databaseColumnOrder = columnAttribute?.Order;
            if (databaseColumnOrder.HasValue)
            {
                propertyMapping.ColumnOrder = databaseColumnOrder.Value;
            }

            if (propertyAttributes.OfType<KeyAttribute>().Any())
            {
                propertyMapping.SetPrimaryKey();
            }

            if (propertyAttributes.OfType<DatabaseGeneratedDefaultValueAttribute>().Any())
            {
                propertyMapping.ExcludeFromInserts();
                propertyMapping.RefreshOnInserts();
            }

            var databaseGeneratedAttributes = propertyAttributes.OfType<DatabaseGeneratedAttribute>();
            // https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.schema.databasegeneratedoption(v=vs.110).aspx
            if (databaseGeneratedAttributes.Any(dbGenerated => dbGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity))
            {
                propertyMapping.SetDatabaseGenerated(DatabaseGeneratedOption.Identity);
            }

            if (databaseGeneratedAttributes.Any(dbGenerated => dbGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed))
            {
                propertyMapping.SetDatabaseGenerated(DatabaseGeneratedOption.Computed);
            }

            var foreignKeyAttribute = propertyAttributes.OfType<ForeignKeyAttribute>().FirstOrDefault();
            if (foreignKeyAttribute != null)
            {
                var referencingTypePropertyDescriptor = TypeDescriptor.GetProperties(propertyMapping.Descriptor.ComponentType)
                                                                                      .OfType<PropertyDescriptor>()
                                                                                      .Single(propDescriptor => propDescriptor.Name == foreignKeyAttribute.Name);
                propertyMapping.SetChildParentRelationship(referencingTypePropertyDescriptor.PropertyType, referencingTypePropertyDescriptor.Name);
            }
        }

        /// <summary>
        /// Return true if an entity property of the given type should be considered for a database mapping.
        /// </summary>
        protected virtual bool IsSimpleSqlType(Type propertyType)
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            propertyType = underlyingType ?? propertyType;
            return
#if COREFX
                propertyType.GetTypeInfo().IsEnum
#else
                propertyType.IsEnum 
#endif
            || _simpleSqlTypes.Contains(propertyType);
        }

        /// <summary>
        /// Clears all the entity to table name conventions.
        /// </summary>
        protected void ClearEntityToTableNameConversionRules()
        {
            _pluralRegexMatchesConversion.Clear();
        }

        /// <summary>
        /// Adds a new rule used for converting an entity class name into a table name.
        /// The rule will be added with the highest priority.
        /// </summary>
        /// <param name="classNameRegex">The regex that will have to match the class name (e.g. "(buffal|tomat|volcan)o$" ) </param>
        /// <param name="sqlTableNameMatchReplacement">The match used to form the sql table name (e.g. "$1oes")</param>
        protected void AddEntityToTableNameConversionRule(string classNameRegex, string sqlTableNameMatchReplacement)
        {
            _pluralRegexMatchesConversion.Insert(
                0,
                new Tuple<Regex, string>(
                    new Regex(classNameRegex, RegexOptions.Compiled),
                    sqlTableNameMatchReplacement));
        }

        private Attribute[] GetEntityAttributes(Type entityType)
        {
            var entityAttributes = TypeDescriptor.GetAttributes(entityType).OfType<Attribute>().ToArray();
#if COREFX
            return entityAttributes;
#else
            var entityMetadataAttribute = entityAttributes.OfType<MetadataTypeAttribute>().FirstOrDefault();
            var entityMetadataAttributes = entityMetadataAttribute == null
                                               ? new Attribute[0]
                                               : TypeDescriptor.GetAttributes(entityMetadataAttribute.MetadataClassType).OfType<Attribute>();
            return entityMetadataAttributes.Concat(entityAttributes).ToArray();
#endif
        }

        private Attribute[] GetEntityPropertyAttributes(Type entityType, PropertyDescriptor property)
        {
            var entityPropertyAttributes = property.Attributes.OfType<Attribute>().ToArray();
#if COREFX
            return entityPropertyAttributes;
#else

            var entityMetadataAttribute = TypeDescriptor.GetAttributes(entityType).OfType<MetadataTypeAttribute>().FirstOrDefault();
            var entityMetadataProperty = entityMetadataAttribute == null
                                               ? null
                                               : (TypeDescriptor
                                                    .GetProperties(entityMetadataAttribute.MetadataClassType).OfType<PropertyDescriptor>()
                                                ).FirstOrDefault(propDescriptor => propDescriptor.Name==property.Name);
            var entityMetadataPropertyAttributes = entityMetadataProperty?.Attributes.OfType<Attribute>() ?? new Attribute[0];

            return entityMetadataPropertyAttributes.Concat(entityPropertyAttributes).ToArray();
#endif
        }
    }
}
