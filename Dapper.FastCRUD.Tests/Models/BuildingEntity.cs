using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.FastCrud.Tests.Models
{
    public partial class Building
    {
        protected bool Equals(Building other)
        {
            return this.BuildingId == other.BuildingId && string.Equals(this.Name, other.Name);
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
                return (this.BuildingId * 397) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
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
