namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;
    using Dapper.FastCrud.SqlBuilders;

    /// <summary>
    /// Formattable identifier.
    /// </summary>
    internal class FormattableIdentifier:Formattable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FormattableIdentifier(
            EntityDescriptor entityDescriptor,
            EntityRegistration? registrationOverride,
            string identifier)
            : base(entityDescriptor, registrationOverride)
        {
            Validate.NotNull(identifier, nameof(identifier));
            this.Identifier = identifier;
        }

        /// <summary>
        /// The provided identifier.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Performs formatting under FastCrud.
        /// </summary>
        internal override string PerformFastCrudFormatting(GenericStatementSqlBuilder sqlBuilder, string? format, GenericSqlStatementFormatter fastCrudFormatter)
        {
            string formattedOutput;
            switch (format)
            {
                case FormatSpecifiers.Identifier:
                case null:
                    formattedOutput = sqlBuilder.GetDelimitedIdentifier(this.Identifier);
                    break;
                default:
                    // running under our own provider, need to obey the rules
                    throw new InvalidOperationException($"Invalid format specifier '{format}' provided for identifier '{this.Identifier}' encountered under the FastCrud formatter.");
            }

            return formattedOutput;
        }

        /// <summary>
        /// Performs formatting outside FastCrud.
        /// </summary>
        internal override string PerformGenericFormatting(GenericStatementSqlBuilder sqlBuilder, string? format)
        {
            string formattedOutput;
            switch (format)
            {
                case FormatSpecifiers.Identifier:
                    formattedOutput = sqlBuilder.GetDelimitedIdentifier(this.Identifier);
                    break;
                case null:
                    // provide the clean identifier
                    formattedOutput = this.Identifier;
                    break;
                default:
                    // running under our own provider, need to obey the rules
                    throw new InvalidOperationException($"Invalid format specifier '{format}' provided for identifier '{this.Identifier}' encountered under a non-FastCrud formatter.");
            }

            return formattedOutput;
        }
    }
}
