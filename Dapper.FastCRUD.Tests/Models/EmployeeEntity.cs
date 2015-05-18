using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.FastCrud.Tests.Models
{
    public partial class Employee
    {
        protected bool Equals(Employee other)
        {
            return this.UserId == other.UserId && this.EmployeeId.Equals(other.EmployeeId) && this.KeyPass.Equals(other.KeyPass) && string.Equals(this.LastName, other.LastName) && string.Equals(this.FirstName, other.FirstName) && this.BirthDate.Equals(other.BirthDate) && this.WorkstationId == other.WorkstationId;
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
            return Equals((Employee)obj);
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
                var hashCode = this.UserId;
                hashCode = (hashCode * 397) ^ this.EmployeeId.GetHashCode();
                hashCode = (hashCode * 397) ^ this.KeyPass.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.LastName != null ? this.LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.FirstName != null ? this.FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.BirthDate.GetHashCode();
                hashCode = (hashCode * 397) ^ this.WorkstationId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Employee left, Employee right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Employee left, Employee right)
        {
            return !Equals(left, right);
        }
    }
}
