// ReSharper disable once CheckNamespace (the namespace is intentionally not in sync with the file location)
namespace Dapper.FastCrud
{
    using System;

    /// <summary>
    /// Denotes that a column has a default value assigned by the database.
    /// Properties marked with this attributes will be ignored on INSERT but refreshed from the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]    
    public class DatabaseGeneratedDefaultValueAttribute:Attribute
    {
    }
}
