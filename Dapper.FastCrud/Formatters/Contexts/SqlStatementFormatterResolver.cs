namespace Dapper.FastCrud.Formatters.Contexts
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.SqlBuilders;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Threading;

    /// <summary>
    /// Holds information about an entity participating in a statement.
    /// A resolver mainly identifies itself with the alias, but it can also identify itself by type name (and table name in case the table and the type names are different)
    /// </summary>
    internal sealed class SqlStatementFormatterResolver
    {
        private readonly Lazy<GenericStatementSqlBuilder> _sqlBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlStatementFormatterResolver(
            EntityDescriptor entityDescriptor,
            EntityRegistration entityRegistration,
            string? alias)
        {
            Requires.NotNull(entityDescriptor, nameof(entityRegistration));
            Requires.NotNull(entityRegistration, nameof(entityRegistration));

            this.EntityDescriptor = entityDescriptor;
            this.EntityRegistration = entityRegistration;
            this.Alias = alias;
            _sqlBuilder = new Lazy<GenericStatementSqlBuilder>(()=> entityDescriptor.GetSqlBuilder(entityRegistration), LazyThreadSafetyMode.None);
        }

        /// <summary>
        /// The entity descriptor.
        /// </summary>
        public EntityDescriptor EntityDescriptor { get; }

        /// <summary>
        /// The entity registration.
        /// </summary>
        public EntityRegistration EntityRegistration { get; }

        /// <summary>
        /// The associated SQL builder.
        /// </summary>
        public GenericStatementSqlBuilder SqlBuilder => _sqlBuilder.Value;

        /// <summary>
        /// The alias as it was assigned for the statement formatter.
        /// </summary>
        public string? Alias { get; }
    }
}
