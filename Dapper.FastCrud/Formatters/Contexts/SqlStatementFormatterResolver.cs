namespace Dapper.FastCrud.Formatters.Contexts
{
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Threading;

    /// <summary>
    /// Holds information about an entity participating in a statement.
    /// </summary>
    internal sealed class SqlStatementFormatterResolver
    {
        private readonly Lazy<ISqlBuilder> _sqlBuilder;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlStatementFormatterResolver(
            EntityDescriptor entityDescriptor,
            EntityRegistration entityRegistration,
            string? alias)
        {
            Requires.NotNull(entityRegistration, nameof(entityRegistration));

            this.EntityRegistration = entityRegistration;
            this.Alias = alias;
            _sqlBuilder = new Lazy<ISqlBuilder>(()=> entityDescriptor.GetSqlBuilder(entityRegistration), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// The entity registration.
        /// </summary>
        public EntityRegistration EntityRegistration { get; }

        /// <summary>
        /// The associated SQL builder.
        /// </summary>
        public ISqlBuilder SqlBuilder => _sqlBuilder.Value;

        /// <summary>
        /// The alias as it was assigned for the statement formatter.
        /// </summary>
        public string? Alias { get; }
    }
}
