namespace Dapper.FastCrud.ParameterResolvers
{
    using System;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Deferrs the resolution of a parameter for a specified entity until the query is ready to produce the SQL.
    /// </summary>
    internal class EntityDeferredSqlBuilderParameterResolver<TEntity>:DeferredSqlBuilderParameterResolver
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EntityDeferredSqlBuilderParameterResolver(EntityMapping entityMappingOverride, Func<ISqlBuilder, string> externalParameterResolver)
            : base(typeof(TEntity), entityMappingOverride, externalParameterResolver)
        {
        }

        /// <summary>
        /// If overriden, returns the sql builder associated with the optional entity descriptor and entity mapping.
        /// Note: Any or all the parameters can be <c>NULL</c>
        /// </summary>
        protected override ISqlBuilder GetSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
        {
            return (entityDescriptor ?? OrmConfiguration.GetEntityDescriptor<TEntity>())
                    .GetSqlStatements<TEntity>(entityMapping)
                    .SqlBuilder;
        }
    }
}
