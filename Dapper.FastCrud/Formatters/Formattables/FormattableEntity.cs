namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;

    /// <summary>
    /// A formattable representing a database entity.
    /// </summary>
    internal class FormattableEntity: IFormattable
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public FormattableEntity(
            EntityDescriptor entityDescriptor, 
            EntityRegistration? registrationOverride, 
            string? alias,
            string? defaultLegacyFormatSpecifier = null)
        {
            Requires.NotNull(entityDescriptor, nameof(entityDescriptor));

            this.DefaultFormatSpecifier = defaultLegacyFormatSpecifier;
            this.EntityDescriptor = entityDescriptor;
            this.EntityRegistrationOverride = registrationOverride;
            this.Alias = alias;
        }

        /// <summary>
        /// An optional entity registration override.
        /// </summary>
        public EntityRegistration? EntityRegistrationOverride { get; }

        /// <summary>
        /// An optional alias.
        /// </summary>
        public string? Alias { get; }

        /// <summary>
        /// The entity type
        /// </summary>
        public EntityDescriptor EntityDescriptor { get; }

        /// <summary>
        /// The default format specifier to use for a legacy behavior.
        /// </summary>
        public string? DefaultFormatSpecifier { get; }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <param name="format">The format to use.
        /// -or-
        /// A null reference (<see langword="Nothing" /> in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
        /// <param name="formatProvider">The provider to use to format the value.
        /// -or-
        /// A null reference (<see langword="Nothing" /> in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public virtual string ToString(string? format, IFormatProvider? formatProvider)
        {
            var sqlBuilder = this.EntityDescriptor.GetSqlBuilder(this.EntityRegistrationOverride);

            if (string.IsNullOrEmpty(format) && this.DefaultFormatSpecifier != null)
            {
                format = this.DefaultFormatSpecifier;
            }

            switch (format)
            {
                case "T":
                    return sqlBuilder.GetTableName(this.Alias);
                default:
                    // for generic usage, we'll return the table name or alias if provided, without delimiters
                    return this.Alias ?? (this.EntityRegistrationOverride ?? this.EntityDescriptor.CurrentEntityMappingRegistration).TableName;
            }
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return this.ToString(null, null);
        }
    }
}
