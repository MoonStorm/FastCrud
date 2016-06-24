namespace Dapper.FastCrud.Tests.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [MetadataType(typeof(EmployeeMetadata))]
    public partial class Employee
    {
        [ThreadStatic]
        private static bool _isCheckingForWorkstationEquality; // prevents an equality check loop

        /// <summary>
        /// The metadata class contains extra db mapping info for existing properties on the main entity.
        /// Any properties that do not exist on the main entity will be ignored.
        /// </summary>
        private class EmployeeMetadata
        {
            [Column("Id")]
            public object UserId { get; } // just a marker
        }

        protected bool Equals(Employee other)
        {
            var isEqual = this.UserId == other.UserId && this.EmployeeId.Equals(other.EmployeeId) && this.KeyPass.Equals(other.KeyPass)
                          && string.Equals(this.LastName, other.LastName) && string.Equals(this.FirstName, other.FirstName)
                          && this.BirthDate.Equals(other.BirthDate) && this.WorkstationId == other.WorkstationId
                          && string.Equals(this.FullName, other.FullName);

            if (isEqual)
            {
                if (!_isCheckingForWorkstationEquality)
                {
                    _isCheckingForWorkstationEquality = true;
                    try
                    {
                        return object.Equals(this.Workstation, other.Workstation);
                    }
                    finally
                    {
                        _isCheckingForWorkstationEquality = false;
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
            if (Equals((Employee)obj))
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
                var hashCode = this.UserId;
                hashCode = (hashCode * 397) ^ this.EmployeeId.GetHashCode();
                hashCode = (hashCode * 397) ^ this.KeyPass.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.LastName != null ? this.LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.FirstName != null ? this.FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.BirthDate.GetHashCode();
                hashCode = (hashCode * 397) ^ this.WorkstationId.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Workstation != null ? this.Workstation.GetHashCode() : 0);
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


        // mysql 
        ////CREATE TABLE `Employee` (
	       //// UserId int NOT NULL,
        ////    EmployeeId BINARY(16) NOT NULL DEFAULT '0',
	       //// KeyPass BINARY(16) NOT NULL DEFAULT '0',
	       //// LastName nvarchar(50) NOT NULL,
        ////    FirstName nvarchar(50) NOT NULL,
        ////    BirthDate datetime NOT NULL,
        ////    WorkstationId int NULL,
        ////    PRIMARY KEY(UserId, EmployeeId)
        ////);

        ////CREATE TRIGGER `Employee_Assign_UUID`
        ////  BEFORE INSERT ON Employee
        ////  FOR EACH ROW
        ////  SET NEW.EmployeeId = UNHEX(REPLACE(UUID(), '-', '')),
        ////  New.KeyPass = UNHEX(REPLACE(UUID(), '-', ''));

    }
}

