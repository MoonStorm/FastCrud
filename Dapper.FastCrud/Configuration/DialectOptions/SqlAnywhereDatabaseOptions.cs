namespace Dapper.FastCrud.Configuration.DialectOptions
{
    internal class SqlAnywhereDatabaseOptions : SqlDatabaseOptions
    {
        public SqlAnywhereDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = "\"";
            this.ParameterPrefix = this.ParameterSuffix = "?";
            this.IsUsingSchemas = true;
        }
    }
}