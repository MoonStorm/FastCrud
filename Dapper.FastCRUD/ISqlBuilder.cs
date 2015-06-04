namespace Dapper.FastCrud
{
    using Dapper.FastCrud.Mappings;

    public interface ISqlBuilder
    {
        string GetTableName(string alias = null);

        string ConstructKeysWhereClause(string alias = null);

        string ConstructColumnEnumerationForSelect(string alias = null);

        string ConstructColumnEnumerationForInsert();

        string ConstructParamEnumerationForInsert();

        string ConstructUpdateClause(string alias = null);
    }
}
