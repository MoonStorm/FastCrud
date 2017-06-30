namespace Dapper.FastCrud.Validations
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Common runtime checks that throw ArgumentExceptions upon failure.
    /// </summary>
    internal static class Requires
    {
        /// <summary>
        /// Throws an exception if the specified parameter's value is the default.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>default</c></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NotDefault<T>(T value, string parameterName)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                throw new ArgumentOutOfRangeException(parameterName, $"Parameter {parameterName} is expected to have a value different than its default.");
            }

            return value;
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NotNull<T>(T value, string parameterName)
            where T : class // ensures value-types aren't passed to a null checking method
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        /////// <summary>
        /////// Throws an exception if the specified parameter's value is IntPtr.Zero.
        /////// </summary>
        /////// <param name="value">The value of the argument.</param>
        /////// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /////// <returns>The value of the parameter.</returns>
        /////// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is IntPtr.Zero</exception>
        ////[DebuggerStepThrough]
        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        ////public static IntPtr NotNull(IntPtr value, string parameterName)
        ////{
        ////    if (value == IntPtr.Zero)
        ////    {
        ////        throw new ArgumentNullException(parameterName);
        ////    }

        ////    return value;
        ////}

        /// <summary>
        /// Throws an exception if the specified parameter's value is null or empty.
        /// </summary>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <c>null</c> or empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            NotNull(value, parameterName);

            if (value.Length == 0 || value[0] == '\0')
            {
                throw new ArgumentException($"Parameter '{parameterName}' is empty.", parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null, empty, or whitespace.
        /// </summary>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <c>null</c> or empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrWhiteSpace(string value, string parameterName)
        {
            NotNull(value, parameterName);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Parameter '{parameterName}' is either empty or contains only whitespaces.", parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null,
        /// has no elements or has an element with a null value.
        /// </summary>
        /// <param name="values">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if the tested condition is false.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmpty(IEnumerable values, string parameterName)
        {
            NotNull(values, parameterName);

            var elementCount = (values as ICollection)?.Count;
            bool hasElements;
            if (elementCount.HasValue)
            {
                hasElements = elementCount > 0;
            }
            else
            {
                var enumerator = values.GetEnumerator();
                using (enumerator as IDisposable)
                {
                    hasElements = enumerator.MoveNext();
                }
            }

            if (!hasElements)
            {
                throw new ArgumentException($"Encountered an empty collection for parameter '{parameterName}'", parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null,
        /// has no elements or has an element with a null value.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="values">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if the tested condition is false.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmptyOrNullElements<T>(IEnumerable<T> values, string parameterName)
            where T : class // ensures value-types aren't passed to a null checking method
        {
            NotNull(values, parameterName);

            bool hasElements = false;
            foreach (T value in values)
            {
                hasElements = true;

                if (value == null)
                {
                    throw new ArgumentException($"An element inside '{parameterName}' is null.", parameterName);
                }
            }

            if (!hasElements)
            {
                throw new ArgumentException($"Encountered an empty collection for parameter '{parameterName}'", parameterName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if a condition does not evaluate to true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Range(bool condition, string parameterName, string message = null)
        {
            if (!condition)
            {
                FailRange(parameterName, message);
            }
        }

        /// <summary>
        /// Throws an ArgumentException if a condition does not evaluate to true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Argument(bool condition, string parameterName, string message)
        {
            if (!condition)
            {
                   throw new ArgumentException(message, parameterName);
            }
        }

        /// <summary>
        /// Validates some expression describing the acceptable condition for an argument evaluates to true.
        /// </summary>
        /// <param name="condition">The expression that must evaluate to true to avoid an <see cref="InvalidOperationException"/>.</param>
        /// <param name="message">The message to include with the exception.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidState(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if a condition does not evaluate to true.
        /// </summary>
        /// <returns>Nothing.  This method always throws.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FailRange(string parameterName, string message = null)
        {
            if (message == null)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
            else
            {
                throw new ArgumentOutOfRangeException(parameterName, message);
            }
        }
    }
}
