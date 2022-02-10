namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Stages
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a parser stage
    /// </summary>
    internal abstract class ResultSetParserStage : IResultSetParserStage
    {
        private readonly List<IResultSetParserStage> _followingStages = new List<IResultSetParserStage>();

        /// <summary>
        /// Default constructor
        /// </summary>
        protected ResultSetParserStage(EntityContainer entityContainer, SqlStatementJoin joinDefinition, int dataSetRowColumnIndex)
        {
            this.EntityContainer = entityContainer;
            this.JoinDefinition = joinDefinition;
            this.DataSetRowColumnIndex = dataSetRowColumnIndex;
        }

        /// <summary>
        /// The entity type the stage is attached to.
        /// </summary>
        public Type FromEntityType => this.EntityContainer.EntityType;

        /// <summary>
        /// The entity type the stage is attached to.
        /// </summary>
        public Type ToEntityType => this.JoinDefinition.ReferencedEntityRegistration.EntityType;

        /// <summary>
        /// Returns the entity container related to the entity type circulating through the stage.
        /// </summary>
        public EntityContainer EntityContainer { get; }

        /// <summary>
        /// The index in the row dataset where the entity can be found.
        /// </summary>
        public int DataSetRowColumnIndex { get; }

        /// <summary>
        /// The join definition.
        /// </summary>
        public SqlStatementJoin JoinDefinition { get; }

        /// <summary>
        /// Adds a new stage as a continuation of the current one.
        /// </summary>
        public void RegisterContinuation(IResultSetParserStage followingStage)
        {
            _followingStages.Add(followingStage);
        }

        /// <summary>
        /// Executes the stage and the linked stages, provided an input entity instance and the data row with already checked unique entities.
        /// </summary>
        public void Execute(EntityInstanceWrapper currentEntityInstance, EntityInstanceWrapper[] dataRow)
        {
            var nextEntityInstance = this.Execute(currentEntityInstance, dataRow[this.DataSetRowColumnIndex]);

            foreach (var followingStage in _followingStages)
            {
                followingStage.Execute(nextEntityInstance, dataRow);
            }
        }

        /// <summary>
        /// Executes the stage provided an input entity instance, the next following entity, and returns the actual next entity used.
        /// </summary>
        protected abstract EntityInstanceWrapper Execute(
            EntityInstanceWrapper referencingEntityInstance,
            EntityInstanceWrapper referencedEntityInstance);
    }
}
