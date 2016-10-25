namespace Dapper.FastCrud.Tests
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;

    public class DatabaseTestContext
    {
        private const int MaxEntityTestingCapacity = 100000;

        public DatabaseTestContext()
        {
            this.Stopwatch = new Stopwatch();

            // ensure the capacity can hold all the processed entities
            this.QueriedEntities = new List<object>(MaxEntityTestingCapacity);
            this.LocalInsertedEntities = new List<object>(MaxEntityTestingCapacity);
        }

        public DbConnection DatabaseConnection { get; set; }
        public Stopwatch Stopwatch { get; private set; }
        public List<object> QueriedEntities { get; set; }
        public List<object> LocalInsertedEntities { get; set; }
        public int QueriedEntitiesDbCount { get; set; }
    }
}
