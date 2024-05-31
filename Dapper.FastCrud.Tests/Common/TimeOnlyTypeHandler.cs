using System;

namespace Dapper.FastCrud.Tests.Common
{
    using System.Data;
    using System.Globalization;
    using Dapper;

    /// <summary>
    /// Temporary type handler for the <see cref="TimeOnly"/> type until we get full support.
    /// </summary>
    internal class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        private readonly NullableTimeOnlyTypeHandler _nullableTimeOnlyTypeHandler = new();

        /// <summary>
        /// Assign the value of a parameter before a command executes
        /// </summary>
        /// <param name="parameter">The parameter to configure</param>
        /// <param name="value">Parameter value</param>
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            _nullableTimeOnlyTypeHandler.SetValue(parameter, value);
        }

        /// <summary>Parse a database value back to a typed value</summary>
        /// <param name="value">The value from the database</param>
        /// <returns>The typed value</returns>
        public override TimeOnly Parse(object value)
        {
            return _nullableTimeOnlyTypeHandler.Parse(value)!.Value;
        }
    }

    /// <summary>
    /// Temporary type handler for the <see cref="TimeOnly"/> type until we get full support.
    /// </summary>
    internal class NullableTimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly?>
    {
        /// <summary>
        /// Assign the value of a parameter before a command executes
        /// </summary>
        /// <param name="parameter">The parameter to configure</param>
        /// <param name="value">Parameter value</param>
        public override void SetValue(IDbDataParameter parameter, TimeOnly? value)
        {
            switch (OrmConfiguration.DefaultDialect)
            {
                case SqlDialect.MySql:
                    parameter.DbType = DbType.String;
                    if (value != null)
                    {
                        // convert to ISO 8601
                        parameter.Value = value.Value.ToString("O", CultureInfo.InvariantCulture);
                    }
                    break;
                case SqlDialect.SqlAnywhere:
                    parameter.DbType = DbType.Time;
                    if (value != null)
                    {
                        parameter.Value = value.Value.ToTimeSpan();
                    }
                    break;
                default:
                    parameter.DbType = DbType.Time;
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
        public override TimeOnly? Parse(object value)
        {
            if (ReferenceEquals(value, null))
            {
                return null;
            }

            if (value is DateTime dateTimeValue)
            {
                return TimeOnly.FromDateTime(dateTimeValue);
            }

            if (value is TimeSpan timeSpanValue)
            {
                return TimeOnly.FromTimeSpan(timeSpanValue);
            }

            throw new NotSupportedException($"Don't know how to convert '{value.GetType()}' to TimeOnly");
        }
    }
}
