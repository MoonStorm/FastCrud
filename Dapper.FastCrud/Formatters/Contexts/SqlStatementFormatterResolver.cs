namespace Dapper.FastCrud.Formatters
{
    using Dapper.FastCrud.Mappings.Registrations;
    using Dapper.FastCrud.Validations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Holds information about an entity participating in a statement.
    /// </summary>
    internal sealed class SqlStatementFormatterResolver:IDisposable
    {
        private static AsyncLocal<Stack<SqlStatementFormatterResolver>> _currentResolvers = new AsyncLocal<Stack<SqlStatementFormatterResolver>>();
        private bool _disposed = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SqlStatementFormatterResolver(
            bool forceFullyQualifiedColumns,
            EntityRegistration entityRegistration,
            ISqlBuilder sqlBuilder,
            string? alias = null)
        {
            Requires.NotNull(entityRegistration, nameof(entityRegistration));
            Requires.NotNull(sqlBuilder, nameof(sqlBuilder));

            this.ForceFullyQualifiedColumns = forceFullyQualifiedColumns;
            this.EntityRegistration = entityRegistration;
            this.SqlBuilder = sqlBuilder;
            this.Alias = alias;

            if (_currentResolvers.Value == null)
            {
                _currentResolvers.Value = new Stack<SqlStatementFormatterResolver>();
            }

            _currentResolvers.Value.Push(this);
        }

        /// <summary>
        /// The entity registration.
        /// </summary>
        public EntityRegistration EntityRegistration { get; }

        /// <summary>
        /// If true, simple column names will never show up in the formatted string.
        /// They will always be prepended with the table name or alias.
        /// </summary>
        public bool ForceFullyQualifiedColumns { get; }

        /// <summary>
        /// The associated SQL builder.
        /// </summary>
        public ISqlBuilder SqlBuilder { get; }

        /// <summary>
        /// The alias as it was assigned for the statement formatter.
        /// </summary>
        public string? Alias { get; }

        /// <summary>
        /// Returns the currently active resolver.
        /// </summary>
        public static SqlStatementFormatterResolver? Current
        {
            get
            {
                if (_currentResolvers.Value == null || _currentResolvers.Value.Count == 0)
                {
                    return null;
                }

                return _currentResolvers.Value.Peek();
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _currentResolvers.Value.Pop();
                if (_currentResolvers.Value.Count == 0)
                {
                    _currentResolvers.Value = null;
                }
            }
        }
    }
}
