namespace Dapper.FastCrud.Formatters.Contexts
{
    using Dapper.FastCrud.Mappings.Registrations;
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
        /// Locates a resolver. In case it can't be found, it will throe an exception.
        /// </summary>
        public SqlStatementFormatterResolver LocateResolver(Type entityType, string? alias)
        {
            Requires.NotNull(entityType, nameof(entityType));

            SqlStatementFormatterResolver? locatedResolver;
            // if you change these line, change the other methods as well
            if (alias != null)
            {
                if (!_resolverMap.TryGetValue(alias, out locatedResolver))
                {
                    throw new InvalidOperationException($"The alias '{alias}' was not registered");
                }
            }
            else
            {
                if (!_resolverMap.TryGetValue(entityType.Name, out locatedResolver))
                {
                    throw new InvalidOperationException($"The type '{entityType}' was not registered");
                }
            }

            if (locatedResolver.EntityRegistration.EntityType != entityType)
            {
                throw new InvalidOperationException($"Mismatched entity registration type located in the resolver map");
            }

            return locatedResolver;
        }

        /// <summary>
        /// Removes a resolver from the map.
        /// </summary>
        public bool RemoveResolver(EntityRegistration registration, string? alias)
        {
            Requires.NotNull(registration, nameof(registration));

            var removed = false;

            // if you change these line, change the other methods as well
            if (alias != null)
            {
                removed = _resolverMap.Remove(alias);
            }
            else
            {
                removed = _resolverMap.Remove(registration.EntityType.Name);
                if (registration.TableName != registration.EntityType.Name)
                {
                    removed &= _resolverMap.Remove(registration.TableName);
                }

            }

            return removed;
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
                if (resolver.EntityRegistration.TableName != resolver.EntityRegistration.EntityType.Name)
                {
                    _resolverMap.Add(resolver.EntityRegistration.TableName, resolver);
                }
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
