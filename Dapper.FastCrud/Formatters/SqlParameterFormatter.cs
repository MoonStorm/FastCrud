namespace Dapper.FastCrud.Formatters
{
    using System;
    using System.Globalization;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Mappings.Registrations;

    /// <summary>
    /// Defers the resolution of a parameter until we're resolved later with our custom format provider.
    /// </summary>
    internal class SqlParameterFormatter: IFormattable
    {
        /// <summary>
        /// Constructor used when the entity type is set to the query's main one.
        /// </summary>
        public SqlParameterFormatter(SqlParameterElementType elementType, string? parameterValue, EntityRegistration? entityMappingOverride)
            : this(elementType, parameterValue, null, entityMappingOverride)
        {
        }

        /// <summary>
        /// Constructor used by entity type aware formatters.
        /// </summary>
        protected SqlParameterFormatter(SqlParameterElementType elementType, string? parameterValue, Type? entityType, EntityRegistration? entityMappingOverride)
        {
            this.ElementType = elementType;
            this.ParameterValue = parameterValue;
            this.EntityType = entityType;
            this.EntityMappingOverride = entityMappingOverride;
        }
        
        /// <summary>
        /// Gets the original value of the parameter.
        /// </summary>
        public string? ParameterValue { get; }

        /// <summary>
        /// Gets the SQL representation of the parameter.
        /// </summary>
        public SqlParameterElementType ElementType { get; }

        /// <summary>
        /// Gets the type of the entity attached to the resolver.
        /// If NULL, the resolver was created for the main entity.
        /// </summary>
        public Type? EntityType { get; }

        /// <summary>
        /// Optional entity mapping .
        /// </summary>
        protected EntityRegistration? EntityMappingOverride { get; }

        /// <summary>
        /// If overridden, returns the sql builder associated with the optional entity descriptor and entity mapping.
        /// Note: Any or all the parameters can be <c>NULL</c>
        /// </summary>
        protected virtual ISqlBuilder GetSqlBuilder(EntityDescriptor? entityDescriptor, EntityRegistration? entityMapping)
        {
            return null;
        }

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>
        /// The value of the current instance in the specified format.
        /// </returns>
        /// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation. </param><param name="formatProvider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            ISqlBuilder sqlBuilder;

            SqlStatementFormatter sqlQueryFormatter;

            // is it our formatter?
            if ((sqlQueryFormatter = formatProvider as SqlStatementFormatter) != null)
            {
                // make sure we can use the formatter
                if (this.EntityType == null || this.EntityType == sqlQueryFormatter.MainEntityType)
                {
                    // the formatter's entity is ours

                    // are we using the same entity mapping?
                    if (this.EntityMappingOverride == null || this.EntityMappingOverride == sqlQueryFormatter.MainEntityMapping)
                    {
                        sqlBuilder = sqlQueryFormatter.MainEntitySqlBuilder;
                    }
                    else
                    {
                        // we can't use the sql builder provided, as it was created for a different entity mapping
                        sqlBuilder = this.GetSqlBuilder(sqlQueryFormatter.MainEntityDescriptor, this.EntityMappingOverride);
                    }
                }
                else
                {
                    // not for our entity, we need to create a brand new builder, if we can
                    sqlBuilder = this.GetSqlBuilder(null, this.EntityMappingOverride);
                }
            }
            else
            {
                // not our formatter, do what we can
                sqlBuilder = this.GetSqlBuilder(null, this.EntityMappingOverride);
            }

            if (sqlBuilder == null)
            {
                    throw new InvalidOperationException("Not enough information is available in the current context.");
            }

            switch (this.ElementType)
            {
                case SqlParameterElementType.Column:
                    return sqlBuilder.GetColumnName(this.ParameterValue);
                case SqlParameterElementType.Table:
                    return sqlBuilder.GetTableName();
                case SqlParameterElementType.TableAndColumn:
                    return string.Format(CultureInfo.InvariantCulture,"{0}.{1}", sqlBuilder.GetTableName(), sqlBuilder.GetColumnName(this.ParameterValue));
                case SqlParameterElementType.Identifier:
                    return sqlBuilder.GetDelimitedIdentifier(this.ParameterValue);
                case SqlParameterElementType.Parameter:
                    return string.Format(CultureInfo.InvariantCulture, "@{0}", this.ParameterValue);
                default:
                    throw new InvalidOperationException($"Unknown SQL element type {this.ElementType}");
            }
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
            return this.ToString(null, null);
        }
    }
}
