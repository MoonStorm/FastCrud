namespace Dapper.FastCrud.Extensions
{
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A set of commonly used extensions.
    /// </summary>
    internal static class CommonExtensions
    {
        private static readonly int _randomSeed = new Random().Next(int.MinValue, int.MaxValue);

        /// <summary>
        /// Returns the property descriptor from a member expression.
        /// </summary>
        public static PropertyDescriptor GetPropertyDescriptor<TType, TPropType>(this Expression<Func<TType, TPropType>> expr)
        {
            Requires.NotNull(expr, nameof(expr));

            var type = typeof(TType);
            var propertyName = expr.Body switch
            {
                // classic TType -> TPropertyType?
                MemberExpression memberExpression => memberExpression.Member.Name,

                // TType -> object?
                UnaryExpression unaryExpression => unaryExpression.Operand switch
                {
                    MemberExpression memberExpression => memberExpression.Member.Name,
                    _ => throw new InvalidOperationException($"Unable to extract the property descriptor from the unary expression operand {unaryExpression?.Operand}")
                },

                // don't know
                _ => throw new InvalidOperationException($"Unable to extract the property descriptor from the expression body {expr?.Body}")
            };
            var properties = TypeDescriptor.GetProperties(type)
                                           .OfType<PropertyDescriptor>()
                                           .Where(propDesc => propDesc.Name == propertyName)
                                           .ToArray();

            if (properties.Length == 0)
            {
                throw new ArgumentException($"Unable to find property '{propertyName}' on type '{type.FullName}'");
            }

            if (properties.Length > 1)
            {
                throw new ArgumentException($"Found more than one property named '{propertyName}' on type '{type.FullName}'");
            }

            return properties[0];
        }

        /// <summary>
        /// Returns the entity type from a property denoting an entity or a collection of entities.
        /// </summary>
        public static Type GetEntityType(this PropertyDescriptor property)
        {
            Requires.NotNull(property, nameof(property));

            if (IsEntityCollectionProperty(property))
            {
                return property.PropertyType.GetGenericArguments()[0];
            }

            return property.PropertyType;
        }

        /// <summary>
        /// Returns true if the property represents a collection of entities.
        /// </summary>
        public static bool IsEntityCollectionProperty(this PropertyDescriptor property)
        {
            Requires.NotNull(property, nameof(property));

            var propertyType = property.PropertyType;
            return typeof(IEnumerable).IsAssignableFrom(propertyType)
                   && propertyType.IsGenericType
                   && propertyType.GetGenericArguments().Length == 1;

        }

        /// <summary>
        /// Combines a hash with other hashes
        /// </summary>
        public static int CombineHash(this int startHash, params int?[] otherHashes)
        {
            var computedHash = CombineHashInternal(_randomSeed, startHash);
            foreach (var otherHash in otherHashes)
            {
                computedHash = CombineHashInternal(computedHash, otherHash ?? 0);
            }

            return computedHash;
        }

        // extracted from the ValueTuple implementation
        private static int CombineHashInternal(int h1, int h2)
        {
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}
