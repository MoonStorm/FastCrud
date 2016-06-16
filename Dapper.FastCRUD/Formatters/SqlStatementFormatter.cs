namespace Dapper.FastCrud.Formatters
{
    using System;
    using System.Globalization;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Used to properly format the parameters of a query
    /// </summary>
    public sealed class SqlStatementFormatter:IFormatProvider, ICustomFormatter
    {
        private static readonly Type _customFormatterType = typeof(ICustomFormatter);
        private readonly bool _forceColumnAsTableColumnResolution;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mainEntityDescriptor">Main entity descriptor</param>
        /// <param name="mainEntityMapping">Main entity mappings</param>
        /// <param name="mainEntitySqlBuilder">SQL mapper for the main entity</param>
        /// <param name="forceColumnAsTableColumnResolution">If true, the format identifier 'C' will be treated as 'TC' </param>
        internal SqlStatementFormatter(EntityDescriptor mainEntityDescriptor, EntityMapping mainEntityMapping, ISqlBuilder mainEntitySqlBuilder, bool forceColumnAsTableColumnResolution)
        {
            _forceColumnAsTableColumnResolution = forceColumnAsTableColumnResolution;
            this.MainEntityType = mainEntityDescriptor.EntityType;
            this.MainEntityDescriptor = mainEntityDescriptor;
            this.MainEntitySqlBuilder = mainEntitySqlBuilder;
            this.MainEntityMapping = mainEntityMapping;            
        }

        /// <summary>
        /// The main entity descriptor
        /// </summary>
        internal EntityDescriptor MainEntityDescriptor { get; }

        /// <summary>
        /// The main entity type used in the query
        /// </summary>
        public Type MainEntityType { get; }

        /// <summary>
        /// The main entity sql builder. The SQL builder should not be used if the entity mapping doesn't match.
        /// </summary>
        public ISqlBuilder MainEntitySqlBuilder { get; }

        /// <summary>
        /// The main entity mapping used by the query.
        /// </summary>
        public EntityMapping MainEntityMapping { get; set; }

        /// <summary>
        /// Returns an object that provides formatting services for the specified type.
        /// </summary>
        /// <returns>
        /// An instance of the object specified by <paramref name="formatType"/>, if the <see cref="T:System.IFormatProvider"/> implementation can supply that type of object; otherwise, null.
        /// </returns>
        /// <param name="formatType">An object that specifies the type of format object to return. </param>
        public object GetFormat(Type formatType)
        {
            if (formatType == _customFormatterType)
            {
                return this;
            }

            return null;
        }

        /// <summary>
        /// Converts the value of a specified object to an equivalent string representation using specified format and culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// The string representation of the value of <paramref name="arg"/>, formatted as specified by <paramref name="format"/> and <paramref name="formatProvider"/>.
        /// </returns>
        /// <param name="format">A format string containing formatting specifications. </param><param name="arg">An object to format. </param><param name="formatProvider">An object that supplies format information about the current instance. </param>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            // check to see whether we are the ones being called
            if (!this.Equals(formatProvider))
            {
                return null;
            }

            Requires.NotNull(arg, nameof(arg));

            string stringArg;
            if ((stringArg = arg as string) != null)
            {
                if (_forceColumnAsTableColumnResolution && format == "C")
                {
                    format = "TC";
                }
                switch (format)
                {
                    case "TC":
                        return string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}.{1}",
                            this.MainEntitySqlBuilder.GetTableName(),
                            this.MainEntitySqlBuilder.GetColumnName(stringArg));
                    case "T":
                        return this.MainEntitySqlBuilder.GetTableName();
                    case "C":
                        return this.MainEntitySqlBuilder.GetColumnName(stringArg);
                    case "I":
                        return this.MainEntitySqlBuilder.GetDelimitedIdentifier(stringArg);
                }
            }

            IFormattable formattableArg;
            if ((formattableArg = arg as IFormattable) != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }

            return arg.ToString();
        }
    }
}
