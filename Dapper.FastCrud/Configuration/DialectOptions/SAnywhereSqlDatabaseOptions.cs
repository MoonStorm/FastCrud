namespace Dapper.FastCrud.Configuration.DialectOptions
{
    internal class SAnywhereSqlDatabaseOptions : SqlDatabaseOptions
    {
        public SAnywhereSqlDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = "\"";
            this.ParameterPrefix = this.ParameterSuffix = "?";
            this.IsUsingSchemas = true;
        }
    }
}