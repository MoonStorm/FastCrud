namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;

    /// <summary>
    /// A formattable representing a property on a database entity.
    /// </summary>
    internal class FormattableEntityProperty:FormattableEntity
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public FormattableEntityProperty(
            EntityDescriptor entityDescriptor, 
            EntityRegistration? registrationOverride, 
            string propertyName, 
            string? alias,
            string? defaultFormatSpecifier)
            : base(entityDescriptor, registrationOverride, alias, defaultFormatSpecifier)
        {
            Requires.NotNullOrEmpty(propertyName, nameof(propertyName));

            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Represents a property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Applies formatting to the current instance. For more information, see <seealso cref="Sql.EntityProperty{TEntity}"/>.
        /// </summary>
        /// <param name="format"> An optional format specifier.</param>
        /// <param name="formatProvider">The provider to use to format the value.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public override string ToString(string? format, IFormatProvider? formatProvider = null)
        {
            var sqlBuilder = this.EntityDescriptor.GetSqlBuilder(this.EntityRegistrationOverride);
            if (formatProvider is GenericSqlStatementFormatter 
                && string.IsNullOrEmpty(format) 
                && this.DefaultFormatSpecifier != null)
            {
                format = this.DefaultFormatSpecifier;
            }

            GenericSqlStatementFormatter.ParseFormat(format, out string parsedFormat, out string parsedAlias);

            if (parsedFormat == null && parsedAlias != null)
            {
                // mimic what we have in the main formatter GenericSqlStatementFormatter
                parsedFormat = FormatSpecifiers.FullyQualifiedColumn;
            }

            if (parsedFormat == null &&  formatProvider is GenericSqlStatementFormatter)
            {
                parsedFormat = this.DefaultFormatSpecifier;
            }

            switch (parsedFormat)
            {
                case FormatSpecifiers.SingleColumn:
                    if (formatProvider is GenericSqlStatementFormatter fastCrudFormatter && fastCrudFormatter.ForceFullyQualifiedColumns)
                    {
                        return this.ToString(FormatSpecifiers.FullyQualifiedColumn, formatProvider);
                    }
                    return sqlBuilder.GetColumnName(this.PropertyName);
                case FormatSpecifiers.FullyQualifiedColumn:
                    return sqlBuilder.GetColumnName(this.PropertyName, parsedAlias ?? this.Alias ?? (this.EntityRegistrationOverride ?? this.EntityDescriptor.CurrentEntityMappingRegistration).TableName);
                default:
                    // by default we'll return the column name in clear
                    var propertyRegistration = (this.EntityRegistrationOverride ?? this.EntityDescriptor.CurrentEntityMappingRegistration).TryGetFrozenPropertyRegistrationByPropertyName(this.PropertyName);
                    if (propertyRegistration == null)
                    {
                        throw new ArgumentException($"Property '{this.PropertyName}' was not found on '{(this.EntityRegistrationOverride??this.EntityDescriptor.CurrentEntityMappingRegistration).EntityType}'");
                    }

                    return propertyRegistration.DatabaseColumnName;
            }
        }
    }
}
