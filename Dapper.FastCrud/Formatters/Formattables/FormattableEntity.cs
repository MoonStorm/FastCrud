namespace Dapper.FastCrud.Formatters.Formattables
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;

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
            string? defaultFormatSpecifier)
        {
            Requires.NotNull(entityDescriptor, nameof(entityDescriptor));

            this.DefaultFormatSpecifier = defaultFormatSpecifier;
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
        /// The default format specifier to use.
        /// </summary>
        public string? DefaultFormatSpecifier { get; }

        /// <summary>
        /// Applies formatting to the current instance. For more information, see <seealso cref="Sql.Entity{TEntity}"/>.
        /// </summary>
        /// <param name="format"> An optional format specifier.</param>
        /// <param name="formatProvider">The provider to use to format the value.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public override string ToString(string? format, IFormatProvider? formatProvider = null)
        {
            var sqlBuilder = this.EntityDescriptor.GetSqlBuilder(this.EntityRegistrationOverride);

            GenericSqlStatementFormatter.ParseFormat(format, out string parsedFormat, out string parsedAlias);

            if (parsedFormat == null && parsedAlias != null)
            {
                parsedFormat = FormatSpecifiers.TableOrAlias;
            }

            if (parsedFormat == null && formatProvider is GenericSqlStatementFormatter)
            {
                parsedFormat = this.DefaultFormatSpecifier;
            }

            switch (parsedFormat)
            {
                case FormatSpecifiers.TableOrAlias:
                    return sqlBuilder.GetTableName(parsedAlias ?? this.Alias);
                default:
                    // for generic usage, we'll return the table name or alias if provided, without delimiters
                    return this.Alias ?? (this.EntityRegistrationOverride ?? this.EntityDescriptor.CurrentEntityMappingRegistration).TableName;
            }
        }
    }
}
