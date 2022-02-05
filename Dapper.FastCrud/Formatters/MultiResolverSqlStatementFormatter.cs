namespace Dapper.FastCrud.Formatters
{
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Offers SQL formatting capabilities in a space where multiple resolvers can be active.
    /// </summary>
    internal class MultiResolverSqlStatementFormatter : GenericSqlStatementFormatter
    {
        private readonly Dictionary<string, SqlStatementFormatterResolver> _referencedTableEntityMap;
        private SqlStatementFormatterResolver? _activeResolver;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultiResolverSqlStatementFormatter()
        {
            _referencedTableEntityMap = new Dictionary<string, SqlStatementFormatterResolver>();
        }

        /// <summary>
        /// Gets the currently active resolver, if one is present.
        /// </summary>
        protected override SqlStatementFormatterResolver? ActiveResolver => _activeResolver;

        /// <summary>
        /// Adds a new known reference.
        /// </summary>
        public void AddAsKnownReference<TEntity>(EntityMapping<TEntity>? entityMappingOverride = null, string? alias = null)
        {
            var entityDescriptor = OrmConfiguration.GetEntityDescriptor<TEntity>();
            var entityRegistration = entityMappingOverride?.Registration ?? entityDescriptor.CurrentEntityMappingRegistration;
            var sqlBuilder = entityDescriptor.GetSqlBuilder(entityRegistration);

            this.AddAsKnownReference(entityRegistration, sqlBuilder, alias);
        }

        /// <summary>
        /// Adds a new known reference.
        /// </summary>
        public void AddAsKnownReference(EntityRegistration entityRegistration, ISqlBuilder sqlBuilder, string? alias = null)
        {
            Requires.NotNull(entityRegistration, nameof(entityRegistration));
            Requires.NotNull(sqlBuilder, nameof(sqlBuilder));

            var entityResolver = new SqlStatementFormatterResolver(entityRegistration, sqlBuilder, alias);
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
            _activeResolver = entityResolver;
        }

        /// <summary>
        /// Formats an alias or a table.
        /// </summary>
        protected override string FormatAliasOrNothing(string? aliasOrNothing)
        {
            if (aliasOrNothing != null)
            {
                this.RecordResolverActivityWithTableOrAlias(aliasOrNothing);
            }

            return base.FormatAliasOrNothing(aliasOrNothing);
        }

        /// <summary>
        /// Formats an aliased qualified column, or in case the alias is not provided, the currently active resolver's table or alias.
        /// </summary>
        protected override string FormatQualifiedColumn(string? aliasOrNothing, string propName)
        {
            if (aliasOrNothing != null)
            {
                this.RecordResolverActivityWithTableOrAlias(aliasOrNothing);
            }

            return base.FormatQualifiedColumn(aliasOrNothing, propName);
        }

        private void RecordResolverActivityWithTableOrAlias(string aliasOrTable)
        {
            if (aliasOrTable == null)
            {
                throw new InvalidOperationException("Empty alias or table");
            }

            if (!_referencedTableEntityMap.TryGetValue(aliasOrTable, out SqlStatementFormatterResolver attendantInfo))
            {
                throw new InvalidOperationException($"Unknown reference '{aliasOrTable}' being used as a table or alias");
            }

            _activeResolver = attendantInfo;
        }
    }
}
