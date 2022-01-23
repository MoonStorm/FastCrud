namespace Dapper.FastCrud.EntityDescriptors
{
    using System;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.SqlStatements;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Basic entity descriptor, holding entity mappings for a specific entity type.
    /// </summary>
    internal abstract class EntityDescriptor
    {
        // this can be set by the developer, prior to making any sql statement calls
        // alternatively it is set by us on first usage
        private EntityMapping? _currentEntityMappingRegistration;

        // entity mappings should have a very long timespan if used correctly (they should be stored by the developer and reused), however we can't make that assumption
        // hence we'll have to keep them for the duration of their lifespan and attach precomputed sql statements
        private readonly ConditionalWeakTable<EntityMapping, ISqlStatements> _historicEntityMappingRegistrations;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        protected EntityDescriptor(Type entityType)
        {
            this.EntityType = entityType;
            _historicEntityMappingRegistrations = new ConditionalWeakTable<EntityMapping, ISqlStatements>();
        }

        /// <summary>
        /// Returns the current entity mapping registration.
        /// </summary>
        public EntityMapping CurrentEntityMappingRegistration
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
        protected abstract EntityMapping DefaultEntityMappingRegistration { get; }

        /// <summary>
        /// Returns the sql statements for an entity mapping, or the current one if the argument is null.
        /// </summary>
        public ISqlStatements GetSqlStatements(EntityMapping? entityMapping = null)
        {
            var sqlStatements = _historicEntityMappingRegistrations.GetValue(
                entityMapping ?? this.CurrentEntityMappingRegistration,
                passedMapping => this.ConstructSqlStatements(passedMapping));

            return sqlStatements;
        }

        protected abstract ISqlStatements ConstructSqlStatements(EntityMapping entityMapping);
    }
}
