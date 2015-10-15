namespace Dapper.FastCrud.ParameterResolvers
{
    using System;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Deferrs the resolution of a parameter until the query is ready to produce the SQL.
    /// </summary>
    internal class DeferredSqlBuilderParameterResolver:IParameterResolver
    {
        private readonly Func<ISqlBuilder, string> _externalParameterResolver;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DeferredSqlBuilderParameterResolver(Type entityType, EntityMapping entityMappingOverride, Func<ISqlBuilder, string> externalParameterResolver)
        {
            this.EntityType = entityType;
            this.EntityMappingOverride = entityMappingOverride;
            _externalParameterResolver = externalParameterResolver;
        }

        /// <summary>
        /// Gets the type of the entity attached to the resolver.
        /// If NULL, the resolver was created for the main entity.
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// Optional entity mapping .
        /// </summary>
        protected EntityMapping EntityMappingOverride { get; private set; }

        /// <summary>
        /// If overriden, returns the sql builder associated with the optional entity descriptor and entity mapping.
        /// Note: Any or all the parameters can be <c>NULL</c>
        /// </summary>
        protected virtual ISqlBuilder GetSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
        {
            return null;
        }

        /// <summary>
        /// Prepares the resolver with as much information as possible and asks for the final value of the parameter.
        /// </summary>
        public string Resolve(EntityDescriptor entityDescriptor, EntityMapping entityMapping, ISqlBuilder sqlBuilder)
        {
            // any of the parameters above can be null
            if (sqlBuilder == null)
            {
                sqlBuilder = this.GetSqlBuilder(entityDescriptor, entityMapping);
                if (sqlBuilder == null)
                {
                    throw new InvalidOperationException("Not enough information is available in the current context.");
                }
            }

            return _externalParameterResolver(sqlBuilder);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            // this really shouldn't be called directly. 
            return this.Resolve(null, null, null);
        }
    }
}
