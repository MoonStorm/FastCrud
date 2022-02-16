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
        private EfInternalDbContext? _dbContext;

        public EfDbContext(DatabaseTestContext testContext)
        {
            _testContext = testContext;
        }

        public EfInternalDbContext Value
        {
            get
            {
                if (_dbContext == null)
                {
                    _dbContext = new EfInternalDbContext(_testContext.DatabaseConnection);
                }

                return _dbContext;
            }
        }


        public bool IsValueCreated => _dbContext!=null;

        public void ResetContext()
        {
            _dbContext?.Dispose();
            _dbContext = null;
        }

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
