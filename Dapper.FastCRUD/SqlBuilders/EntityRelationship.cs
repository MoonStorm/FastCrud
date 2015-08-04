namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Dapper.FastCrud.Mappings;

    /// <summary>
    /// Investigates potential relationships between two entities.
    /// </summary>
    internal class EntityRelationship
    {
        public enum RelationshipType
        {
            None,
            ParentChild,
            ChildParent
        }

        public EntityRelationship(IStatementSqlBuilder sourceSqlBuilder, IStatementSqlBuilder destinationSqlBuilder)
        {
            this.Relationship = RelationshipType.None;

            var sourceRelationshipMapping = this.GetRelationshipMapping(
                sourceSqlBuilder, 
                destinationSqlBuilder, 
                parentChildRelationship:false);

            var destinationRelationshipMapping = this.GetRelationshipMapping(
                destinationSqlBuilder,
                sourceSqlBuilder,
                parentChildRelationship: true);

            if (sourceRelationshipMapping != null && destinationRelationshipMapping != null)
            {
                // found a child-parent relationship
                this.Relationship = RelationshipType.ChildParent;
            }
            else
            {
                sourceRelationshipMapping = this.GetRelationshipMapping(
                    sourceSqlBuilder,
                    destinationSqlBuilder,
                    parentChildRelationship: true);

                destinationRelationshipMapping = this.GetRelationshipMapping(
                    destinationSqlBuilder,
                    sourceSqlBuilder,
                    parentChildRelationship: false);

                if (sourceRelationshipMapping != null && destinationRelationshipMapping != null)
                {
                    // found a parent child relationship
                    this.Relationship = RelationshipType.ParentChild;
                }
            }

            if (this.Relationship != RelationshipType.None)
            {
                this.OwnPropertyDescriptor = sourceRelationshipMapping.Descriptor;
                this.ForeignPropertyDescriptor = destinationRelationshipMapping.Descriptor;
                this.RelationshipLink = GetRelationshipLink(
                    sourceSqlBuilder,
                    sourceRelationshipMapping,
                    destinationSqlBuilder,
                    destinationRelationshipMapping);
            }
        }

        public Tuple<PropertyMapping, PropertyMapping>[] RelationshipLink { get; private set; }
        public RelationshipType Relationship { get; private set; }
        public PropertyDescriptor OwnPropertyDescriptor { get; private set; }
        public PropertyDescriptor ForeignPropertyDescriptor { get; private set; }

        private PropertyMapping GetRelationshipMapping(
            IStatementSqlBuilder sourceSqlBuilder,
            IStatementSqlBuilder destinationSqlBuilder,
            bool parentChildRelationship)
        {
            var relationshipSourcePropertyMappings = sourceSqlBuilder.ForeignEntityProperties.Where(propMapping => parentChildRelationship
                ?this.IsChildrenPropertyMapping(propMapping, destinationSqlBuilder.EntityMapping.EntityType)
                :this.IsParentPropertyMapping(propMapping, destinationSqlBuilder.EntityMapping.EntityType)).ToArray();
            if (relationshipSourcePropertyMappings.Length > 1)
            {
                throw new InvalidOperationException($"Entity '{sourceSqlBuilder.EntityMapping.EntityType.Name}' has more than one relationship of type {(parentChildRelationship?"parent-child":"child-parent")} with the entity '{destinationSqlBuilder.EntityMapping.EntityType.Name}'");
            }
            var relationshipSourcePropertyMapping = relationshipSourcePropertyMappings.SingleOrDefault();
            return relationshipSourcePropertyMapping;
        }

        private bool IsParentPropertyMapping(PropertyMapping propMapping, Type entityType)
        {
            return propMapping.Descriptor.PropertyType == entityType;
        }

        private bool IsChildrenPropertyMapping(PropertyMapping propMapping, Type entityType)
        {
            var propertyType = propMapping.Descriptor.PropertyType;

            // we're looking for IEnumerable<Entity>
            return propertyType.IsGenericType && !propertyType.IsGenericTypeDefinition
                   && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) && propertyType.GetGenericArguments().Length == 1
                   && propertyType.GetGenericArguments()[0] == entityType;
        }

        private Tuple<PropertyMapping, PropertyMapping>[] GetRelationshipLink(
            IStatementSqlBuilder sourceSqlBuilder,
            PropertyMapping sourcePropMapping,
            IStatementSqlBuilder destinationSqlBuilder,
            PropertyMapping destinationPropMapping)
        {
            var sourcePropNames = sourcePropMapping.RelationshipPropertyNames;
            var destinationPropNames = destinationPropMapping.RelationshipPropertyNames;
            if (sourcePropNames == null)
            {
                throw new InvalidOperationException($"No property was found for the relationship denoted by '{sourcePropMapping.PropertyName}' on '{sourceSqlBuilder.EntityMapping.EntityType.Name}'");
            }
            if (destinationPropNames == null)
            {
                throw new InvalidOperationException($"No property was found for the relationship denoted by '{destinationPropMapping.PropertyName}' on '{destinationSqlBuilder.EntityMapping.EntityType.Name}'");
            }
            if (sourcePropNames.Length == 0 && destinationPropNames.Length == 0)
            {
                throw new InvalidOperationException($"No properties were found for the relationship between '{sourceSqlBuilder.EntityMapping.EntityType.Name}[{sourcePropMapping.PropertyName}]' and '{destinationSqlBuilder.EntityMapping.EntityType.Name}[{destinationPropMapping.PropertyName}]'");
            }
            if (sourcePropNames.Length == 0)
            {
                if (sourceSqlBuilder.KeyProperties.Length != 1)
                {
                    throw new InvalidOperationException($"Unable to infer the property used in the relationship between '{sourceSqlBuilder.EntityMapping.EntityType.Name}[{sourcePropMapping.PropertyName}]' and '{destinationSqlBuilder.EntityMapping.EntityType.Name}[{destinationPropMapping.PropertyName}]'");
                }
                sourcePropNames = new[] { sourceSqlBuilder.KeyProperties[0].PropertyName };
            }
            if (destinationPropNames.Length == 0)
            {
                if (destinationSqlBuilder.KeyProperties.Length != 1)
                {
                    throw new InvalidOperationException($"Unable to infer the property used in the relationship between '{destinationSqlBuilder.EntityMapping.EntityType.Name}[{destinationPropMapping.PropertyName}]' and '{sourceSqlBuilder.EntityMapping.EntityType.Name}[{sourcePropMapping.PropertyName}]' ");
                }
                destinationPropNames = new[] { sourceSqlBuilder.KeyProperties[0].PropertyName };
            }

            if (sourcePropNames.Length != destinationPropNames.Length)
            {
                throw new InvalidOperationException("Invalid number of properties used in the relationship between '{sourceSqlBuilder.EntityMapping.EntityType.Name}[{sourcePropMapping.PropertyName}]' and '{destinationSqlBuilder.EntityMapping.EntityType.Name}[{destinationPropMapping.PropertyName}]'");
            }

            return Enumerable.Range(0, sourcePropNames.Length).Select(
                index =>
                    {
                        PropertyMapping sourcePropertyMapping;
                        if (!sourceSqlBuilder.EntityMapping.PropertyMappings.TryGetValue(sourcePropNames[index], out sourcePropertyMapping))
                        {
                            throw new InvalidOperationException($"Unable to locate property '{sourcePropNames[index]}' on the entity '{sourceSqlBuilder.EntityMapping.EntityType.Name}' for the relationship with '{destinationSqlBuilder.EntityMapping.EntityType.Name}'");
                        }
                        PropertyMapping destinationPropertyMapping;
                        if (!destinationSqlBuilder.EntityMapping.PropertyMappings.TryGetValue(destinationPropNames[index], out destinationPropertyMapping))
                        {
                            throw new InvalidOperationException($"Unable to locate property '{destinationPropNames[index]}' on the entity '{destinationSqlBuilder.EntityMapping.EntityType.Name}' for the relationship with '{sourceSqlBuilder.EntityMapping.EntityType.Name}'");
                        }

                        return new Tuple<PropertyMapping, PropertyMapping>(sourcePropertyMapping, destinationPropertyMapping);
                    }).ToArray();
        }

    }
}
