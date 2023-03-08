namespace Dapper.FastCrud.Formatters
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Formatters.Contexts;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The central point for all the formatting capabilities.
    /// Only one instance exists for the duration of a statement.
    /// Every participant in the statement, including the main entity, will register themselves with the formatter and receive a unique resolver.
    /// </summary>
    internal class GenericSqlStatementFormatter: IFormatProvider, ICustomFormatter
    {
        private static readonly Type _customFormatterType = typeof(ICustomFormatter);
        private readonly SqlStatementFormatterResolverMap _resolverMap = new SqlStatementFormatterResolverMap();
        private readonly Stack<SqlStatementFormatterResolver> _activeResolverStack = new Stack<SqlStatementFormatterResolver>();
        private readonly Stack<bool> _activeResolverRequiresFullyQualifiedColumns = new Stack<bool>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public GenericSqlStatementFormatter()
        {
        }

        /// <summary>
        /// Replaces one resolver with another. Note that this won't affect the active resolver.
        /// <exception cref="InvalidOperationException">The old registration could not be found or the new one is invalid.</exception>
        /// </summary>
        public SqlStatementFormatterResolver ReplaceRegisteredResolver(
            EntityDescriptor entityDescriptor,
            EntityRegistration oldRegistration, 
            string? oldAlias, 
            EntityRegistration newRegistration, 
            string? newAlias)
        {
            if (!_resolverMap.RemoveResolver(oldRegistration, oldAlias))
            {
                throw new InvalidOperationException("An old resolver could not be located");
            }

            var newResolver = new SqlStatementFormatterResolver(entityDescriptor, newRegistration, newAlias);
            _resolverMap.AddResolver(newResolver);
            return newResolver;
        }

        /// <summary>
        /// Locates a resolver.
        /// <exception cref="InvalidOperationException">The resolver could not be located.</exception>
        /// </summary>
        public SqlStatementFormatterResolver LocateResolver(
            Type entityType,
            string? alias)
        {
            return _resolverMap.LocateResolver(entityType, alias);
        }

        /// <summary>
        /// Registers a new resolver. Note that this is not gonna replace the existing active main resolver. 
        /// </summary>
        public SqlStatementFormatterResolver RegisterResolver(EntityDescriptor entityDescriptor,
                                                              EntityRegistration? registrationOverride,
                                                              string? alias)
        {
            var newResolver = new SqlStatementFormatterResolver(entityDescriptor, registrationOverride??entityDescriptor.CurrentEntityMappingRegistration, alias);
            _resolverMap.AddResolver(newResolver);
            return newResolver;
        }

        /// <summary>
        /// Gets all the formatters registered, excluding the set provided.
        /// </summary>
        public SqlStatementFormatterResolver[] GetAllFormatterResolversExcluding(params SqlStatementFormatterResolver[] unwantedResolvers)
        {
            return _resolverMap.GetAllFormatterResolversExcluding(unwantedResolvers);
        }

        /// <summary>
        /// Sets the main resolver used for incomplete column resolutions.
        /// The resolver MUST be registered first with <seealso cref="RegisterResolver"/>.
        /// Call the result's <seealso cref="IDisposable.Dispose"/> to restore the previous main resolver.
        /// </summary>
        public IDisposable SetActiveMainResolver(SqlStatementFormatterResolver resolver, bool forceFullyQualifiedColumns)
        {
            Validate.NotNull(resolver, nameof(resolver));

            _activeResolverStack.Push(resolver);
            _activeResolverRequiresFullyQualifiedColumns.Push(forceFullyQualifiedColumns);
            return new MainResolverRestoreHelper(this, resolver);
        }

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
        public string? Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            // Check whether this is an appropriate callback
            if (!this.Equals(formatProvider))
            {
                return null;
            }

            ParseFormat(format, out string? parsedFormat, out string? parsedAlias);

            switch (arg)
            {
                // take care of formattables, either ours or others (see the Formattables folder)
                case IFormattable formattableArg:
                    return formattableArg.ToString(format, formatProvider);
                // anything that's a string
                case string stringArg:
                    if (string.IsNullOrEmpty(stringArg))
                    {
                        throw new InvalidOperationException("Empty argument passed as an argument to the Dapper.FastCrud formatter");
                    }

                    // if we got no format but we got an alias, we'll default to fully qualified column
                    if (parsedAlias != null && parsedFormat == null)
                    {
                        parsedFormat = FormatSpecifiers.FullyQualifiedColumn;
                    }

                    switch (parsedFormat)
                    {
                        case FormatSpecifiers.Parameter:
                            // input: "param"
                            // output: "@param"
                            return this.FormatParameter(stringArg);
                        case FormatSpecifiers.Identifier:
                            // input: "anything"
                            // output: "[anything]"
                            return this.FormatIdentifier(stringArg);
                        case FormatSpecifiers.TableOrAlias:
                            // input: "alias"|"type"|""
                            // output: "[alias_or_table_or_current]"
                            return this.FormatTypeOrAliasOrNothing(parsedAlias ?? stringArg);
                        case FormatSpecifiers.FullyQualifiedColumn:
                            // input: "prop"
                            // output: "[tableOrAlias].[column]"
                            return this.FormatQualifiedColumn(parsedAlias, stringArg);
                        case FormatSpecifiers.SingleColumn:
                            // input: "prop"
                            // output: "[column]"
                            // just a column (note: legacy extra where/order conditions are forcing this with the current table or alias)
                            return this.FormatColumn(stringArg);
                        case null:
                            return stringArg;
                        default:
                            throw new InvalidOperationException($"Unknown format specifier '{format}' specified for a string argument to Dapper.FastCrud");
                    }
                    break;
                case Type typeArg:

                    if (parsedFormat == null)
                    {
                        parsedFormat = FormatSpecifiers.TableOrAlias;
                    }

                    switch (parsedFormat)
                    {
                        case FormatSpecifiers.TableOrAlias:
                            // input: "type"
                            // output: "[alias_or_table]"
                            return this.FormatTypeOrAliasOrNothing(typeArg.Name);
                        default:
                            throw new InvalidOperationException($"Unknown format specifier '{format}' specified for a type argument to Dapper.FastCrud");
                    }
                default:
                    // try again, but this time with the object converted to a string
                    return this.Format(format, arg?.ToString() ?? string.Empty, formatProvider);
            }
        }

        /// <summary>
        /// Gets the currently active resolver.
        /// </summary>
        public SqlStatementFormatterResolver MainActiveResolver => _activeResolverStack.Peek();

        /// <summary>
        /// If true, simple column names will never show up in the formatted string.
        /// They will always be prepended with the table name or alias.
        /// </summary>
        public bool ForceFullyQualifiedColumns => _activeResolverRequiresFullyQualifiedColumns.Peek();

        /// <summary>
        /// Resolves a format that might contain alias information.
        /// </summary>
        public static void ParseFormat(string? format, out string? parsedFormat, out string? parsedAlias)
        {
            parsedFormat = format;
            parsedAlias = null;

            var aliasWithFormat = format?.Split(':');
            switch (aliasWithFormat?.Length)
            {
                case 2:
                    if (aliasWithFormat[0].StartsWith(FormatSpecifiers.BelongsToAliasPrefix))
                    {
                        parsedAlias = aliasWithFormat[0].Substring(FormatSpecifiers.BelongsToAliasPrefix.Length);
                        parsedFormat = aliasWithFormat[1];
                    }
                    else
                    {
                        // we don't know what it is, it will likely error later
                    }
                    break;
                case 1:
                    if (format.StartsWith(FormatSpecifiers.BelongsToAliasPrefix))
                    {
                        parsedAlias = format.Substring(FormatSpecifiers.BelongsToAliasPrefix.Length);
                        parsedFormat = null;
                    }
                    break;
            }

            if (string.IsNullOrWhiteSpace(parsedFormat))
            {
                parsedFormat = null;
            }

            if (string.IsNullOrWhiteSpace(parsedAlias))
            {
                parsedAlias = null;
            }
        }

        /// <summary>
        /// Formats a regular parameter.
        /// </summary>
        protected string FormatParameter(string parameter)
        {
            return this.MainActiveResolver.SqlBuilder.GetPrefixedParameter(parameter);
        }

        /// <summary>
        /// Formats an identifier.
        /// </summary>
        protected string FormatIdentifier(string identifier)
        {
            return this.MainActiveResolver.SqlBuilder.GetDelimitedIdentifier(identifier);
        }

        /// <summary>
        /// Formats a column in the context of the current resolver.
        /// </summary>
        protected virtual string FormatColumn(string propName)
        {
            if (this.ForceFullyQualifiedColumns)
            {
                return this.FormatQualifiedColumn(null, propName);
            }

            return this.MainActiveResolver.SqlBuilder.GetColumnName(propName);
        }

        /// <summary>
        /// Formats an alias.
        /// </summary>
        protected virtual string FormatTypeOrAliasOrNothing(string? typeOrAliasOrNothing)
        {
            SqlStatementFormatterResolver resolverToUse = this.MainActiveResolver;

            if (typeOrAliasOrNothing != null)
            {
                resolverToUse = _resolverMap[typeOrAliasOrNothing];
            }

            return this.FormatIdentifier(resolverToUse.Alias??resolverToUse.EntityRegistration.TableName);
        }

        /// <summary>
        /// Formats an aliased qualified column, or in case the alias is not provided, the currently active resolver's table or alias.
        /// </summary>
        protected virtual string FormatQualifiedColumn(string? typeOrAliasOrNothing, string propName)
        {
            SqlStatementFormatterResolver resolverToUse = this.MainActiveResolver;

            if (typeOrAliasOrNothing != null)
            {
                resolverToUse = this._resolverMap[typeOrAliasOrNothing];
            }
            typeOrAliasOrNothing = resolverToUse.Alias ?? resolverToUse.EntityRegistration.TableName;

            return resolverToUse.SqlBuilder.GetColumnName(propName, typeOrAliasOrNothing);
        }

        private sealed class MainResolverRestoreHelper:IDisposable
        {
            private readonly SqlStatementFormatterResolver _expectedLastResolver;
            private readonly GenericSqlStatementFormatter _formatter;

            public MainResolverRestoreHelper(
                GenericSqlStatementFormatter formatter,
                SqlStatementFormatterResolver expectedLastResolver)
            {
                _formatter = formatter;
                _expectedLastResolver = expectedLastResolver;
            }

            public void Dispose()
            {
                if (_formatter._activeResolverStack.Count == 0 || _formatter._activeResolverStack.Peek() != _expectedLastResolver)
                {
                    throw new InvalidOperationException("The main resolver stack was found to be in an inconsistent state");
                }

                _formatter._activeResolverStack.Pop();
                _formatter._activeResolverRequiresFullyQualifiedColumns.Pop();
            }
        }
    }
}
