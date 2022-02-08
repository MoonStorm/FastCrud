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

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <param name="format">The format to use.
        /// -or-
        /// A null reference (<see langword="Nothing" /> in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
        /// <param name="formatProvider">The provider to use to format the value.
        /// -or-
        /// A null reference (<see langword="Nothing" /> in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public override string ToString(string? format, IFormatProvider? formatProvider)
        {
            var sqlBuilder = this.EntityDescriptor.GetSqlBuilder(this.EntityRegistrationOverride);
            if (formatProvider is GenericSqlStatementFormatter 
                && string.IsNullOrEmpty(format) 
                && this.DefaultFormatSpecifier != null)
            {
                format = this.DefaultFormatSpecifier;
            }

            switch (format)
            {
                case FormatSpecifiers.SingleColumn:
                    if (formatProvider is GenericSqlStatementFormatter fastCrudFormatter && fastCrudFormatter.ForceFullyQualifiedColumns)
                    {
                        return this.ToString(FormatSpecifiers.TableOrAliasWithColumn, formatProvider);
                    }
                    return sqlBuilder.GetColumnName(this.PropertyName);
                case FormatSpecifiers.TableOrAliasWithColumn:
                    return sqlBuilder.GetColumnName(this.PropertyName, this.Alias ?? (this.EntityRegistrationOverride ?? this.EntityDescriptor.CurrentEntityMappingRegistration).TableName);
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
