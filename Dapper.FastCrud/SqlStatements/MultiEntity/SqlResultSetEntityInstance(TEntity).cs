namespace Dapper.FastCrud.SqlStatements.MultiEntity
{
    using Dapper.FastCrud.Mappings.Registrations;

    /// <summary>
    /// Represents an unknown entity instance, as retrieved by executing the statement.
    /// </summary>
    internal class SqlResultSetEntityInstance<TEntity> : EntityInstanceWrapper
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlResultSetEntityInstance(EntityRegistration entityRegistration, TEntity entityInstance)
        :base(entityRegistration, entityInstance)
        {
        }

        /// <summary>
        /// Gets the underlying instance.
        /// </summary>
        public new TEntity EntityInstance => (TEntity)base.EntityInstance;

    }
}
