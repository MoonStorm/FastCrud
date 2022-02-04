namespace Dapper.FastCrud.Formatters
{
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Holds information about an entity participating in a statement.
    /// </summary>
    internal class SqlStatementFormatterEntityAttendant
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlStatementFormatterEntityAttendant(
            EntityRegistration entityRegistration,
            ISqlBuilder sqlBuilder,
            string? alias = null)
        {
            Requires.NotNull(entityRegistration, nameof(entityRegistration));
            Requires.NotNull(sqlBuilder, nameof(sqlBuilder));

            this.EntityRegistration = entityRegistration;
            this.SqlBuilder = sqlBuilder;
            this.Alias = alias;
        }

        /// <summary>
        /// The entity registration.
        /// </summary>
        public EntityRegistration EntityRegistration { get; }

        /// <summary>
        /// The associated SQL builder.
        /// </summary>
        public ISqlBuilder SqlBuilder { get; }

        /// <summary>
        /// The alias as it was assigned for the statement formatter.
        /// </summary>
        public string? Alias { get; }
    }
}
