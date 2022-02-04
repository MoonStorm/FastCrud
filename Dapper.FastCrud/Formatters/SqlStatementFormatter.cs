namespace Dapper.FastCrud.Formatters
{
    using System;
    using System.Globalization;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System.Collections.Generic;

    /// <summary>
    /// Used to properly format the parameters of a query
    /// </summary>
    internal sealed class SqlStatementFormatter:IFormatProvider, ICustomFormatter
    {
        private static readonly Type _customFormatterType = typeof(ICustomFormatter);
        private readonly Dictionary<string, SqlStatementFormatterEntityAttendant> _referencedTableEntityMap;
        private SqlStatementFormatterEntityAttendant _recentlyActiveAttendant;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlStatementFormatter()
        {
            _referencedTableEntityMap = new Dictionary<string, SqlStatementFormatterEntityAttendant>();
        }

        /// <summary>
        /// Adds a new known reference.
        /// </summary>
        public void AddAsKnownReference<TEntity>(EntityMapping<TEntity>? entityMappingOverride = null, string? alias = null)
        {
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            var entityRegistration = entityMappingOverride?.Registration ?? entityDescriptor.CurrentEntityMappingRegistration;
            var sqlStatements = entityDescriptor.GetSqlStatements(entityRegistration);
            var sqlBuilder = sqlStatements.SqlBuilder;

            this.AddAsKnownReference(entityRegistration, sqlBuilder, alias);
        }

        /// <summary>
        /// Adds a new known reference.
        /// </summary>
        public void AddAsKnownReference(EntityRegistration entityRegistration, ISqlBuilder sqlBuilder, string? alias = null)
        {
            Requires.NotNull(entityRegistration, nameof(entityRegistration));
            Requires.NotNull(sqlBuilder, nameof(sqlBuilder));

            var entityAttendant = new SqlStatementFormatterEntityAttendant(entityRegistration, sqlBuilder, alias);
            var knownAttendantKey = alias ?? entityRegistration.TableName;
            if (_referencedTableEntityMap.ContainsKey(knownAttendantKey))
            {
                if (alias == null)
                {
                    throw new ArgumentException(
                        $"The table or alias '{entityRegistration.TableName}' was already added as a known reference. Assign a unique alias every time you use the table '{entityRegistration.TableName}' in this query.",
                        nameof(alias));
                }
                else
                {
                    throw new ArgumentException($"The alias '{alias}' was already added as a known reference. Pick a unique alias for this reference.");
                }
            }

            // use this reference as the most recent active one
            _recentlyActiveAttendant = entityAttendant;
        }

        /// <summary>
        /// Returns an object that provides formatting services for the specified type.
        /// </summary>
        /// <returns>
        /// An instance of the object specified by <paramref name="formatType"/>, if the <see cref="T:System.IFormatProvider"/> implementation can supply that type of object; otherwise, null.
        /// </returns>
        /// <param name="formatType">An object that specifies the type of format object to return. </param>
        public object? GetFormat(Type formatType)
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
        public string Format(string format, object? arg, IFormatProvider formatProvider)
        {
            Requires.NotNull(formatProvider, nameof(formatProvider));
            Requires.Argument(formatProvider == this, nameof(formatProvider),"Invalid format provider provided to Dapper.FastCrud");

            if (_recentlyActiveAttendant == null)
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
                            // input: "typeOrAlias"
                            // output: "[tableOrAlias]"
                            return this.FormatAliasOrTable(stringArg);
                        case "TC":
                            // input: "typeOrAlias.prop" OR "prop"
                            // output: "[tableOrAlias].[column]"
                            return this.FormatAliasOrTableWithColumn(stringArg);
                        case "C":
                            // input: "prop"
                            // output: "[column]"
                            // just a column (note: in legacy this was sometimes forced with a table in certain circumstances)
                            return this.FormatColumn(stringArg);
                        default:
                            throw new InvalidOperationException($"Unknown format specifier '{format}' for Dapper.FastCrud");
                    }
                case Type typeArg:
                        return this.Format(format, typeArg.Name.ToString(CultureInfo.InvariantCulture), formatProvider);
                default:
                    // try again, but this time with the object converted to a string
                    return this.Format(format, arg?.ToString()??string.Empty, formatProvider);
            }
        }

        private string FormatParameter(string parameter)
        {
            return string.Format(CultureInfo.InvariantCulture, "@{0}", parameter);
        }

        private string FormatIdentifier(string identifier)
        {
            return _recentlyActiveAttendant.SqlBuilder.GetDelimitedIdentifier(identifier);
        }

        private string FormatAliasOrTable(string? aliasOrTable)
        {
            if (aliasOrTable != null)
            {
                this.RecordAttendantActivityWithTableOrAlias(aliasOrTable);
            }

            if (_recentlyActiveAttendant.Alias == null)
            {
                return _recentlyActiveAttendant.SqlBuilder.GetTableName();
            }

            return this.FormatIdentifier(_recentlyActiveAttendant.Alias);
        }

        private string FormatColumn(string column)
        {
            return _recentlyActiveAttendant.SqlBuilder.GetColumnName(column);
        }

        private string FormatAliasOrTableWithColumn(string aliasOrTableWithColumn)
        {
            var dotIndex = aliasOrTableWithColumn.IndexOf('.');
            string column;
            string? tableOrAlias;
            if (dotIndex >= 0)
            {
                // we were given both a table/alias AND a column
                tableOrAlias = aliasOrTableWithColumn.Substring(0, dotIndex);
                column = aliasOrTableWithColumn.Substring(dotIndex + 1);
            }
            else
            {
                // we were given no table/alias but it's expected of us to return one
                tableOrAlias = null;
                column = aliasOrTableWithColumn;
            }

            return FormattableString.Invariant($"{this.FormatAliasOrTable(tableOrAlias)}.{this.FormatColumn(column)}");
        }

        private void RecordAttendantActivityWithTableOrAlias(string aliasOrTable)
        {
            if (aliasOrTable == null)
            {
                throw new InvalidOperationException("Empty alias or table");
            }

            if (!_referencedTableEntityMap.TryGetValue(aliasOrTable, out SqlStatementFormatterEntityAttendant attendantInfo))
            {
                throw new InvalidOperationException($"Unknown reference '{aliasOrTable}' being used as a table or alias");
            }

            _recentlyActiveAttendant = attendantInfo;
        }
    }
}
