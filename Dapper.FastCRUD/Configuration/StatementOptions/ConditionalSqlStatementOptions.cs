namespace Dapper.FastCrud.Configuration.StatementOptions
{
    using System;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Sql options for a statement restricted by a where clause.
    /// </summary>
    internal interface IConditionalSqlStatementOptionsGetter:IStandardSqlStatementOptionsGetter
    {
        /// <summary>
        /// Parameters used by the statement.
        /// </summary>
        object Parameters { get; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        FormattableString WhereClause { get; }
    }

    /// <summary>
    /// Conditional sql statement options setter. 
    /// </summary>
    public interface IConditionalSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        : IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        where TStatementOptionsSetter : IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        TStatementOptionsSetter Where(FormattableString whereClause);

        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        TStatementOptionsSetter WithParameters(object parameters);
    }

    /// <summary>
    /// Sql options for a statement restricted by a where clause.
    /// </summary>
    internal class ConditionalSqlStatementOptions<TEntity, TStatementOptionsSetter> 
        : StandardSqlStatementOptions<TEntity,TStatementOptionsSetter>, 
        IConditionalSqlStatementOptionsGetter,
        IConditionalSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
        where TStatementOptionsSetter : class, IStandardSqlStatementOptionsSetter<TEntity, TStatementOptionsSetter>
    {
        /// <summary>
        /// Parameters used by the statement.
        /// </summary>
        public object Parameters { get; private set; }

        /// <summary>
        /// Gets or sets a where clause.
        /// </summary>
        public FormattableString WhereClause { get; private set; }

        /// <summary>
        /// Limits the result set with a where clause.
        /// </summary>
        public TStatementOptionsSetter Where(FormattableString whereClause)
        {
            Requires.NotNull(whereClause, nameof(whereClause));

            this.WhereClause = whereClause;
            return this as TStatementOptionsSetter;
        }

        /// <summary>
        /// Sets the parameters to be used by the statement.
        /// </summary>
        public TStatementOptionsSetter WithParameters(object parameters)
        {
            Requires.NotNull(parameters, nameof(parameters));

            this.Parameters = parameters;
            return this as TStatementOptionsSetter;
        }
    }

    /// <summary>
    /// Conditional sql statement options builder. 
    /// </summary>
    public interface IConditionalSqlStatementOptionsBuilder<TEntity> :
        IConditionalSqlStatementOptionsSetter<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>>
    {

    }

    /// <summary>
    /// Conditional sql statement options builder. 
    /// </summary>
    internal class ConditionalSqlStatementOptionsBuilder<TEntity> :
        ConditionalSqlStatementOptions<TEntity, IConditionalSqlStatementOptionsBuilder<TEntity>>,
        IConditionalSqlStatementOptionsBuilder<TEntity>
    {        
    }
}
