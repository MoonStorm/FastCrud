namespace Dapper.FastCrud.Benchmarks
{
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using Dapper.FastCrud.Tests.Models;

    public class EfDbContext:DbContext
    {
        /// <summary>
        /// Constructs a new context instance using the existing connection to connect to a database,
        ///             and initializes it from the given model.
        ///             The connection will not be disposed when the context is disposed if <paramref name="contextOwnsConnection"/>
        ///             is <c>false</c>.
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context. </param>
        /// <param name="model">The model that will back this context. </param>
        /// <param name="contextOwnsConnection">If set to <c>true</c> the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.
        ///             </param>
        public EfDbContext(DbConnection existingConnection, DbCompiledModel model, bool enableProxyCreation)
            : base(existingConnection, model, false)
        {
            this.Configuration.ProxyCreationEnabled = enableProxyCreation;
        }

        public DbSet<SimpleBenchmarkEntity> BenchmarkEntities { get; set; }
    }
}
