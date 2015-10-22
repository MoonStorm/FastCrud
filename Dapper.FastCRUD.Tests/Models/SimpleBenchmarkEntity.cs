namespace Dapper.FastCrud.Tests.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SimpleBenchmarkEntities")]
    public class SimpleBenchmarkEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual DateTime? DateOfBirth { get; set; }

        protected bool Equals(SimpleBenchmarkEntity other)
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
            return Equals((SimpleBenchmarkEntity)obj);
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

        public static bool operator ==(SimpleBenchmarkEntity left, SimpleBenchmarkEntity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SimpleBenchmarkEntity left, SimpleBenchmarkEntity right)
        {
            return !Equals(left, right);
        }

        // MYSQL table generation
        ////CREATE TABLE `SimpleBenchmarkEntities` (
	       //// Id int NOT NULL,
	       //// FirstName nvarchar(50) NULL,
	       //// LastName nvarchar(50) NOT NULL,
        ////    DateOfBirth date NULL,
        ////    PRIMARY KEY(Id)
        ////)

    }
}
