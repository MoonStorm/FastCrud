namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;
    using Dapper.FastCrud.SqlBuilders;

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
            string? forcedDefaultFormat)
            : base(entityDescriptor, registrationOverride, alias, forcedDefaultFormat)
        {
            Validate.NotNullOrEmpty(propertyName, nameof(propertyName));

            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Represents a property name.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Performs formatting under FastCrud.
        /// </summary>
        internal override string PerformFastCrudFormatting(
            GenericStatementSqlBuilder sqlBuilder,
            string? format,
            GenericSqlStatementFormatter fastCrudFormatter)
        {
            GenericSqlStatementFormatter.ParseFormat(format, out string? parsedFormat, out string? parsedAlias);

            // fix the parsedAlias if not set
            parsedAlias = parsedAlias ?? this.Alias;

            // fix the format if we can
            if (parsedFormat == null)
            {
                parsedFormat = this.ForcedDefaultFormat;
            }

            string formattedOutput;
            switch (parsedFormat)
            {
                case FormatSpecifiers.SingleColumn:
                    // return the delimited column
                    formattedOutput = sqlBuilder.GetColumnName(this.PropertyName);
                    break;
                case FormatSpecifiers.FullyQualifiedColumn:
                case null:
                    // return the delimited column qualified with the alias or the table name
                    formattedOutput = sqlBuilder.GetColumnName(this.PropertyName, parsedAlias ?? sqlBuilder.EntityDescriptor.CurrentEntityMappingRegistration.TableName);
                    break;
                default:
                    // running under our own formatter, needs to play by our own rules
                    throw new InvalidOperationException($"Invalid format specifier '{format}' provided for the entity property'{sqlBuilder.EntityDescriptor.EntityType.Name} (alias: {this.Alias ?? "None"}).{this.PropertyName}' encountered under the FastCrud formatter.");
            }

            return formattedOutput;
        }

        /// <summary>
        /// Performs formatting outside FastCrud.
        /// </summary>
        internal override string PerformGenericFormatting(GenericStatementSqlBuilder sqlBuilder, string? format)
        {
            GenericSqlStatementFormatter.ParseFormat(format, out string? parsedFormat, out string? parsedAlias);

            // fix the parsedAlias if not set
            parsedAlias = parsedAlias ?? this.Alias;

            string formattedOutput;
            switch (parsedFormat)
            {
                case FormatSpecifiers.SingleColumn:
                    formattedOutput = sqlBuilder.GetTableName(parsedAlias);
                    break;
                case FormatSpecifiers.FullyQualifiedColumn:
                    // return the delimited column qualified with the alias or the table name
                    formattedOutput = sqlBuilder.GetColumnName(this.PropertyName, parsedAlias ?? sqlBuilder.EntityDescriptor.CurrentEntityMappingRegistration.TableName);
                    break;
                case null:
                    // NOT under our own formatter, return the column name in clear
                    var propertyRegistration = sqlBuilder.EntityRegistration.TryGetFrozenPropertyRegistrationByPropertyName(this.PropertyName);
                    if (propertyRegistration == null)
                    {
                        throw new ArgumentException($"The entity property found that the property '{this.PropertyName}' was not found on '{sqlBuilder.EntityDescriptor.EntityType.Name}' under a non-FastCrud formatter.");
                    }

                    if (parsedAlias != null)
                    {
                        // it's debatable whether we should do this,
                        // but at the same time the caller should not provide the alias if it didn't want the alias to be part of the output.
                        formattedOutput = FormattableString.Invariant($"{parsedAlias}.{propertyRegistration.DatabaseColumnName}");
                    }
                    else
                    {
                        formattedOutput = propertyRegistration.DatabaseColumnName;
                    }

                    break;
                default:
                    throw new InvalidOperationException($"Invalid format specifier '{format}' provided for the entity property'{sqlBuilder.EntityDescriptor.EntityType.Name} (alias: {this.Alias ?? "None"}).{this.PropertyName}' encountered under a non-FastCrud formatter.");
            }

            return formattedOutput;
        }

    }
}
