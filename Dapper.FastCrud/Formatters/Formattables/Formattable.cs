namespace Dapper.FastCrud.Formatters.Formattables
{
    using System;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// A formattable that can be used straight in the formattable strings representing various clauses in Dapper.FastCrud.
    /// It can also be used on its own through <seealso cref="IFormattable.ToString(string,IFormatProvider)"/> or <seealso cref="ToString()"/>.
    /// For more information, check the method that was used to create it in <seealso cref="Sql"/>.
    /// </summary>
    public abstract class Formattable : IFormattable
    {
        private readonly EntityDescriptor _entityDescriptor;
        private readonly EntityRegistration? _registrationOverride; 

        /// <summary>
        /// Default constructor
        /// </summary>
        internal Formattable(
            EntityDescriptor entityDescriptor,
            EntityRegistration? registrationOverride)
        {
            Validate.NotNull(entityDescriptor, nameof(entityDescriptor));

            _entityDescriptor = entityDescriptor;
            _registrationOverride = registrationOverride;
        }

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
            var sqlBuilder = _entityDescriptor.GetSqlBuilder(_registrationOverride);

            string output;
            if (formatProvider is GenericSqlStatementFormatter fastCrudFormatter)
            {
                output = this.PerformFastCrudFormatting(sqlBuilder, format, fastCrudFormatter);
            }
            else
            {
                output = this.PerformGenericFormatting(sqlBuilder, format);
            }

            return output;
        }

        /// <summary>
        /// Returns the raw representation of the object, which is not SQL ready.
        /// Depending on the usage, this can be the name of the resolved table, column, alias, identifier or parameter.
        /// </summary>
        public override string ToString()
        {
            return this.ToString(null, null);
        }

        /// <summary>
        /// Performs formatting under FastCrud.
        /// </summary>
        internal abstract string PerformFastCrudFormatting(
            GenericStatementSqlBuilder sqlBuilder,
            string? format,
            GenericSqlStatementFormatter fastCrudFormatter);

        /// <summary>
        /// Performs formatting outside FastCrud.
        /// </summary>
        internal abstract string PerformGenericFormatting(
            GenericStatementSqlBuilder sqlBuilder,
            string? format);

    }
}
