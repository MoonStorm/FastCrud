namespace Dapper.FastCrud
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Extensions;
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
        /// By default, its ToString method returns the provided parameter name in its raw form,
        ///   but it can be used with the "P" format specifier in a SQL statement as @Param.
        /// </summary>
        /// <param name="sqlParameterName">A SQL parameter name. It is recommended to be passed as nameof(params.Param).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Parameter(string sqlParameterName)
        {
            Requires.NotNullOrEmpty(sqlParameterName, nameof(sqlParameterName));

            return new FormattableParameter(_defaultEntityRegistration.Value, null, sqlParameterName);
        }

        /// <summary>
        /// Returns a formattable SQL identifier.
        /// By default, its ToString method returns the provided identifier in its raw form,
        ///   but it can be used with the "I" format specifier in a SQL statement where it will receive the delimiters specific to the dialect.
        /// Do not use this method for table or column names.
        /// </summary>
        /// <param name="sqlIdentifier">An SQL identifier that is not a table or a column name. It is recommended to be passed using nameof.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Identifier(string sqlIdentifier)
        {
            Requires.NotNullOrEmpty(sqlIdentifier, nameof(sqlIdentifier));

            return new FormattableIdentifier(_defaultEntityRegistration.Value, null, sqlIdentifier);
        }

        /// <summary>
        /// Returns a formattable database entity.
        /// By default, its ToString method returns the mapped table name in its raw form (or the alias if passed as an argument in this method),
        ///   but it can be used with the "T" format specifier in a SQL statement where it gets fully qualified.
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
        /// Returns a formattable database entity property.
        /// By default, its ToString method returns the mapped database column name in its raw form, but it can be used with
        ///   the "C" format specifier in a SQL statement where it will return the column name wrapped in delimiters OR with 
        ///   the "TC" format specifier where the column name will get qualified either by the alias (if provided) or the table name.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable EntityProperty<TEntity>(Expression<Func<TEntity, object>> property, string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Requires.NotNull(property, nameof(property));
            var propertyDescriptor = property.GetPropertyDescriptor();

            return EntityProperty<TEntity>(propertyDescriptor.Name, alias, entityMappingOverride);
        }

        /// <summary>
        /// Returns a formattable database entity property.
        /// By default, its ToString method returns the mapped database column name in its raw form, but it can be used with
        ///   the "C" format specifier in a SQL statement where it will return the column name OR with 
        ///   the "TC" format specifier where the column name will get qualified either by the alias (if provided) or the table name, all wrapped in delimiters.
        /// </summary>
        /// <param name="alias">An alias to be used instead of the table name.</param>
        /// <param name="entityMappingOverride">An optional override to the default entity mapping.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IFormattable EntityProperty<TEntity>(string propertyName, string? alias = null, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Requires.NotNullOrEmpty(propertyName,nameof(propertyName));
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(entityDescriptor, entityMappingOverride?.Registration, propertyName, alias);
        }

        [Obsolete("This method will be removed in a future version. Use Entity instead.", error: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Table<TEntity>(EntityMapping<TEntity>? entityMappingOverride = null)
        {
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntity(entityDescriptor, entityMappingOverride?.Registration, null, "T");
        }

        [Obsolete("This method will be removed in a future version. Use EntityProperty instead.", error: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Column<TEntity>(string propertyName, EntityMapping<TEntity>? entityMappingOverride = null)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));

            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(entityDescriptor, entityMappingOverride?.Registration, propertyName, null, "C");
        }

        [Obsolete("This method will be removed in a future version. Use EntityProperty instead.", error: true)]
        public static IFormattable TableAndColumn<TEntity>(string propertyName, EntityMapping<TEntity> entityMappingOverride = null)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));

            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            return new FormattableEntityProperty(entityDescriptor, entityMappingOverride?.Registration, propertyName, null, "TC");
        }

        /// <summary>
        /// Used in cases where we don't really need an entity, just a random SQL builder.
        /// </summary>
        private class FakeEntity
        {
        }
    }
}
