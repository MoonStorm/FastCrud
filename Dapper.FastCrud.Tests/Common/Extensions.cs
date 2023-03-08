namespace Dapper.FastCrud.Tests.Common
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Dapper.FastCrud.Validations;
    using Microsoft.Extensions.Configuration;

    public static class Extensions
    {
        /// <summary>
        /// Gets the full path to the directory holding the assembly
        /// </summary>
        /// <returns></returns>
        public static string GetDirectory(this Assembly assembly)
        {
            Validate.NotNull(assembly, nameof(assembly));

            //The CodeBase is a URL to the place where the file was found, while the Location is the path from where it was actually loaded.
            //For example, if the assembly was downloaded from the internet, its CodeBase may start with “http://”, but its Location may start with “C:\”. 
            // If the file was shadow copied, the Location would be the path to the copy of the file in the shadow-copy dir.
            //It’s also good to know that the CodeBase is not guaranteed to be set for assemblies in the GAC.Location will always be set for assemblies loaded from disk, however.
            //return Path.GetDirectoryName(assembly.Location);
            return Path.GetDirectoryName(new System.Uri(assembly.CodeBase).LocalPath);
        }

        public static TEntity Clone<TEntity>(this TEntity entity) where TEntity : class,new()
        {
            var clone = new TEntity();
            foreach(var propInfo in typeof(TEntity).GetProperties())
            {
                var propValue = propInfo.GetValue(entity);
                propInfo.SetValue(clone, propValue);
            }

            return clone;
        }
    }
}
