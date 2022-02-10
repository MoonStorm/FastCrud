namespace Dapper.FastCrud.Formatters.Formattables
{
    using System;

    /// <summary>
    /// A formattable that can be used straight in the formattable strings representing various clauses in Dapper.FastCrud.
    /// It can also be used on its own through <seealso cref="IFormattable.ToString(string,System.IFormatProvider)"/> or <seealso cref="ToString"/>.
    /// For more information, check the method that was used to create it in <seealso cref="Sql"/>.
    /// </summary>
    public abstract class Formattable : IFormattable
    {
        /// <summary>
        /// Applies formatting to the current instance.
        /// For more information, check the method that was used to create it in <seealso cref="Sql"/>.
        /// </summary>
        /// <param name="format"> An optional format specifier.</param>
        /// <param name="formatProvider">The provider to use to format the value.</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public abstract string ToString(string? format, IFormatProvider? formatProvider = null);

        /// <summary>
        /// Returns the raw representation of the object, which is not SQL ready.
        /// Depending on the usage, this can be the name of the resolved table, column, alias, identifier or parameter.
        /// </summary>
        public override string ToString()
        {
            return this.ToString(null);
        }
    }
}
