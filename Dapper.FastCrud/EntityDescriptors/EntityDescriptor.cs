namespace Dapper.FastCrud.EntityDescriptors
{
    using System;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlStatements;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Basic entity descriptor, holding entity mappings for a specific entity type.
    /// </summary>
    internal abstract class EntityDescriptor
    {
        // this can be set by the developer, prior to making any sql statement calls
        // alternatively it is set by us on first usage
        private volatile EntityRegistration? _currentEntityMappingRegistration;

        // entity mappings should have a very long timespan if used correctly (they should be stored by the developer and reused), however we can't make that assumption
        // hence we'll have to keep them for the duration of their lifespan and attach precomputed sql statements
        private readonly ConditionalWeakTable<EntityRegistration, ISqlStatements> _historicSqlStatements;
        private readonly ConditionalWeakTable<EntityRegistration, ISqlBuilder> _historicSqlBuilder;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected EntityDescriptor(Type entityType)
        {
            this.EntityType = entityType;
            _historicSqlStatements = new ConditionalWeakTable<EntityRegistration, ISqlStatements>();
            _historicSqlBuilder = new ConditionalWeakTable<EntityRegistration, ISqlBuilder>();
        }

        /// <summary>
        /// Returns the current entity mapping registration.
        /// </summary>
        public EntityRegistration CurrentEntityMappingRegistration
        {
            get
            {
                if (_currentEntityMappingRegistration == null)
                {
                    _currentEntityMappingRegistration = this.DefaultEntityMappingRegistration;
                }

                return _currentEntityMappingRegistration;
            }
            set
            {
                _currentEntityMappingRegistration = value;
            }
        }

        /// <summary>
        /// Gets the associated entity type.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Returns the default entity mapping registration.
        /// </summary>
        protected abstract EntityRegistration DefaultEntityMappingRegistration { get; }

        /// <summary>
        /// Returns the sql builder for an entity mapping, or the current one if the argument is null.
        /// </summary>
        public ISqlBuilder GetSqlBuilder(EntityRegistration? entityRegistration = null)
        {
            var sqlStatements = _historicSqlBuilder.GetValue(
                entityRegistration ?? this.CurrentEntityMappingRegistration,
                this.ConstructSqlBuilder);

            return sqlStatements;
        }

        /// <summary>
        /// Returns the sql statements for a single entity, attached to the default entity registration or an overriden entity registration if provided.
        /// </summary>
        protected ISqlStatements GetSqlStatements(EntityRegistration? entityRegistration = null)
        {
            var sqlStatements = _historicSqlStatements.GetValue(
                entityRegistration ?? this.CurrentEntityMappingRegistration,
                this.ConstructSqlStatements);

            return sqlStatements;
        }
        protected abstract ISqlStatements ConstructSqlStatements(EntityRegistration entityRegistration);
        protected abstract ISqlBuilder ConstructSqlBuilder(EntityRegistration entityRegistration);
    }
}
