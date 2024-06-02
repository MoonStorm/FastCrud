namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;
    using System;

    /// <summary>
    /// Formattable parameter.
    /// </summary>
    internal class FormattableParameter : Formattable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FormattableParameter(
            EntityDescriptor entityDescriptor, 
            EntityRegistration? registrationOverride, 
            string parameter)
        :base(entityDescriptor, registrationOverride)
        {
            Validate.NotNull(parameter, nameof(parameter));
            this.Parameter = parameter;
        }

        /// <summary>
        /// The provided identifier.
        /// </summary>
        public string Parameter { get; }

        /// <summary>
        /// Performs formatting under FastCrud.
        /// </summary>
        internal override string PerformFastCrudFormatting(GenericStatementSqlBuilder sqlBuilder, string? format, GenericSqlStatementFormatter fastCrudFormatter)
        {
            string formattedOutput;
            switch (format)
            {
                case FormatSpecifiers.Parameter:
                case null:
                    formattedOutput = sqlBuilder.GetPrefixedParameter(this.Parameter);
                    break;
                default:
                    // running under our own provider, need to obey the rules
                    throw new InvalidOperationException($"Invalid format specifier '{format}' provided for parameter '{this.Parameter}' encountered under the FastCrud formatter.");
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
                case FormatSpecifiers.Parameter:
                    formattedOutput = sqlBuilder.GetPrefixedParameter(this.Parameter);
                    break;
                case null:
                    // provide the clean identifier
                    formattedOutput = this.Parameter;
                    break;
                default:
                    // running under our own provider, need to obey the rules
                    throw new InvalidOperationException($"Invalid format specifier '{format}' provided for parameter '{this.Parameter}' encountered under a non-FastCrud formatter.");
            }

            return formattedOutput;
        }
    }
}
