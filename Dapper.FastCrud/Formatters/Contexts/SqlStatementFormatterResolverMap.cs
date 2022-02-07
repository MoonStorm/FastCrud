namespace Dapper.FastCrud.Formatters.Contexts
{
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds a map of active resolvers.
    /// </summary>
    internal class SqlStatementFormatterResolverMap
    {
        private readonly Dictionary<string, SqlStatementFormatterResolver> _resolverMap;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlStatementFormatterResolverMap()
        {
            _resolverMap = new Dictionary<string, SqlStatementFormatterResolver>();
        }

        /// <summary>
        /// Removes a resolver from the map.
        /// </summary>
        public bool RemoveResolver(SqlStatementFormatterResolver resolver)
        {
            Requires.NotNull(resolver, nameof(resolver));

            // if you change these line, change the other methods as well
            if (resolver.Alias != null)
            {
                return _resolverMap.Remove(resolver.Alias);
            }

            return _resolverMap.Remove(resolver.EntityRegistration.EntityType.Name) && _resolverMap.Remove(resolver.EntityRegistration.TableName);
        }

        /// <summary>
        /// Adds a new resolver to the map.
        /// </summary>
        /// <exception cref="InvalidOperationException">A resolver with the same alias/table/entity already exists. A unique alias must be provided.</exception>
        public void AddResolver(SqlStatementFormatterResolver resolver)
        {
            Requires.NotNull(resolver, nameof(resolver));

            // if you change these line, change the other methods as well
            if (resolver.Alias != null)
            {
                // if an alias was provided, check to see if it's unique
                if (_resolverMap.ContainsKey(resolver.Alias))
                {
                    throw new InvalidOperationException($"The alias '{resolver.Alias}' is not unique.");
                }

                _resolverMap.Add(resolver.Alias, resolver);
            }
            else
            {
                // otherwise check to see if both entity type and database table are unique (they might both be used for resolution, both internally and externally)
                if (_resolverMap.ContainsKey(resolver.EntityRegistration.EntityType.Name))
                {
                    throw new InvalidOperationException($"The type '{resolver.EntityRegistration.EntityType.Name}' was already used. Please use a unique alias instead.");
                }

                if (_resolverMap.ContainsKey(resolver.EntityRegistration.TableName))
                {
                    throw new InvalidOperationException($"The table '{resolver.EntityRegistration.TableName}' was already used. Please use a unique alias instead.");
                }

                _resolverMap.Add(resolver.EntityRegistration.EntityType.Name, resolver);
                _resolverMap.Add(resolver.EntityRegistration.TableName, resolver);
            }
        }

        /// <summary>
        /// Resolves a formatter.
        /// </summary>
        public SqlStatementFormatterResolver this[string aliasOrTableOrTypeName]
        {
            get
            {
                Requires.NotNullOrEmpty(aliasOrTableOrTypeName, nameof(aliasOrTableOrTypeName));

                if (!_resolverMap.TryGetValue(aliasOrTableOrTypeName, out SqlStatementFormatterResolver resolver))
                {
                    throw new InvalidOperationException($"Unable to resolve '{aliasOrTableOrTypeName}'");
                }

                return resolver;
            }
        }


    }
}
