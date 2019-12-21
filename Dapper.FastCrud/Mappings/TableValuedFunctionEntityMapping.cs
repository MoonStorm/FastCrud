using Dapper.FastCrud.Validations;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.FastCrud.Mappings
{
    /// <summary>
    /// Holds information about table-valued fanction mapped properties for a particular entity type.
    /// Multiple instances of such mappings can be active for a single entity type.
    /// </summary>
    public interface ITableValuedFunctionEntityMapping
    {
        /// <summary>
        ///  table-valued function parameters
        /// </summary>
        IEnumerable<string> ParameterNames { get; }

        /// <summary>
        /// Constructs a parameter selection of all parameter to be refreshed function call of the form <code>@PropertyName1,@PropertyName2...</code>
        /// </summary>
        string ConstructFunctionParameterClause();
    }

    /// <summary>
    /// Holds information about table-valued fanction mapped properties for a particular entity type.
    /// Multiple instances of such mappings can be active for a single entity type.
    /// </summary>
    public sealed class TableValuedFunctionEntityMapping<TEntity> : EntityMapping<TEntity, TableValuedFunctionEntityMapping<TEntity>>, ITableValuedFunctionEntityMapping
    {
        /// <summary>
        ///  table-valued function parameters
        /// </summary>
        public IEnumerable<string> ParameterNames { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TableValuedFunctionEntityMapping()
        {
            this.ParameterNames = new string[0];
        }

        /// <summary>
        /// Current Entity Mapping
        /// </summary>
        protected override TableValuedFunctionEntityMapping<TEntity> CurrentEntityMapping => this;

        /// <summary>
        /// Sets the database table-valued function parameters
        /// </summary>
        /// <param name="functionParameterNames">Parameters of table-valued functio name</param>
        public TableValuedFunctionEntityMapping<TEntity> SetFunctionParameterNames(string[] functionParameterNames)
        {
            this.ValidateState();

            Requires.NotNull(functionParameterNames, nameof(functionParameterNames));

            this.ParameterNames = functionParameterNames ?? new string[0];
            return this.CurrentEntityMapping;
        }

        /// <summary>
        /// Clones the current mapping set, allowing for further modifications.
        /// </summary>
        public override TableValuedFunctionEntityMapping<TEntity> Clone()
        {
            var clonedMappings = new TableValuedFunctionEntityMapping<TEntity>()
                .SetSchemaName(this.SchemaName)
                .SetTableName(this.TableName)
                .SetFunctionParameterNames(this.ParameterNames.ToArray())
                .SetDialect(this.Dialect);

            foreach (var clonedPropMapping in this.PropertyMappings.Select(propNameMapping => propNameMapping.Value.Clone(clonedMappings)))
            {
                clonedMappings.SetPropertyInternal(clonedPropMapping);
            }

            return clonedMappings;
        }

        /// <summary>
        /// Constructs a parameter selection of all parameter to be refreshed function call of the form <code>@PropertyName1,@PropertyName2...</code>
        /// </summary>
        string ITableValuedFunctionEntityMapping.ConstructFunctionParameterClause()
        {
            var parameterPrefix = OrmConfiguration.Conventions.GetDatabaseOptions(this.Dialect).ParameterPrefix;
            return string.Join(",", this.ParameterNames.Select(parameterName => parameterPrefix + parameterName));

        }
    }
}
