using System;

namespace Dapper.FastCrud.Mappings
{
    /// <summary>
    /// Denotes that a column excluded from update operations
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ExcludeUpdateAttribute : Attribute
    {

    }
}
