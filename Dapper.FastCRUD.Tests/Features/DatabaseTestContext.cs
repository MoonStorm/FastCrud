namespace Dapper.FastCrud.Tests.Features
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;

    public class DatabaseTestContext
    {
        private const int MaxEntityTestingCapacity = 100000;

        public DatabaseTestContext()
        {
            Stopwatch = new Stopwatch();

            // ensure the capacity can hold all the processed entities

            InsertedEntities = new List<object>(MaxEntityTestingCapacity); 
            QueriedEntities = new List<object>(MaxEntityTestingCapacity);
            UpdatedEntities = new List<object>(MaxEntityTestingCapacity);
        }

        public IDbConnection DatabaseConnection { get; set; }
        public Stopwatch Stopwatch { get; private set; }
        public List<object> InsertedEntities { get; private set; }
        public List<object> QueriedEntities { get; set; }
        public List<object> UpdatedEntities { get; set; }
    }
}
