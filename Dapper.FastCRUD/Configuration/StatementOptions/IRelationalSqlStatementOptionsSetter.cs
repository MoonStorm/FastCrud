namespace Dapper.FastCrud.Configuration.StatementOptions
{
    /// <summary>
    /// Statement options for entity relationships
    /// </summary>
    public interface IRelationalStatementOptionsSetter<TLeftEntity, TStatementOptionsSetter>
    {
        TStatementOptionsSetter Include<TRightEntity>()
    }
}
