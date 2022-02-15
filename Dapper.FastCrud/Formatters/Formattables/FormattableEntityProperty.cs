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
            string? legacyDefaultFormatSpecifierOutsideOurFormatter)
            : base(entityDescriptor, registrationOverride, alias, legacyDefaultFormatSpecifierOutsideOurFormatter)
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
            GenericSqlStatementFormatter.ParseFormat(format, out string? parsedFormat, out string? parsedAlias);

            if (parsedFormat == null)
            {
                if (parsedAlias != null)
                {
                    // only happening in the new world
                    parsedFormat = FormatSpecifiers.FullyQualifiedColumn;
                }
                else if (formatProvider is GenericSqlStatementFormatter)
                {
                    // running under our own formatter, might be legacy, otherwise we'll fail later
                    parsedFormat = this.LegacyDefaultFormatSpecifierOutsideOurFormatter;
                }
                else if (this.LegacyDefaultFormatSpecifierOutsideOurFormatter != null)
                {
                    // running outside our formatter, default to a legacy behavior
                    parsedFormat = this.LegacyDefaultFormatSpecifierOutsideOurFormatter;
                }
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
                    if (formatProvider is GenericSqlStatementFormatter)
                    {
                        throw new InvalidOperationException($"Unknown format specifier '{format}' specified for an entity property in Dapper.FastCrud");
                    }
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
