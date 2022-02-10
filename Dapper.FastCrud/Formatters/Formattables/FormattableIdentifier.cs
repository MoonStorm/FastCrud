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
            string identifier,
            string defaultFormatSpecifier)
        {
            Requires.NotNull(defaultFormatSpecifier, nameof(defaultFormatSpecifier));
            Requires.NotNull(entityDescriptor, nameof(entityDescriptor));
            Requires.NotNullOrEmpty(identifier, nameof(identifier));

            this.DefaultFormatSpecifier = defaultFormatSpecifier;
            this.EntityDescriptor = entityDescriptor;
            this.EntityRegistrationOverride = registrationOverride;
            this.Identifier = identifier;
        }

        /// <summary>
        /// The default format specifier to use.
        /// </summary>
        public string DefaultFormatSpecifier { get; }

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

            if (formatProvider is GenericSqlStatementFormatter && string.IsNullOrEmpty(format))
            {
                format = this.DefaultFormatSpecifier;
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
