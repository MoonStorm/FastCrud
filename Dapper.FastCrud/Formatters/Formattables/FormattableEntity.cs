namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// A formattable representing a database entity.
    /// </summary>
    internal class FormattableEntity: Formattable
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public FormattableEntity(
            EntityDescriptor entityDescriptor, 
            EntityRegistration? registrationOverride, 
            string? alias,
            string? forcedDefaultFormat)
        :base(entityDescriptor, registrationOverride)
        {
            Validate.NotNull(entityDescriptor, nameof(entityDescriptor));

            this.ForcedDefaultFormat = forcedDefaultFormat;
            this.Alias = alias;
        }

        /// <summary>
        /// An optional alias.
        /// </summary>
        public string? Alias { get; }

        /// <summary>
        /// When set, a default is going to be enforced.
        /// </summary>
        public string? ForcedDefaultFormat { get; }

        ///// <summary>
        ///// The default format specifier to use when running outside our own formatter.
        ///// </summary>
        //public string? LegacyDefaultFormatSpecifierOutsideOurFormatter { get; }

        /// <summary>
        /// Performs formatting under FastCrud.
        /// </summary>
        internal override string PerformFastCrudFormatting(GenericStatementSqlBuilder sqlBuilder, string? format, GenericSqlStatementFormatter fastCrudFormatter)
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
                case FormatSpecifiers.TableOrAlias:
                case null:
                    formattedOutput = sqlBuilder.GetTableName(parsedAlias);
                    break;
                default:
                    // running under our own provider, need to obey the rules
                    throw new InvalidOperationException(
                        $"Invalid format specifier '{format}' provided for entity '{sqlBuilder.EntityDescriptor.EntityType.Name} (alias: {parsedAlias ?? "None"})' encountered under the FastCrud formatter.");
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
                case FormatSpecifiers.TableOrAlias:
                    formattedOutput = sqlBuilder.GetTableName(parsedAlias);
                    break;
                case null:
                    // we are able to provide the table name, non-delimited, when we're not asked to format with our own specifiers
                    formattedOutput = parsedAlias ?? sqlBuilder.EntityDescriptor.CurrentEntityMappingRegistration.TableName;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid format specifier '{format}' provided for entity '{sqlBuilder.EntityDescriptor.EntityType.Name} (alias: {parsedAlias ?? "None"})' encountered under a non-FastCrud formatter.");
            }

            return formattedOutput;
        }
    }
}
