namespace Dapper.FastCrud.Benchmarks.Models
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
