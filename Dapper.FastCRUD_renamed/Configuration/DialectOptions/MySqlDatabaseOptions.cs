namespace Dapper.FastCrud.Configuration.DialectOptions
{
    internal class MySqlDatabaseOptions : SqlDatabaseOptions
    {
        public MySqlDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = "`";
        }
    }
}
