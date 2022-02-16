namespace Dapper.FastCrud.Tests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class ObjectMatchContext
    {
        private Dictionary<Tuple<object, object>, ObjectMatchResult?> _alreadyChecked = new Dictionary<Tuple<object, object>, ObjectMatchResult?>();
        private Dictionary<Type, PropertyInfo[]> _propertyInfos = new Dictionary<Type, PropertyInfo[]>();

        public ObjectMatchResult GetObjectMatchResult(object expected, object actual)
        {
                var key = new Tuple<object, object>(expected, actual);
                ObjectMatchResult result;
                if (!_alreadyChecked.TryGetValue(key, out result))
                {
                    result = new ObjectMatchResult(expected, actual);
                    _alreadyChecked.Add(key, result);
                }

                result.RegisterInterestedParty();
                return result;
        }

        public PropertyInfo[] GetProperties(Type entityType)
        {
                PropertyInfo[] properties;
                if (!_propertyInfos.TryGetValue(entityType, out properties))
                {
                    properties = entityType.GetProperties();
                    _propertyInfos.Add(entityType, properties);
                }

                return properties;
        }
    }
}
