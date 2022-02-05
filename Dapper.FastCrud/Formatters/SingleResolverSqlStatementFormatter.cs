namespace Dapper.FastCrud.Formatters
{
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Offers SQL formatting capabilities in a space where only one resolver can be active.
    /// </summary>
    internal class SingleResolverSqlStatementFormatter: GenericSqlStatementFormatter
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SingleResolverSqlStatementFormatter(
            EntityRegistration entityRegistration, 
            ISqlBuilder sqlBuilder, 
            string? alias = null)
        {
            Requires.NotNull(entityRegistration, nameof(entityRegistration));
            Requires.NotNull(sqlBuilder, nameof(sqlBuilder));

            this.ActiveResolver = new SqlStatementFormatterResolver(entityRegistration, sqlBuilder, alias);
        }

        /// <summary>
        /// Gets the currently active resolver, if one is present.
        /// </summary>
        protected override SqlStatementFormatterResolver? ActiveResolver { get; }
    }
}
