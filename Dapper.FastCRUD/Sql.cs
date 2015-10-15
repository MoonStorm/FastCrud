namespace Dapper.FastCrud
{
    using System;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.ParameterResolvers;

    /// <summary>
    /// This SQL builder can be used for mapping table and column names to their SQL counterparts.
    /// </summary>
    public static class Sql
    {
        private static readonly Func<ISqlBuilder, string> _noAliasTableNameResolverAction;
        private static readonly Func<string, Func<ISqlBuilder, string>> _noAliasColumnNameResolverAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        static Sql()
        {
            // initialize the resolver actions
            _noAliasTableNameResolverAction = sqlBuilder => sqlBuilder.GetTableName();

            _noAliasColumnNameResolverAction = (propertyName) => sqlBuilder => sqlBuilder.GetColumnName(propertyName);
        }

        /// <summary>
        /// Returns a parameter resolver for the SQL table name of the main entity.
        /// If you wish to resolve the table name of another entity, please use the generic overload instead.
        /// The resolver can be used as-is in a formattable string expression.
        /// </summary>
        /// <param name="entityMappingOverride">Overrides the entity mapping used in the query method.</param>
        public static IParameterResolver Table(EntityMapping entityMappingOverride = null)
        {
            return new DeferredSqlBuilderParameterResolver(null, entityMappingOverride, _noAliasTableNameResolverAction);
        }

        /// <summary>
        /// Returns a parameter resolver for the SQL table name of the provided entity.
        /// If you wish to resolve the table name of the main entity, please use the non-generic overload instead.
        /// The resolver can be used as-is in a formattable string expression.
        /// </summary>
        /// <param name="entityMappingOverride">Overrides the entity mapping used in the query method.</param>
        public static IParameterResolver Table<TEntity>(EntityMapping entityMappingOverride = null)
        {
            return new EntityDeferredSqlBuilderParameterResolver<TEntity>(entityMappingOverride, _noAliasTableNameResolverAction);
        }

        /// <summary>
        /// Returns a parameter resolver for name of the database column attached to the specified property of the main entity.
        /// If you wish to resolve the column name of another entity, please use the generic overload instead.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. <code>nameof(entity.propname)</code>)</param>
        /// <param name="entityMappingOverride">Overrides the entity mapping used in the query method.</param>
        public static IParameterResolver Column(string propertyName, EntityMapping entityMappingOverride = null)
        {
            return new DeferredSqlBuilderParameterResolver(null, entityMappingOverride, _noAliasColumnNameResolverAction(propertyName));
        }

        /// <summary>
        /// Returns the name of the database column attached to the property of the provided entity.
        /// If you wish to resolve the column name of the main entity, please use the non-generic overload instead.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. <code>nameof(entity.propname)</code>)</param>
        /// <param name="entityMappingOverride">Overrides the entity mapping used in the query method.</param>
        public static IParameterResolver Column<TEntity>(string propertyName, EntityMapping entityMappingOverride = null)
        {
            return new EntityDeferredSqlBuilderParameterResolver<TEntity>(entityMappingOverride, _noAliasColumnNameResolverAction(propertyName));
        }
    }
}
