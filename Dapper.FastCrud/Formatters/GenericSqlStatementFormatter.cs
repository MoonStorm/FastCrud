namespace Dapper.FastCrud.Formatters
{
    using Dapper.FastCrud.Validations;
    using System;
    using System.Globalization;

    /// <summary>
    /// Offers SQL basic formatting capabilities.
    /// </summary>
    internal abstract class GenericSqlStatementFormatter: IFormatProvider, ICustomFormatter
    {
        private static readonly Type _customFormatterType = typeof(ICustomFormatter);

        /// <summary>Returns an object that provides formatting services for the specified type.</summary>
        /// <param name="formatType">An object that specifies the type of format object to return.</param>
        /// <returns>An instance of the object specified by <paramref name="formatType" />, if the <see cref="T:System.IFormatProvider" /> implementation can supply that type of object; otherwise, <see langword="null" />.</returns>
        public object? GetFormat(Type formatType)
        {
            if (formatType == _customFormatterType)
            {
                return this;
            }

            return null;
        }

        /// <summary>Converts the value of a specified object to an equivalent string representation using specified format and culture-specific formatting information.</summary>
        /// <param name="format">A format string containing formatting specifications.</param>
        /// <param name="arg">An object to format.</param>
        /// <param name="formatProvider">An object that supplies format information about the current instance.</param>
        /// <returns>The string representation of the value of <paramref name="arg" />, formatted as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            Requires.NotNull(formatProvider, nameof(formatProvider));
            Requires.Argument(formatProvider == this, nameof(formatProvider), "Invalid format provider provided to Dapper.FastCrud");

            if (this.ActiveResolver == null)
            {
                throw new InvalidOperationException("No valid attendant present to satisfy the Dapper.FastCrud formatting request");
            }

            switch (arg)
            {
                case IFormattable formattableArg:
                    return formattableArg.ToString(format, formatProvider);
                case string stringArg:
                    if (string.IsNullOrEmpty(stringArg))
                    {
                        throw new InvalidOperationException("Empty argument passed as an argument to the Dapper.FastCrud formatter");
                    }
                    switch (format)
                    {
                        case "P":
                            // input: "param"
                            // output: "@param"
                            return this.FormatParameter(stringArg);
                        case "I":
                            // input: "anything"
                            // output: "[anything]"
                            return this.FormatIdentifier(stringArg);
                        case "T":
                            // input: "alias"|""
                            // output: "[alias_or_current]"
                            return this.FormatAliasOrNothing(stringArg);
                        case "TC":
                            // input: "typeOrAlias.prop" OR "prop"
                            // output: "[tableOrAlias].[column]"
                            return this.FormatQualifiedColumn(stringArg);
                        case "C":
                            // input: "prop"
                            // output: "[column]"
                            // just a column (note: sometimes this is being forced with a table in certain circumstances)
                            return this.FormatColumn(stringArg);
                        default:
                            throw new InvalidOperationException($"Unknown format specifier '{format}' specified for a string argument to Dapper.FastCrud");
                    }
                // disabled, needs to be reviewed
                //case Type typeArg:
                //    if (typeArg == null)
                //    {
                //        throw new InvalidOperationException("Null argument passed as an argument to the Dapper.FastCrud formatter");
                //    }

                //    switch (format)
                //    {
                //        case "T":
                //            // input: EntityType
                //            // output: "[alias_or_table]"
                //            return this.FormatType(typeArg);
                //        default:
                //            throw new InvalidOperationException($"Unknown format specifier '{format}' specified for a Type argument to Dapper.FastCrud");
                //    }

                //    return this.Format(format, typeArg.Name.ToString(CultureInfo.InvariantCulture), formatProvider);
                // TODO: implement support for formattable types and properties
                default:
                    // try again, but this time with the object converted to a string
                    return this.Format(format, arg?.ToString() ?? string.Empty, formatProvider);
            }
        }

        /// <summary>
        /// Gets the currently active resolver, if one is present.
        /// </summary>
        protected abstract SqlStatementFormatterResolver? ActiveResolver { get; }

        /// <summary>
        /// Formats a regular parameter.
        /// </summary>
        protected virtual string FormatParameter(string parameter)
        {
            return string.Format(CultureInfo.InvariantCulture, "@{0}", parameter);
        }

        /// <summary>
        /// Formats an identifier.
        /// </summary>
        protected virtual string FormatIdentifier(string identifier)
        {
            return this.ActiveResolver!.SqlBuilder.GetDelimitedIdentifier(identifier);
        }

        /// <summary>
        /// Formats a column in the context of the current resolver.
        /// </summary>
        protected virtual string FormatColumn(string propName)
        {
            return this.ActiveResolver!.SqlBuilder.GetColumnName(propName);
        }

        /// <summary>
        /// Formats an entity type.
        /// </summary>
        //protected virtual string FormatType(Type type)
        //{
        //    if (this.ActiveResolver!.EntityRegistration.EntityType == type)
        //    {
        //        return this.ActiveResolver.Alias ?? this.ActiveResolver.SqlBuilder.GetTableName();
        //    }
        //    throw new InvalidOperationException($"Unable to resolve type '{type}'. It is not known as part of this conversation.");
        //}

        /// <summary>
        /// Formats an alias.
        /// </summary>
        protected virtual string FormatAliasOrNothing(string? aliasOrNothing)
        {
            if (aliasOrNothing != null)
            {
                return this.FormatIdentifier(aliasOrNothing);
            }

            if (this.ActiveResolver!.Alias != null)
            {
                return this.FormatIdentifier(this.ActiveResolver!.Alias);
            }

            return this.ActiveResolver.SqlBuilder.GetTableName();
        }

        /// <summary>
        /// Formats an aliased qualified column, or in case the alias is not provided, the currently active resolver's table or alias.
        /// </summary>
        protected virtual string FormatQualifiedColumn(string? aliasOrNothing, string propName)
        {
            string formattedAliasOrTable;
            if (aliasOrNothing != null)
            {
                formattedAliasOrTable = this.FormatIdentifier(aliasOrNothing);
            }
            else if (this.ActiveResolver!.Alias != null)
            {
                formattedAliasOrTable = this.FormatIdentifier(this.ActiveResolver!.Alias);
            }
            else
            {
                formattedAliasOrTable = this.ActiveResolver.SqlBuilder.GetTableName();
            }

            string formattedColumnName;
            formattedColumnName = this.ActiveResolver.SqlBuilder.GetColumnName(propName);

            return FormattableString.Invariant($"{formattedAliasOrTable}.{formattedColumnName}");
        }

        private string FormatQualifiedColumn(string qualifiedColumn)
        {
            // multiple dots should not be present in here!
            var dotIndex = qualifiedColumn.IndexOf('.');
            string column;
            string? alias;
            if (dotIndex >= 0)
            {
                // we were given both a alias AND a column
                alias = qualifiedColumn.Substring(0, dotIndex);
                column = qualifiedColumn.Substring(dotIndex + 1);
            }
            else
            {
                // we were given no table/alias but it's expected of us to return one
                alias = null;
                column = qualifiedColumn;
            }

            return this.FormatQualifiedColumn(alias, column);
        }

    }
}
