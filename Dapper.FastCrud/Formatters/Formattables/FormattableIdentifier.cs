namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;

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
        {
            Requires.NotNull(entityDescriptor, nameof(entityDescriptor));
            Requires.NotNullOrEmpty(identifier, nameof(identifier));

            this.EntityDescriptor = entityDescriptor;
            this.EntityRegistrationOverride = registrationOverride;
            this.Identifier = identifier;
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
        public string Identifier { get; }

        /// <summary>
        /// Applies formatting to the current instance. For more information, see <seealso cref="Sql.Identifier"/>.
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
                format = FormatSpecifiers.Identifier;
            }

            switch (format)
            {
                case FormatSpecifiers.Identifier:
                    return sqlBuilder.GetDelimitedIdentifier(this.Identifier);
                default:
                    // for generic usage we'll return what we have, without delimiters
                    return this.Identifier;
            }
        }
    }
}
