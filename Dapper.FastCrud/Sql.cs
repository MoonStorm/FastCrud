namespace Dapper.FastCrud
{
    using Dapper.FastCrud.Configuration;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Extensions;
    using Dapper.FastCrud.Formatters;
    using System;
    using System.Runtime.CompilerServices;
    using Dapper.FastCrud.Formatters.Formattables;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;
    using System.Linq.Expressions;

    /// <summary>
    /// This SQL builder can be used for mapping table and column names to their SQL counterparts.
    /// </summary>
    public static class Sql
    {
        private static readonly Lazy<EntityDescriptor> _defaultEntityRegistration = new Lazy<EntityDescriptor>(OrmConfiguration.GetEntityDescriptor<FakeEntity>);

        /// <summary>
        /// Returns a formattable SQL parameter.
        /// When used with the FastCrud's formatter, it defaults to the "P" specifier (e.g. @Param).
        /// When used with any other formatter, it defaults to the raw parameter name
        ///   but the "P" specifier is still available in this mode.
        /// </summary>
        /// <param name="sqlParameterName">A SQL parameter name. It is recommended to be passed as nameof(params.Param).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable Parameter(string sqlParameterName)
        {
            Validate.NotNullOrEmpty(sqlParameterName, nameof(sqlParameterName));

            return new FormattableParameter(
                _defaultEntityRegistration.Value,
                null, 
                sqlParameterName);
        }

        /// <summary>
        /// Returns a formattable SQL identifier.
        /// When used with the FastCrud's formatter, it defaults to the "I" specifier (e.g. [Identifier]).
        /// When used with any other formatter, it defaults to the raw identifier but the "I" specifier is still available in this mode.
        /// Do not use this method for table or column names.
        /// </summary>
        /// <param name="sqlIdentifier">An SQL identifier that is not a table or a column name. It is recommended to be passed using nameof.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable Identifier(string sqlIdentifier)
        {
            Validate.NotNullOrEmpty(sqlIdentifier, nameof(sqlIdentifier));

            return new FormattableIdentifier(
                _defaultEntityRegistration.Value, 
                null, 
                sqlIdentifier);
        }

        /// <summary>
        /// Returns a formattable database entity.
        /// When used with the FastCrud's formatter, it has no default but responds to the "T" specifier for table or alias.
        /// When used with any other formatter, it defaults to the raw alias (if provided) or the table name associated with the entity
        ///   but the "T" specifier is still available in this mode as well.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable Entity<TEntity>(string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntity(entityDescriptor, entityMappingOverride?.Registration, alias, null);
        }

        /// <summary>
        /// Returns a formattable property of a database entity.
        /// When used with the FastCrud's formatter, it has no default but responds to
        ///   the "T" specifier for table or alias or
        ///   the "C" specifier for the single column name or
        ///   the "TC" specifier for a fully qualified SQL column.
        ///   the alias notation
        /// When used with any other formatter, it defaults to the raw column name associated with the provided property
        ///   but the "C", "T" and "TC" specifiers, together with the alias notation, still work in this mode.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable Entity<TEntity>(Expression<Func<TEntity, object?>> property, string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Validate.NotNull(property, nameof(property));
            var propertyDescriptor = property.GetPropertyDescriptor();

            return EntityProperty<TEntity>(propertyDescriptor.Name, alias, entityMappingOverride, null);
        }

        /// <summary>
        /// Returns a formattable database table associated with an entity.
        /// For consistency, it is recommended to use <see cref="Entity{TEntity}(string?,Dapper.FastCrud.Mappings.EntityMapping{TEntity}?)"/> instead.
        /// Irrespective of the formatter used, it defaults to the "T" specifier for table or alias.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [Obsolete("Recommended to use Entity<TEntity> with a proper format specifier.", error:false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable Table<TEntity>(string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntity(
                entityDescriptor, 
                entityMappingOverride?.Registration, 
                alias, FormatSpecifiers.TableOrAlias);
        }

        /// <summary>
        /// Returns a formattable database column associated with a property.
        /// For consistency, it is recommended to use <see cref="Entity{TEntity}(System.Linq.Expressions.Expression{System.Func{TEntity,object?}},string?,Dapper.FastCrud.Mappings.EntityMapping{TEntity}?)"/> instead.
        /// Irrespective of the formatter used, it defaults to the "C" specifier, however when using with the FastCrud's formatter it also responds to 
        ///   the "T" specifier for table or alias or
        ///   the "TC" specifier for a fully qualified SQL column.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="propertyName">The name of the property. It is recommended to use nameof to provide this value.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [Obsolete("Recommended to use Entity<TEntity>(entity => entity.prop) with a proper format specifier.", error: false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable Column<TEntity>(string propertyName, string alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Validate.NotNullOrEmpty(propertyName, nameof(propertyName));
            return EntityProperty<TEntity>(propertyName, alias, entityMappingOverride, FormatSpecifiers.SingleColumn);
        }

        /// <summary>
        /// Returns a formattable database column associated with a property.
        /// For consistency, it is recommended to use <see cref="Entity{TEntity}(System.Linq.Expressions.Expression{System.Func{TEntity,object?}},string?,Dapper.FastCrud.Mappings.EntityMapping{TEntity}?)"/> instead.
        /// Irrespective of the formatter used, it defaults to the "C" specifier, however when using with the FastCrud's formatter it also responds to 
        ///   the "T" specifier for table or alias or
        ///   the "TC" specifier for a fully qualified SQL column.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="property">The property of the entity mapped to a column.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [Obsolete("Recommended to use Entity<TEntity>(entity => entity.prop) with a proper format specifier.", error: false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable Column<TEntity>(Expression<Func<TEntity, object?>> property, string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Validate.NotNull(property, nameof(property));
            return EntityProperty<TEntity>(property.GetPropertyDescriptor().Name, alias, entityMappingOverride, FormatSpecifiers.SingleColumn);
        }

        /// <summary>
        /// Returns a fully qualified formattable database column associated with a property.
        /// For consistency, it is recommended to use <see cref="Entity{TEntity}(System.Linq.Expressions.Expression{System.Func{TEntity,object?}},string?,Dapper.FastCrud.Mappings.EntityMapping{TEntity}?)"/> instead.
        /// Irrespective of the formatter used, it defaults to the "TC" specifier, however when using with the FastCrud's formatter it also responds to 
        ///   the "T" specifier for table or alias or
        ///   the "C" specifier for a single column.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="propertyName">The name of the property. It is recommended to use nameof to provide this value.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [Obsolete("Recommended to use Entity<TEntity>(entity => entity.prop) with a proper format specifier.", error: false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable TableAndColumn<TEntity>(string propertyName, string? alias = null, EntityMapping<TEntity> entityMappingOverride = null)
        {
            Validate.NotNullOrEmpty(propertyName, nameof(propertyName));

            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(entityDescriptor, entityMappingOverride?.Registration, propertyName, alias, FormatSpecifiers.FullyQualifiedColumn);
        }

        /// <summary>
        /// Returns a fully qualified formattable database column associated with a property.
        /// For consistency, it is recommended to use <see cref="Entity{TEntity}(System.Linq.Expressions.Expression{System.Func{TEntity,object?}},string?,Dapper.FastCrud.Mappings.EntityMapping{TEntity}?)"/> instead.
        /// Irrespective of the formatter used, it defaults to the "TC" specifier, however when using with the FastCrud's formatter it also responds to 
        ///   the "T" specifier for table or alias or
        ///   the "C" specifier for a single column.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="property">The property of the entity mapped to a column.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [Obsolete("Recommended to use Entity<TEntity>(entity => entity.prop) with a proper format specifier.", error: false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Formattable TableAndColumn<TEntity>(Expression<Func<TEntity, object?>> property, string? alias = null, EntityMapping<TEntity> entityMappingOverride = null)
        {
            Validate.NotNull(property, nameof(property));

            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(
                entityDescriptor,
                entityMappingOverride?.Registration,
                property.GetPropertyDescriptor().Name,
                alias,
                FormatSpecifiers.FullyQualifiedColumn);
        }

        /// <summary>
        /// Formats a formattable string, using the provided entity as the main entity resolver.
        /// </summary>
        /// <param name="stringToFormat">The formattable string to format.</param>
        /// <param name="entityMappingOverride">An optional entity mapping override, if different than the default one.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format<TEntity>(FormattableString stringToFormat, EntityMapping<TEntity> entityMappingOverride = null)
        {
            // TODO: expose via a fluent option builder pattern all the options supported by the formatter.
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            var formatter = new GenericSqlStatementFormatter();
            var resolver = formatter.RegisterResolver(entityDescriptor, entityMappingOverride?.Registration, null);
            formatter.SetActiveMainResolver(resolver, false);
            return stringToFormat.ToString(formatter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Formattable EntityProperty<TEntity>(string propertyName, string? alias, EntityMapping<TEntity>? entityMappingOverride, string? legacyDefaultFormatSpecifierOutsideOurFormatter)
        {
            Validate.NotNullOrEmpty(propertyName, nameof(propertyName));
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(entityDescriptor, entityMappingOverride?.Registration, propertyName, alias, legacyDefaultFormatSpecifierOutsideOurFormatter);
        }
    }
}
