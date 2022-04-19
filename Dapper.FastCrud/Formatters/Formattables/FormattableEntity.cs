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
            string? legacyDefaultFormatSpecifierOutsideOurFormatter)
        {
            Requires.NotNull(entityDescriptor, nameof(entityDescriptor));

            this.LegacyDefaultFormatSpecifierOutsideOurFormatter = legacyDefaultFormatSpecifierOutsideOurFormatter;
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
        /// The default format specifier to use when running outside of our own formatter.
        /// </summary>
        public string? LegacyDefaultFormatSpecifierOutsideOurFormatter { get; }

        /// <summary>
        /// Applies formatting to the current instance. For more information, see <seealso cref="Sql.Entity{TEntity}(string?,Dapper.FastCrud.Mappings.EntityMapping{TEntity}?)"/>.
        /// </summary>
        /// <param name="format"> An optional format specifier.</param>
        /// <param name="formatProvider">The provider to use to format the value.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public override string ToString(string? format, IFormatProvider? formatProvider = null)
        {
            var sqlBuilder = this.EntityDescriptor.GetSqlBuilder(this.EntityRegistrationOverride);

            GenericSqlStatementFormatter.ParseFormat(format, out string? parsedFormat, out string? parsedAlias);

            if (parsedFormat == null)
            {
                if (parsedAlias != null)
                {
                    // only happening in the new world
                    parsedFormat = FormatSpecifiers.TableOrAlias;
                }
                else if (formatProvider is GenericSqlStatementFormatter)
                {
                    // running under our own formatter could be legacy, otherwise we'll fail
                    parsedFormat = this.LegacyDefaultFormatSpecifierOutsideOurFormatter;
                }
                else if (this.LegacyDefaultFormatSpecifierOutsideOurFormatter != null)
                {
                    // running outside our formatter, default to a legacy behavior
                    parsedFormat = this.LegacyDefaultFormatSpecifierOutsideOurFormatter;
                }
            }

            switch (parsedFormat)
            {
                case FormatSpecifiers.TableOrAlias:
                    return sqlBuilder.GetTableName(parsedAlias ?? this.Alias);
                default:
                    if (formatProvider is GenericSqlStatementFormatter)
                    {
                        throw new InvalidOperationException($"Unknown format specifier '{format}' specified for an entity in Dapper.FastCrud");
                    }

                    // for generic usage, we'll return the table name or alias if provided, without delimiters
                    return this.Alias ?? (this.EntityRegistrationOverride ?? this.EntityDescriptor.CurrentEntityMappingRegistration).TableName;
            }
        }
    }
}
