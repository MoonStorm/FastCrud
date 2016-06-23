namespace Dapper.FastCrud.Tests.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Workstation
    {
        [ThreadStatic]
        private static bool _isCheckingForBuildingEquality; // prevents an equality check loop

        /// <summary>
        /// Extra property, not found in the database used by the T4 entity generation, used to test the relationship with the <see cref="Building"/> entity.
        /// </summary>
        [ForeignKey(nameof(Building))]
        public int? BuildingId { get; set; }

        /// <summary>
        /// Extra foreign entity, referenced by <see cref="BuildingId"/>.
        /// </summary>
        public Building Building { get; set; }

        protected bool Equals(Workstation other)
        {
            var isEqual = 
                this.WorkstationId == other.WorkstationId
                && this.BuildingId == other.BuildingId
                && string.Equals(this.Name, other.Name)
                && this.AccessLevel == other.AccessLevel
                && !(this.Employees??new Employee[0]).Except(other.Employees??new Employee[0]).Any();

            if (isEqual)
            {
                if (!_isCheckingForBuildingEquality)
                {
                    _isCheckingForBuildingEquality = true;
                    try
                    {
                        return object.Equals(this.Building, other.Building);
                    }
                    finally
                    {
                        _isCheckingForBuildingEquality = false;
                    }
                }
            }

            return isEqual;
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

            if (Equals((Workstation)obj))
            {
                return true;
            }

            return false;
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
