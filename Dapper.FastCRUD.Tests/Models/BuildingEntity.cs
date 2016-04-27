namespace Dapper.FastCrud.Tests.Models
{
    /// <summary>
    /// Code first entity. Do not set attributes!
    /// </summary>
    public class Building
    {
        public int BuildingId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        protected bool Equals(Building other)
        {
            return this.BuildingId == other.BuildingId && string.Equals(this.Name, other.Name) && string.Equals(this.Description, other.Description);
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
            return Equals((Building)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (this.BuildingId * 397) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Description != null ? this.Description.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Building left, Building right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Building left, Building right)
        {
            return !Equals(left, right);
        }
    }
}
