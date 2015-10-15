namespace Dapper.FastCrud.ParameterResolvers
{
    using System;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Used to properly format the parameters of a query
    /// </summary>
    internal sealed class SqlQueryFormatter:IFormatProvider, ICustomFormatter
    {
        private static readonly Type _customFormatterType = typeof(ICustomFormatter);

        private readonly Type _mainEntityType;
        private readonly ISqlBuilder _mainEntitySqlBuilder;
        private EntityMapping _mainEntityMapping;

        public SqlQueryFormatter(Type mainEntityType, EntityMapping mainEntityMapping, ISqlBuilder mainEntitySqlBuilder)
        {
            _mainEntityType = mainEntityType;
            _mainEntitySqlBuilder = mainEntitySqlBuilder;
            _mainEntityMapping = mainEntityMapping;
        }

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
            if (this.Equals(formatProvider))
            {
                return null;
            }

            return null;
        }
    }
}
