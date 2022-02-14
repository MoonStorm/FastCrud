namespace Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers
{
    using Dapper.FastCrud.Configuration;
    using Dapper.FastCrud.SqlStatements.MultiEntity.ResultSetParsers.Containers;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// The main entry into the parser stages.
    /// </summary>
    internal class MainEntityResultSetParser<TMainEntity>: BasicResultSetParser
    {
        private readonly Type _mainEntityType = typeof(TMainEntity);
        private static readonly PropertyDescriptor _mainEntityListPropDesc = TypeDescriptor.GetProperties(typeof(MainEntityResultSetParser<TMainEntity>))[nameof(MainEntityResultSetParser<TMainEntity>.MainEntityCollection)];
        private Lazy<TypedEntityContainer> _typedMainEntityContainer;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainEntityResultSetParser(SqlStatementJoin[] joins)
            :base(new EntityContainer())
        {
            _typedMainEntityContainer = new Lazy<TypedEntityContainer>(() => this.SharedContainer[_mainEntityType], LazyThreadSafetyMode.None);
            this.ConstructParserStages(joins);
        }

        /// <summary>
        /// Returns the parsed collection of main entities.
        /// </summary>
        public List<TMainEntity> MainEntityCollection { get; } = new List<TMainEntity>();

        /// <summary>
        /// Executes the current stage and produces the next instance.
        /// </summary>
        protected override EntityInstanceWrapper ProduceNextInstance(EntityInstanceWrapper? _, EntityInstanceWrapper[] originalEntityRow)
        {
            // get unique entities in the row (the uniqueness is per entire statement, not just a particular row or join)
            for (var dataRowIndex = 0; dataRowIndex < originalEntityRow.Length; dataRowIndex++)
            {
                var entityInstance = originalEntityRow[dataRowIndex];
                var uniqueEntityInstance = this.SharedContainer[entityInstance.EntityRegistration.EntityType].GetOrRegisterGlobally(entityInstance);
                originalEntityRow[dataRowIndex] = uniqueEntityInstance;
            }

            // add our entity uniquely to our own collection of main entities to return
            var mainEntity = _typedMainEntityContainer.Value.GetOrAddToLocalCollection(this, _mainEntityListPropDesc, this.MainEntityCollection, originalEntityRow[0]);

            return mainEntity;
        }

        private void ConstructParserStages(SqlStatementJoin[] joins)
        {
            // maintain a map of what is responsible for handling a particular referencing entity
            var joinStageMap = new Dictionary<Type, BasicResultSetParser>();
            joinStageMap.Add(_mainEntityType, this);

            // now let's go through the joins
            for (var joinIndex = 0; joinIndex<joins.Length; joinIndex++)
            {
                var join = joins[joinIndex];

                // and now through the relationships
                foreach (var joinRelationship in join.ResolvedRelationships.Where( rel => rel.MapResults))
                {
                    if (!joinStageMap.TryGetValue(joinRelationship.ReferencingEntityDescriptor.EntityType, out BasicResultSetParser existingResultSetParser))
                    {
                        throw new InvalidOperationException($"The result set parser could not locate an existing parser for entity '{joinRelationship.ReferencingEntityDescriptor.EntityType}'. Adjusting the order of the Includes may solve this problem.");
                    }

                    var joinRelationshipResolver = new EntityPropertyResultSetParser(this.SharedContainer, joinRelationship, joinIndex + 1);
                    existingResultSetParser.RegisterContinuation(joinRelationshipResolver);

                    // now add it as the new resolver for the referenced type
                    if (!joinStageMap.ContainsKey(joinRelationship.ReferencedEntityRegistration.EntityType))
                    {
                        joinStageMap[joinRelationship.ReferencedEntityRegistration.EntityType] = joinRelationshipResolver;
                    }
                }

                //// one final thing we need is to add the object extractor for the join index
                //var newEntityExtractor = new DataRowEntityResultParser(this.SharedContainer, joinIndex + 1); // the first one is the main entity so skip that
                //if (joinStageMap.TryGetValue(join.ReferencedEntityRegistration.EntityType, out BasicResultSetParser existingEntityExtractor))
                //{
                //    existingEntityExtractor.RegisterContinuation(newEntityExtractor);
                //}
                //else
                //{
                //    this.RegisterContinuation(newEntityExtractor);
                //}
                //joinStageMap[join.ReferencedEntityRegistration.EntityType] = newEntityExtractor;
            }
        }
    }
}
