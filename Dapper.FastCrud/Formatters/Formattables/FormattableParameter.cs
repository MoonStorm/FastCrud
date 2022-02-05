namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;

    /// <summary>
    /// Formattable parameter.
    /// </summary>
    internal class FormattableParameter : IFormattable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FormattableParameter(EntityDescriptor entityDescriptor, EntityRegistration? registrationOverride, string parameter)
        {
            Requires.NotNull(entityDescriptor, nameof(entityDescriptor));
            Requires.NotNullOrEmpty(parameter, nameof(parameter));

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

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <param name="format">The format to use.
        /// -or-
        /// A null reference (<see langword="Nothing" /> in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
        /// <param name="formatProvider">The provider to use to format the value.
        /// -or-
        /// A null reference (<see langword="Nothing" /> in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (format)
            {
                case "P":
                    return FormattableString.Invariant($"@{this.Parameter}");
                default:
                    // for generic usage we'll return what we have, without delimiters
                    return this.Parameter;
            }
        }
    }
}
