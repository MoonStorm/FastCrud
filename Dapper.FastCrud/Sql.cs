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
        /// When used with the Dapper FastCrud's formatter, it defaults to the "P" specifier (e.g. @Param).
        /// When used with any other formatter, it defaults to the raw parameter name
        ///   but the "P" specifier is still available in this mode.
        /// </summary>
        /// <param name="sqlParameterName">A SQL parameter name. It is recommended to be passed as nameof(params.Param).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Parameter(string sqlParameterName)
        {
            Requires.NotNullOrEmpty(sqlParameterName, nameof(sqlParameterName));

            return new FormattableParameter(
                _defaultEntityRegistration.Value,
                null, 
                sqlParameterName,
                FormatSpecifiers.Parameter);
        }

        /// <summary>
        /// Returns a formattable SQL identifier.
        /// When used with the Dapper FastCrud's formatter, it defaults to the "I" specifier (e.g. "Identifier").
        /// When used with any other formatter, it defaults to the raw identifier but the "I" specifier is still available in this mode.
        /// Do not use this method for table or column names.
        /// </summary>
        /// <param name="sqlIdentifier">An SQL identifier that is not a table or a column name. It is recommended to be passed using nameof.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Identifier(string sqlIdentifier)
        {
            Requires.NotNullOrEmpty(sqlIdentifier, nameof(sqlIdentifier));

            return new FormattableIdentifier(
                _defaultEntityRegistration.Value, 
                null, 
                sqlIdentifier,
                FormatSpecifiers.Identifier);
        }

        /// <summary>
        /// Returns a formattable database entity.
        /// When used with the Dapper FastCrud's formatter, it has no default but responds to the "T" specifier for table or alias.
        /// When used with any other formatter, it defaults to the raw alias (if provided) or the table name associated with the entity
        ///   but the "T" specifier is still available in this mode as well.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Entity<TEntity>(string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntity(entityDescriptor, entityMappingOverride?.Registration, alias);
        }

        /// <summary>
        /// Returns a formattable property of a database entity.
        /// When used with the Dapper FastCrud's formatter, it has no default but responds to
        ///   the "T" specifier for table or alias or
        ///   the "C" specifier for the single column name or
        ///   the "TC" specifier for a fully qualified SQL column.
        /// When used with any other formatter, it defaults to the raw column name associated with the provided property
        ///   but the "C", "T" and "TC" specifiers still work in this mode.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable EntityProperty<TEntity>(Expression<Func<TEntity, object?>> property, string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Requires.NotNull(property, nameof(property));
            var propertyDescriptor = property.GetPropertyDescriptor();

            return EntityProperty<TEntity>(propertyDescriptor.Name, alias, entityMappingOverride, null);
        }

        /// <summary>
        /// Returns a formattable database table associated with an entity.
        /// For consistency, it is recommended to use <see cref="Entity{TEntity}"/> instead.
        /// When used with the Dapper FastCrud's formatter, it defaults to the "T" specifier for table or alias.
        /// When used with any other formatter, it defaults to the raw alias (if provided) or the table name associated with the entity
        ///   but the "T" specifier is still available in this mode as well.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Table<TEntity>(string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntity(
                entityDescriptor, 
                entityMappingOverride?.Registration, 
                alias, FormatSpecifiers.TableOrAlias);
        }

        /// <summary>
        /// Returns a formattable property of a database entity.
        /// For consistency, it is recommended to use <see cref="EntityProperty{TEntity}"/> instead.
        /// When used with the Dapper FastCrud's formatter, it defaults to the "C" specifier for the single column name but also responds to
        ///   the "T" specifier for table or alias or
        ///   the "TC" specifier for a fully qualified SQL column.
        /// When used with any other formatter, it defaults to the raw column name associated with the provided property
        ///   but the "C", "T" and "TC" specifiers still work in this mode.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="propertyName">The name of the property. It is recommended to use nameof to provide this value.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [Obsolete(message:"Use the typed method instead. This method will be removed in a future version.", error:false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Column<TEntity>(string propertyName, string alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));
            return EntityProperty<TEntity>(propertyName, alias, entityMappingOverride, FormatSpecifiers.SingleColumn);
        }

        /// <summary>
        /// Returns a formattable property of a database entity.
        /// For consistency, it is recommended to use <see cref="EntityProperty{TEntity}"/> instead.
        /// When used with the Dapper FastCrud's formatter, it defaults to the "C" specifier for the single column name but also responds to
        ///   the "T" specifier for table or alias or
        ///   the "TC" specifier for a fully qualified SQL column.
        /// When used with any other formatter, it defaults to the raw column name associated with the provided property
        ///   but the "C", "T" and "TC" specifiers still work in this mode.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="property">The property of the entity mapped to a column.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Column<TEntity>(Expression<Func<TEntity, object?>> property, string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Requires.NotNull(property, nameof(property));
            return EntityProperty<TEntity>(property.GetPropertyDescriptor().Name, alias, entityMappingOverride, FormatSpecifiers.SingleColumn);
        }

        /// <summary>
        /// Returns a formattable property of a database entity.
        /// For consistency, it is recommended to use <see cref="EntityProperty{TEntity}"/> instead.
        /// When used with the Dapper FastCrud's formatter, it defaults to the "TC" for a SQL qualified column name but it also responds to 
        ///   the "T" specifier for table or alias or
        ///   the "C" specifier for the single column name or
        /// When used with any other formatter, it defaults to the raw column name associated with the provided property
        ///   but the "C", "T" and "TC" specifiers still work in this mode.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="propertyName">The name of the property. It is recommended to use nameof to provide this value.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [Obsolete(message: "Use the typed method instead. This method will be removed in a future version.", error: false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable TableAndColumn<TEntity>(string propertyName, string? alias = null, EntityMapping<TEntity> entityMappingOverride = null)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));

            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(entityDescriptor, entityMappingOverride?.Registration, propertyName, alias, FormatSpecifiers.TableOrAliasWithColumn);
        }

        /// <summary>
        /// Returns a formattable property of a database entity.
        /// For consistency, it is recommended to use <see cref="EntityProperty{TEntity}"/> instead.
        /// When used with the Dapper FastCrud's formatter, it defaults to the "TC" for a SQL qualified column name but it also responds to 
        ///   the "T" specifier for table or alias or
        ///   the "C" specifier for the single column name or
        /// When used with any other formatter, it defaults to the raw column name associated with the provided property
        ///   but the "C", "T" and "TC" specifiers still work in this mode.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="property">The property of the entity mapped to a column.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable TableAndColumn<TEntity>(Expression<Func<TEntity, object?>> property, string? alias = null, EntityMapping<TEntity> entityMappingOverride = null)
        {
            Requires.NotNull(property, nameof(property));

            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(
                entityDescriptor,
                entityMappingOverride?.Registration,
                property.GetPropertyDescriptor().Name,
                alias,
                FormatSpecifiers.TableOrAliasWithColumn);
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
        private static IFormattable EntityProperty<TEntity>(string propertyName, string? alias, EntityMapping<TEntity>? entityMappingOverride, string? defaultSpecifier)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(entityDescriptor, entityMappingOverride?.Registration, propertyName, alias, defaultSpecifier);
        }
    }
}
