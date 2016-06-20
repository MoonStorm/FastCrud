namespace Dapper.FastCrud.Tests.Models
{
    using System.Linq;

    public partial class Workstation
    {
        protected bool Equals(Workstation other)
        {
            return this.WorkstationId == other.WorkstationId
                   && string.Equals(this.Name, other.Name)
                   && this.AccessLevel == other.AccessLevel
                   && ((this.Employees == null && other.Employees == null) || (this.Employees!=null && other.Employees!=null && !this.Employees.Except(other.Employees).Any()));
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
            return Equals((Workstation)obj);
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
                var hashCode = this.WorkstationId.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.AccessLevel;
                return hashCode;
            }
        }

        public static bool operator ==(Workstation left, Workstation right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Workstation left, Workstation right)
        {
            return !Equals(left, right);
        }
    }
}
