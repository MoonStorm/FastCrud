namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Stages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The main entry into the parser stages.
    /// </summary>
    internal class MainEntityResultSetParserStage<TMainEntity>: IResultSetParserStage
    {
        private readonly Dictionary<Type, EntityContainer> _entityContainers = new Dictionary<Type, EntityContainer>();
        private readonly List<TMainEntity> _mainEntityInstanceCollection = new List<TMainEntity>();
        private readonly Type _mainEntityType = typeof(TMainEntity);
        private readonly List<IResultSetParserStage> _registeredStages = new List<IResultSetParserStage>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainEntityResultSetParserStage(SqlStatementJoin[] joins)
        {
            _entityContainers[_mainEntityType] = new EntityContainer(_mainEntityType, _mainEntityInstanceCollection);
            this.ConstructParserStages(joins);
        }

        /// <summary>
        /// The entity type the stage is attached to.
        /// </summary>
        public Type? FromEntityType { get; } = null;

        /// <summary>
        /// The entity type the stage is attached to.
        /// </summary>
        public Type ToEntityType => _mainEntityType;

        /// <summary>
        /// Returns the parsed collection of main entities.
        /// </summary>
        public ICollection<TMainEntity> EntityCollection => _mainEntityInstanceCollection;

        /// <summary>
        /// Adds a new stage as a continuation of the current one.
        /// </summary>
        public void RegisterContinuation(IResultSetParserStage followingStage)
        {
            _registeredStages.Add(followingStage);
        }

        /// <summary>
        /// Executes the stage and the linked stages, provided an input entity instance and the data row with already checked unique entities.
        /// </summary>
        public void Execute(EntityInstanceWrapper? _, EntityInstanceWrapper[] dataRow)
        {
            // get unique entities in the row
            var uniqueDataRow = new EntityInstanceWrapper[dataRow.Length];
            for (var dataRowIndex = 0; dataRowIndex < dataRow.Length; dataRowIndex++)
            {
                var entityInstance = dataRow[dataRowIndex];
                var uniqueEntityInstance = this.EnsureEntityContainerCreated(entityInstance.EntityRegistration.EntityType)
                                     .GetOrAdd(entityInstance);
                uniqueDataRow[dataRowIndex] = uniqueEntityInstance;
            }

            var mainEntity = uniqueDataRow[0];
            foreach (var followingStage in _registeredStages)
            {
                followingStage.Execute(mainEntity, uniqueDataRow);
            }
        }

        private EntityContainer EnsureEntityContainerCreated(Type entityType)
        {
            if(!_entityContainers.TryGetValue(entityType, out EntityContainer entityContainer))
            {
                entityContainer = new EntityContainer(entityType);
                _entityContainers.Add(entityType, entityContainer);
            }

            return entityContainer;
        }

        private void ConstructParserStages(SqlStatementJoin[] joins)
        {
            var joinStageMap = new Dictionary<SqlStatementJoin, IResultSetParserStage>();
            var stagesToAnalyze = new Queue<IResultSetParserStage>();
            stagesToAnalyze.Enqueue(this);

            while (stagesToAnalyze.Count > 0)
            {
                var stageToAnalyze = stagesToAnalyze.Dequeue();

                // now locate the joins that we haven't looked at already that have the entity type of the stage as the referencing entity
                foreach(var matchedJoinInfo in joins.Select((join, joinIndex) =>
                                                        {
                                                            return new
                                                            {
                                                                Join = join,
                                                                JoinIndex = joinIndex,
                                                                ReferencingType = join.ReferencingEntitySqlBuilder.EntityDescriptor.EntityType
                                                            };
                                                        })
                                                        .Where(joinInfo => joinInfo.ReferencingType == stageToAnalyze.ToEntityType)
                                                        .Where(joinInfo => !joinStageMap.ContainsKey(joinInfo.Join))
                                                        .ToArray())
                {
                    var entityContainer = this.EnsureEntityContainerCreated(stageToAnalyze.ToEntityType);
                    var newStage = new PropertyResultSetParserStage(entityContainer, matchedJoinInfo.Join, matchedJoinInfo.JoinIndex + 1);
                    joinStageMap[matchedJoinInfo.Join] = newStage;
                    stageToAnalyze.RegisterContinuation(newStage);
                    stagesToAnalyze.Enqueue(newStage);
                }
            }
        }
    }
}
