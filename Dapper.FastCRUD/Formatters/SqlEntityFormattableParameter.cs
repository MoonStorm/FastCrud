namespace Dapper.FastCrud.Formatters
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Deferrs the resolution of a parameter for a specified entity until the query is ready to produce the SQL.
    /// </summary>
    internal class SqlEntityFormattableParameter<TEntity>:SqlParameterFormatter
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SqlEntityFormattableParameter(
            SqlParameterElementType elementType,
            string parameterValue,
            EntityMapping entityMappingOverride)
            : base(elementType, parameterValue, typeof(TEntity), entityMappingOverride)
        {
        }

        /// <summary>
        /// If overriden, returns the sql builder associated with the optional entity descriptor and entity mapping.
        /// Note: Any or all the parameters can be <c>NULL</c>
        /// </summary>
        protected override ISqlBuilder GetSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
        {
            return ((entityDescriptor as EntityDescriptor<TEntity>) ?? OrmConfiguration.GetEntityDescriptor<TEntity>())
                    .GetSqlStatements(entityMapping)
                    .SqlBuilder;
        }
    }
}
