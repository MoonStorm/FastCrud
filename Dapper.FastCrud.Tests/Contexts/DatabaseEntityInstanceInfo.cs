namespace Dapper.FastCrud.Tests.Contexts
{
    using System;

    public class DatabaseEntityInstanceInfo
    {
        public DatabaseEntityInstanceInfo(Type entityType, object instance)
        {
            this.EntityType = entityType;
            this.Instance = instance;
        }

        public Type EntityType { get; }
        public object Instance { get; }
    }
}
