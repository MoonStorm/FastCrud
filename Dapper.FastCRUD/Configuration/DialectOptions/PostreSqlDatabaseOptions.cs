namespace Dapper.FastCrud.Configuration.DialectOptions
{
    internal class PostreSqlDatabaseOptions : SqlDatabaseOptions
    {
        public PostreSqlDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = "\"";
            this.IsUsingSchemas = true;
        }
    }
}
