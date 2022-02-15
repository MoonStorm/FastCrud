namespace Dapper.FastCrud.Benchmarks.Targets.EntityFramework
{
    using global::Dapper.FastCrud.Benchmarks.Models;
    using global::Dapper.FastCrud.Tests.Contexts;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Data.Common;
    using System.Threading;

    public class EfDbContext:DbContext
    {
        private readonly DatabaseTestContext _testContext;
        private Lazy<EfInternalDbContext> _dbContext;

        public EfDbContext(DatabaseTestContext testContext)
        {
            _testContext = testContext;
            _dbContext = new Lazy<EfInternalDbContext>(() => new EfInternalDbContext(_testContext.DatabaseConnection), LazyThreadSafetyMode.None);

        }

        public EfInternalDbContext Value => _dbContext.Value;

        public bool IsValueCreated => _dbContext.IsValueCreated;

        public class EfInternalDbContext : DbContext
        {
            private DbConnection _dbConnection;

            public EfInternalDbContext(DbConnection dbConnection)
            {
                _dbConnection = dbConnection;
            }

            public DbSet<SimpleBenchmarkEntity> BenchmarkEntities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(_dbConnection);
                base.OnConfiguring(optionsBuilder);
            }
        }


    }
}
