using System;

namespace Dapper.FastCrud.Tests.Common
{
    using System.Data;
    using System.Globalization;
    using Dapper;

    /// <summary>
    /// Temporary type handler for the <see cref="DateOnly"/> type until we get full support.
    /// </summary>
    internal class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        private readonly NullableDateOnlyTypeHandler _nullableDateOnlyTypeHandler = new();

        /// <summary>
        /// Assign the value of a parameter before a command executes
        /// </summary>
        /// <param name="parameter">The parameter to configure</param>
        /// <param name="value">Parameter value</param>
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            _nullableDateOnlyTypeHandler.SetValue(parameter, value);
        }

        /// <summary>Parse a database value back to a typed value</summary>
        /// <param name="value">The value from the database</param>
        /// <returns>The typed value</returns>
        public override DateOnly Parse(object value)
        {
            return _nullableDateOnlyTypeHandler.Parse(value)!.Value;
        }
    }

    /// <summary>
    /// Temporary type handler for the <see cref="DateOnly"/> type until we get full support.
    /// </summary>
    internal class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
    {
        /// <summary>
        /// Assign the value of a parameter before a command executes
        /// </summary>
        /// <param name="parameter">The parameter to configure</param>
        /// <param name="value">Parameter value</param>
        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            switch (OrmConfiguration.DefaultDialect)
            {
                case SqlDialect.MySql:
                    parameter.DbType = DbType.String;
                    if (value != null)
                    {
                        // convert to ISO
                        parameter.Value = value.Value.ToString("O", CultureInfo.InvariantCulture);
                    }
                    break;
                case SqlDialect.SqlAnywhere:
                    parameter.DbType = DbType.DateTime;
                    if (value != null)
                    {
                        parameter.Value = value.Value.ToDateTime(new TimeOnly());
                    }
                    break;
                default:
                    parameter.DbType = DbType.Date;
                    if (value != null)
                    {
                        parameter.Value = value.Value;
                    }
                    break;
            }
        }

        /// <summary>Parse a database value back to a typed value</summary>
        /// <param name="value">The value from the database</param>
        /// <returns>The typed value</returns>
        public override DateOnly? Parse(object value)
        {
            if (ReferenceEquals(value, null))
            {
                return null;
            }

            if (value is DateTime dateTimeValue)
            {
                return DateOnly.FromDateTime(dateTimeValue);
            }

            throw new NotSupportedException($"Don't know how to convert '{value.GetType()}' to DateOnly");
        }
    }
}
