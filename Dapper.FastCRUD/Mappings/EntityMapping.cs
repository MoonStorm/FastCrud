namespace Dapper.FastCrud.Mappings
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Holds information about table mapped properties for a particular entity.
    /// </summary>
    public class EntityMapping
    {
        private static long _currentGlobalId = long.MinValue;
        private readonly long _id;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityMapping(Type entityType, string tableName = null, string schemaName = null)
        {
            this._id = Interlocked.Increment(ref _currentGlobalId);
            this.EntityType = entityType;
            this.PropertyMappings = new Dictionary<string, PropertyMapping>();
            this.TableName = tableName ?? entityType.Name;
            this.SchemaName = schemaName;
        }

        internal string TableName { get; private set; }
        internal string SchemaName { get; private set; }
        internal IDictionary<string, PropertyMapping> PropertyMappings { get; private set; }
        internal Type EntityType { get; private set; }

        protected bool Equals(EntityMapping other)
        {
            return this._id == other._id;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return this.Equals((EntityMapping)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this._id.GetHashCode();
        }

        public static bool operator ==(EntityMapping left, EntityMapping right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityMapping left, EntityMapping right)
        {
            return !Equals(left, right);
        }
    }
}
