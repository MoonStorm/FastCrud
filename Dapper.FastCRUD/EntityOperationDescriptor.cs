namespace Dapper.FastCrud
{
    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal abstract class EntityOperationDescriptor<TEntityDescriptor, TEntity> where TEntityDescriptor:EntityDescriptor<TEntity>
    {
        protected EntityOperationDescriptor(TEntityDescriptor entityDescriptor)
        {
            this.EntityDescriptor = entityDescriptor;
        }

        public TEntityDescriptor EntityDescriptor { get; private set; }
    }
}
