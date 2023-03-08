namespace Dapper.FastCrud.Validations
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
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
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotDefault<T>(
#if NETSTANDARD2_1
            [NotNull]
#endif
            T value, 
            string parameterName)
            where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                throw new ArgumentOutOfRangeException(parameterName, $"Parameter {parameterName} is expected to have a value different than its default.");
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull<T>(
#if NETSTANDARD2_1
            [NotNull]
#endif
            T? value, 
            string parameterName)
            where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull<T>(
#if NETSTANDARD2_1
            [NotNull]
#endif
            T? value, 
            string parameterName)
            where T : class // ensures value-types aren't passed to a null checking method
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null or empty.
        /// </summary>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <c>null</c> or empty.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmpty(
#if NETSTANDARD2_1
            [NotNull]
#endif
            string? value, 
            string parameterName)
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
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrWhiteSpace(
#if NETSTANDARD2_1
            [NotNull]
#endif
            string? value, 
            string parameterName)
        {
            NotNull(value, parameterName);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Parameter '{parameterName}' is either empty or contains only whitespaces.", parameterName);
            }
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null or has no elements.
        /// </summary>
        /// <param name="values">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if the tested condition is false.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmpty(
#if NETSTANDARD2_1
            [NotNull]
#endif
            IEnumerable? values, 
            string parameterName)
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
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmptyOrNullElements<T>(
#if NETSTANDARD2_1
            [NotNull]
#endif
            IEnumerable<T>? values, 
            string parameterName)
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
        /// Throws an exception if the specified parameter's value is null,
        /// has no elements or has an element with a null value.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="values">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if the tested condition is false.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrNullElements<T>(
#if NETSTANDARD2_1
            [NotNull]
#endif
            IEnumerable<T>? values, 
            string parameterName)
            where T : class // ensures value-types aren't passed to a null checking method
        {
            NotNull(values, parameterName);

            foreach (T value in values)
            {
                if (value == null)
                {
                    throw new ArgumentException($"An element inside '{parameterName}' is null.", parameterName);
                }
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if a condition does not evaluate to true.
        /// </summary>
        //[ContractAnnotation("condition:false => halt")]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Range(bool condition, string parameterName, string? message = null)
        {
            if (!condition)
            {
                FailRange(parameterName, message);
            }
        }

        /// <summary>
        /// Throws an ArgumentException if a condition does not evaluate to true.
        /// </summary>
        //[ContractAnnotation("condition:false => halt")]
        [DebuggerStepThrough]
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
        //[ContractAnnotation("condition:false => halt")]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidState(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Checks whether a directory exists on the disk.
        /// </summary>
        /// <param name="directoryPath">Path of the directory</param>
        /// <param name="parameterName">Name of the parameter</param>
        //[ContractAnnotation("directoryPath:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DirectoryExists(
#if NETSTANDARD2_1
            [NotNull]
#endif
            string? directoryPath, 
            string parameterName)
        {
            NotNullOrWhiteSpace(directoryPath, parameterName);
            if (!Directory.Exists(directoryPath))
            {
                throw new ArgumentException($"Directory '{directoryPath}' could not be found", parameterName);
            }
        }

        /// <summary>
        /// Checks whether a path is rooted and is a subfolder of the base directory.
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <param name="baseDirectory">The folder the path must be rooted in</param>
        /// <param name="parameterName">Name of the parameter</param>
        //[ContractAnnotation("path:null => halt")]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PathIsRooted(
#if NETSTANDARD2_1
            [NotNull]
#endif
            string? path,
#if NETSTANDARD2_1
            [NotNull]
#endif
            string? baseDirectory, 
            string parameterName)
        {
            NotNullOrWhiteSpace(path, parameterName);
            NotNullOrWhiteSpace(baseDirectory, nameof(baseDirectory));
            Argument(Path.IsPathRooted(path), parameterName, $"Path '{path}' is not rooted");
            Argument(Path.IsPathRooted(baseDirectory), nameof(baseDirectory), $"Path '{baseDirectory}' is not rooted");
            Argument(path.TrimEnd(Path.DirectorySeparatorChar).StartsWith(baseDirectory.TrimEnd(Path.DirectorySeparatorChar), StringComparison.CurrentCultureIgnoreCase), parameterName, $"Path '{path}' does not represent a subfolder of '{baseDirectory}'");
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if a condition does not evaluate to true.
        /// </summary>
        /// <returns>Nothing.  This method always throws.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TargetIsType<T>(
#if NETSTANDARD2_1
            [NotNull]
#endif
            object? target, 
            string name)
        {
            if ((target == null) || !(target is T))
            {
                throw new ArgumentException($"{name} is not assignable to {typeof(T).FullName}");
            }
        }

        /// <summary>
        /// Checks whether a file exists on the disk.
        /// </summary>
        /// <param name="filePath">Path of the file</param>
        /// <param name="parameterName">Name of the parameter</param>
        //[ContractAnnotation("filePath:null => halt")]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FileExists(
#if NETSTANDARD2_1
            [NotNull]
#endif
            string? filePath, 
            string parameterName)
        {
            NotNullOrWhiteSpace(filePath, parameterName);
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"File '{filePath}' could not be found", parameterName);
            }
        }

        /// <summary>
        /// Checks whether a file does not exist on the disk.
        /// </summary>
        /// <param name="filePath">Path of the file</param>
        /// <param name="parameterName">Name of the parameter</param>
        //[ContractAnnotation("filePath:null => halt")]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FileNotFound(
#if NETSTANDARD2_1
            [NotNull]
#endif
            string? filePath, 
            string parameterName)
        {
            NotNullOrWhiteSpace(filePath, parameterName);
            if (File.Exists(filePath))
            {
                throw new ArgumentException($"File '{filePath}' already exists", parameterName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if a condition does not evaluate to true.
        /// </summary>
        /// <returns>Nothing.  This method always throws.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FailRange(string parameterName, string? message = null)
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
