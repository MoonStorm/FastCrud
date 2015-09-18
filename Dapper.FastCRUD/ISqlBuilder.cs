namespace Dapper.FastCrud
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// The SQL builder is useful for creating verbatim SQL queries.
    /// </summary>
    public interface ISqlBuilder
    {
        /// <summary>
        /// Returns the table name associated with the current entity.
        /// </summary>
        /// <param name="tableAlias">Optional table alias using AS.</param>
        string GetTableName(string tableAlias = null);

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// Please use <code>nameof(entity.propname)</code> if available, alternatively use the other overload for this method.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="tableAlias">Optional table alias.</param>
        string GetColumnName(string propertyName, string tableAlias = null);

        /// <summary>
        /// Returns the name of the database column attached to the specified property.
        /// Please use <code>nameof(entity.propname)</code> if available, alternatively use the other overload for this method.
        /// </summary>
        /// <param name="property">Property for which you want to get the column name.</param>
        /// <param name="tableAlias">Optional table alias.</param>
        string GetColumnName<TEntity,TProperty>(Expression<Func<TEntity, TProperty>> property, string tableAlias = null);

        /// <summary>
        /// Constructs a condition of form <code>ColumnName=@PropertyName and ...</code> with all the key columns (e.g. <code>Id=@Id and EmployeeId=@EmployeeId</code>)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        string ConstructKeysWhereClause(string tableAlias = null);

        /// <summary>
        /// Constructs an enumeration of all the selectable columns (i.e. all the columns corresponding to entity properties which are not part of a relationship).
        /// (e.g. Id, HouseNo, AptNo)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        string ConstructColumnEnumerationForSelect(string tableAlias = null);

        /// <summary>
        /// Constructs an enumeration of all the columns available for insert.
        /// (e.g. HouseNo, AptNo)
        /// </summary>
        string ConstructColumnEnumerationForInsert();

        /// <summary>
        /// Constructs an enumeration of all the parameters denoting properties that are bound to columns available for insert.
        /// (e.g. @HouseNo, @AptNo)
        /// </summary>
        string ConstructParamEnumerationForInsert();

        /// <summary>
        /// Constructs a update clause of form <code>ColumnName=@PropertyName, ...</code> with all the updateable columns (e.g. <code>EmployeeId=@EmployeeId,DeskNo=@DeskNo</code>)
        /// </summary>
        /// <param name="tableAlias">Optional table alias.</param>
        string ConstructUpdateClause(string tableAlias = null);
    }
}
