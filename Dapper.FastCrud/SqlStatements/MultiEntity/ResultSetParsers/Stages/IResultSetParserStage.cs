namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Stages
{
    using System;

    internal interface IResultSetParserStage
    {
        /// <summary>
        /// The entity type the stage is attached to.
        /// </summary>
        Type FromEntityType { get; }

        /// <summary>
        /// The entity type the stage is attached to.
        /// </summary>
        Type ToEntityType { get; }

        /// <summary>
        /// Adds a new stage as a continuation of the current one.
        /// </summary>
        void RegisterContinuation(IResultSetParserStage followingStage);

        /// <summary>
        /// Executes the stage and the linked stages, provided an input entity instance and the data row with already checked unique entities.
        /// </summary>
        void Execute(EntityInstanceWrapper currentEntityInstance, EntityInstanceWrapper[] dataRow);
    }
}