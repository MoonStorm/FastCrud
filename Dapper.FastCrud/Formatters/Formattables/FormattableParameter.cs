namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
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
        {
            Validate.NotNull(entityDescriptor, nameof(entityDescriptor));
            Validate.NotNullOrEmpty(parameter, nameof(parameter));

            this.EntityDescriptor = entityDescriptor;
            this.EntityRegistrationOverride = registrationOverride;
            this.Parameter = parameter;
        }

        /// <summary>
        /// Entity descriptor.
        /// </summary>
        public EntityDescriptor EntityDescriptor { get; }

        /// <summary>
        /// An optional entity registration override.
        /// </summary>
        public EntityRegistration? EntityRegistrationOverride { get; }

        /// <summary>
        /// The provided identifier.
        /// </summary>
        public string Parameter { get; }

        /// <summary>
        /// Applies formatting to the current instance. For more information, see <seealso cref="Sql.Parameter"/>.
        /// </summary>
        /// <param name="format"> An optional format specifier.</param>
        /// <param name="formatProvider">The provider to use to format the value.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public override string ToString(string? format, IFormatProvider? formatProvider = null)
        {
            var sqlBuilder = this.EntityDescriptor.GetSqlBuilder(this.EntityRegistrationOverride);
            if (string.IsNullOrEmpty(format) && formatProvider is GenericSqlStatementFormatter)
            {
                // if OUR formatter is being used, use the SQL ready version
                format = FormatSpecifiers.Parameter;
            }

            switch (format)
            {
                case FormatSpecifiers.Parameter:
                    return sqlBuilder.GetPrefixedParameter(this.Parameter);
                default:
                    // for generic usage we'll return what we have, without delimiters
                    return this.Parameter;
            }
        }
    }
}
