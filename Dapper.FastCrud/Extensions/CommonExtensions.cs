namespace Dapper.FastCrud.Extensions
{
    using Dapper.FastCrud.Validations;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A set of commonly used extensions.
    /// </summary>
    internal static class CommonExtensions
    {
        /// <summary>
        /// Returns the property descriptor from a member expression.
        /// </summary>
        public static PropertyDescriptor GetPropertyDescriptor<TType, TPropType>(this Expression<Func<TType, TPropType>> expr)
        {
            Requires.NotNull(expr, nameof(expr));

            var type = typeof(TType);
            var propertyName = expr.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                UnaryExpression unaryExpression => unaryExpression.Operand switch
                {
                    MemberExpression memberExpression => memberExpression.Member.Name,
                    _ => throw new InvalidOperationException($"Unable to extract the property descriptor from the unary expression operand {unaryExpression?.Operand}")
                },
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
    }
}
