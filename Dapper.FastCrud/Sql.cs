namespace Dapper.FastCrud
{
    using System;
    using System.Runtime.CompilerServices;
    using Dapper.FastCrud.Formatters;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// This SQL builder can be used for mapping table and column names to their SQL counterparts.
    /// </summary>
    public static class Sql
    {
        /// <summary>
        /// Returns a formatter for an SQL parameter, allowing it to be properly formatted.
        /// </summary>
        /// <param name="sqlParameterName">An SQL identifier that is not a table or a column name.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Parameter(string sqlParameterName)
        {
            Requires.NotNullOrEmpty(sqlParameterName, nameof(sqlParameterName));
            return new SqlParameterFormatter(SqlParameterElementType.Parameter, sqlParameterName, null);
        }

        /// <summary>
        /// Returns a parameter formatter for an SQL identifier, allowing it to be properly formatted by adding delimiters.
        /// Do not use this method for table or column names.
        /// </summary>
        /// <param name="sqlIdentifier">An SQL identifier that is not a table or a column name.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Identifier(string sqlIdentifier)
        {
            Requires.NotNullOrEmpty(sqlIdentifier, nameof(sqlIdentifier));
            return new SqlParameterFormatter(SqlParameterElementType.Identifier, sqlIdentifier, null);
        }

        /// <summary>
        /// Returns a parameter formatter for the SQL table name of the main entity.
        /// If you wish to resolve the table name of another entity, please use the generic overload instead.
        /// The resolver can be used as-is in a formattable string expression.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Table()
        {
            return new SqlParameterFormatter(SqlParameterElementType.Table, null, null);
        }

        /// <summary>
        /// Returns a parameter formatter for the SQL table name of the provided entity.
        /// If you wish to resolve the table name of the main entity, please use the non-generic overload instead.
        /// The resolver can be used as-is in a formattable string expression.
        /// </summary>
        /// <param name="entityMappingOverride">Overrides the entity mapping used in the query method.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Table<TEntity>(EntityMapping<TEntity> entityMappingOverride)
        {
            return new SqlEntityFormattableParameter<TEntity>(SqlParameterElementType.Table, null, entityMappingOverride.Registration);
        }

        /// <summary>
        /// Returns a parameter formatter for the name of the database column attached to the specified property of the main entity.
        /// If you wish to resolve the column name of another entity, please use the generic overload instead.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. <code>nameof(entity.propname)</code>)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Column(string propertyName)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));
            return new SqlParameterFormatter(SqlParameterElementType.Column, propertyName, null);
        }

        /// <summary>
        /// Returns a parameter formatter for the name of the database column attached to the specified property of the main entity.
        /// If you wish to resolve the column name of another entity, please use the generic overload instead.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. <code>nameof(entity.propname)</code>)</param>
        /// <param name="entityMappingOverride">Overrides the entity mapping used in the query method.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable Column<TEntity>(string propertyName, EntityMapping<TEntity> entityMappingOverride)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));
            return new SqlEntityFormattableParameter<TEntity>(SqlParameterElementType.Column, propertyName, entityMappingOverride.Registration);
        }

        /// <summary>
        /// Returns a parameter formatter for the combined table and column attached to the specified property of the main entity.
        /// If you wish to resolve the column name of another entity, please use the generic overload instead.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. <code>nameof(entity.propname)</code>)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable TableAndColumn(string propertyName)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));
            return new SqlParameterFormatter(SqlParameterElementType.TableAndColumn, propertyName, null);
        }

        /// <summary>
        /// Returns a parameter formatter for the combined table and column attached to the specified property of the main entity.
        /// If you wish to resolve the column name of another entity, please use the generic overload instead.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. <code>nameof(entity.propname)</code>)</param>
        /// <param name="entityMappingOverride">Overrides the entity mapping used in the query method.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IFormattable TableAndColumn<TEntity>(string propertyName, EntityMapping<TEntity> entityMappingOverride)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));
            return new SqlParameterFormatter(SqlParameterElementType.TableAndColumn, propertyName, entityMappingOverride.Registration);
        }
    }
}
