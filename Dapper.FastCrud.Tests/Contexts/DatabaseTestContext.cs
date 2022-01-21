namespace Dapper.FastCrud.Tests.Contexts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;
    using Dapper.FastCrud.Tests.Common;

    public class DatabaseTestContext
    {
        private readonly List<DatabaseEntityInstanceInfo> _insertedEntities = new List<DatabaseEntityInstanceInfo>();
        private readonly List<DatabaseEntityInstanceInfo> _queriedEntities = new List<DatabaseEntityInstanceInfo>();
        private readonly List<DatabaseEntityInstanceInfo> _updatedEntities = new List<DatabaseEntityInstanceInfo>();

        public string DatabaseName { get; } = "FastCrudTestDatabaseF8D8B1E3"; // we need something fairly unique here
        public string CurrentExecutionFolder { get; } = typeof(DatabaseTestContext).Assembly.GetDirectory();
        public Stopwatch Stopwatch { get; } = new Stopwatch();
        public DbConnection DatabaseConnection { get; set; }
        public int LastCountQueryResult { get; set; }

        public void RecordInsertedEntity<EntityType>(EntityType entityInstance)
        {
            _insertedEntities.Add(new DatabaseEntityInstanceInfo(typeof(EntityType), entityInstance));
        }

        public void RecordUpdatedEntity<EntityType>(EntityType entityInstance)
        {
            _updatedEntities.Add(new DatabaseEntityInstanceInfo(typeof(EntityType), entityInstance));
        }

        public void RecordQueriedEntity<EntityType>(EntityType entityInstance)
        {
            _queriedEntities.Add(new DatabaseEntityInstanceInfo(typeof(EntityType), entityInstance));
        }

        public object[] GetInsertedEntitiesOfType(Type entityType, int? onlyLastCount = null)
        {
            return this.GetEntitiesOfType(_insertedEntities, entityType, onlyLastCount)
                .ToArray();
        }

        public TEntityType[] GetInsertedEntitiesOfType<TEntityType>(int? onlyLastCount = null)
        {
            return this.GetEntitiesOfType(_insertedEntities, typeof(TEntityType), onlyLastCount)
                       .Select(entity => (TEntityType)entity)
                       .ToArray();
        }

        public object[] GetUpdatedEntitiesOfType(Type entityType, int? onlyLastCount = null)
        {
            return this.GetEntitiesOfType(_updatedEntities, entityType, onlyLastCount)
                       .ToArray();
        }

        public TEntityType[] GetUpdatedEntitiesOfType<TEntityType>(int? onlyLastCount = null)
        {
            return this.GetEntitiesOfType(_updatedEntities, typeof(TEntityType), onlyLastCount)
                       .Select(entity => (TEntityType)entity)
                       .ToArray();
        }

        public object[] GetQueriedEntitiesOfType(Type entityType, int? onlyLastCount = null)
        {
            return this.GetEntitiesOfType(_queriedEntities, entityType, onlyLastCount)
                       .ToArray();
        }

        public TEntityType[] GetQueriedEntitiesOfType<TEntityType>(int? onlyLastCount = null)
        {
            return this.GetEntitiesOfType(_queriedEntities, typeof(TEntityType), onlyLastCount)
                       .Select(entity => (TEntityType)entity)
                       .ToArray();
        }

        private IEnumerable<object> GetEntitiesOfType(IList<DatabaseEntityInstanceInfo> collection, Type entityType, int? onlyLastCount)
        {
            var typedCollection = collection
                             .Where(entityInfo => entityInfo.EntityType == entityType)
                             .Select(entityInfo => entityInfo.Instance);
            if (onlyLastCount.HasValue)
            {
                typedCollection = typedCollection.TakeLast(onlyLastCount.Value);
            }

            return typedCollection;
        }
    }
}
