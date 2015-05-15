using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.FastCrud.Tests.Models
{
    public partial class SingleIntPrimaryKeyEntity
    {
        protected bool Equals(SingleIntPrimaryKeyEntity other)
        {
            return this.Id == other.Id && string.Equals(this.FirstName, other.FirstName) && string.Equals(this.LastName, other.LastName) && this.DateOfBirth.Equals(other.DateOfBirth);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
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
            return Equals((SingleIntPrimaryKeyEntity)obj);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Id;
                hashCode = (hashCode * 397) ^ (this.FirstName != null ? this.FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.LastName != null ? this.LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.DateOfBirth.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SingleIntPrimaryKeyEntity left, SingleIntPrimaryKeyEntity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SingleIntPrimaryKeyEntity left, SingleIntPrimaryKeyEntity right)
        {
            return !Equals(left, right);
        }
    }
}
