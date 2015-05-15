namespace Dapper.FastCrud.Tests.Features
{
    using System.Data;
    using System.Diagnostics;

    public class DatabaseTestContext
    {
        public DatabaseTestContext()
        {
            Stopwatch = new Stopwatch();    
        }

        public IDbConnection DatabaseConnection { get; set; }
        public Stopwatch Stopwatch { get; private set; }
    }
}
