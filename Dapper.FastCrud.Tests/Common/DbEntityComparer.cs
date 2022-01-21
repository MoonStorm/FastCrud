namespace Dapper.FastCrud.Tests.Common
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Linq;
    using NUnit.Framework;

    public class DbEntityComparer:IComparer
    {
        public static DbEntityComparer Instance = new DbEntityComparer();

        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        public int Compare(object firstEntity, object secondEntity)
        {
            if (firstEntity == null || secondEntity == null)
            {
                Assert.Fail($"At least one of the two entities is NULL");
            }

            var firstEntityType = firstEntity.GetType();
            var secondEntityType = secondEntity.GetType();

            Type typeToUseForPropEnumeration = null;
            if (firstEntityType != secondEntityType)
            {
                // we do allow comparisons between base and derived types, we just need to get the order right
                if (firstEntityType.IsAssignableFrom(secondEntityType))
                {
                    typeToUseForPropEnumeration = firstEntityType;
                }
                else if (secondEntityType.IsAssignableFrom(firstEntityType))
                {
                    typeToUseForPropEnumeration = secondEntityType;
                }
                else
                {
                    Assert.Fail($"Entity '{firstEntityType}' and '{secondEntityType}' can't be used for db entity comparison");
                }
            }
            else
            {
                typeToUseForPropEnumeration = firstEntityType;
            }

            var comparisonDecision = 0;

            foreach (var propDescriptor in TypeDescriptor.GetProperties(typeToUseForPropEnumeration).OfType<PropertyDescriptor>())
            {
                var propType = propDescriptor.PropertyType;
                var firstEntityPropValue = propDescriptor.GetValue(firstEntity);
                var secondEntityPropValue = propDescriptor.GetValue(secondEntity);

                // for the time being, ignore the properties that are of complex type
                // normally they are foreign key entities
                if (propType.IsValueType || propType == typeof(string))
                {
                    comparisonDecision = Comparer.Default.Compare(firstEntityPropValue, secondEntityPropValue);

                    if (comparisonDecision != 0)
                    {
                        // for dates, SQL Server only stores time to approximately 1/300th of a second or 3.33ms so we need to treat them differently
                        var dateComparisonsMaxAllowedTicks = TimeSpan.FromMilliseconds(2*3.33).Ticks;
                        if (propType == typeof(Nullable<DateTime>) || propType == typeof(DateTime))
                        {
                            if (Math.Abs(((DateTime)secondEntityPropValue).Ticks - ((DateTime)firstEntityPropValue).Ticks) <= dateComparisonsMaxAllowedTicks)
                            {
                                comparisonDecision = 0;
                            }
                        }

                        if (propType == typeof(Nullable<DateTimeOffset>) || propType == typeof(DateTimeOffset))
                        {
                            if (Math.Abs(((DateTimeOffset)secondEntityPropValue).Ticks - ((DateTimeOffset)firstEntityPropValue).Ticks) <= dateComparisonsMaxAllowedTicks)
                            {
                                comparisonDecision = 0;
                            }
                        }
                    }
                }

                if (comparisonDecision != 0)
                {
                    // no reason to continue
                    return comparisonDecision;
                }
            }

            return comparisonDecision;
        }
    }
}
