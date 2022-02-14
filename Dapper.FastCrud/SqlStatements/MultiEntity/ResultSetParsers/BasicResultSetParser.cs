namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers
{
    using Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Containers;
    using System.Collections.Generic;

    /// <summary>
    /// A result set parser stage is responsible for handling setting up values on an entity
    /// </summary>
    internal abstract class BasicResultSetParser
    {
        private readonly List<BasicResultSetParser> _registeredParsers = new List<BasicResultSetParser>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BasicResultSetParser(EntityContainer sharedContainer)
        {
            this.SharedContainer = sharedContainer;
        }

        /// <summary>
        /// The global shared entity container.
        /// </summary>
        public EntityContainer SharedContainer { get; }

        /// <summary>
        /// Adds a new stage as a continuation of the current one.
        /// </summary>
        public void RegisterContinuation(BasicResultSetParser followingStage)
        {
            _registeredParsers.Add(followingStage);
        }

        /// <summary>
        /// Executes the stage using an entity instance as input and the original entity row as a reference.
        /// </summary>
        public void Execute(EntityInstanceWrapper? previousEntity, EntityInstanceWrapper[] originalEntityRow)
        {
            var nextInstance = this.ProduceNextInstance(previousEntity, originalEntityRow);
            foreach (var registeredParser in _registeredParsers)
            {
                registeredParser.Execute(nextInstance, originalEntityRow);
            }
        }

        /// <summary>
        /// Executes the current stage and produces the next instance.
        /// </summary>
        protected abstract EntityInstanceWrapper ProduceNextInstance(EntityInstanceWrapper? previousEntity, EntityInstanceWrapper[] originalEntityRow);
    }
}