namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;

    /// <summary>
    /// Formattable identifier.
    /// </summary>
    internal class FormattableIdentifier:IFormattable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FormattableIdentifier(
            EntityDescriptor entityDescriptor, 
            EntityRegistration? registrationOverride, 
            string identifier,
            string? defaultFormatSpecifier = null)
        {
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
        public string? DefaultFormatSpecifier { get; }

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

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <param name="format">The format to use.
        /// -or-
        /// A null reference (<see langword="Nothing" /> in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
        /// <param name="formatProvider">The provider to use to format the value.
        /// -or-
        /// A null reference (<see langword="Nothing" /> in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public string ToString(string? format, IFormatProvider? formatProvider)
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
                case FormatSpecifiers.Identifier:
                    return sqlBuilder.GetDelimitedIdentifier(this.Identifier);
                default:
                    // for generic usage we'll return what we have, without delimiters
                    return this.Identifier;
            }
        }
    }
}
