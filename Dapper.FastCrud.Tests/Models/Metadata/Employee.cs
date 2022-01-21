namespace Dapper.FastCrud.Tests.Models.Metadata
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Dapper.FastCrud.Tests.Models.CodeFirst;
    using Dapper.FastCrud.Tests.Models.Poco;
    
    /// <summary>
    /// Metadata is set up in a separate class.
    /// In this case the entities are kept clean, with the exception of a metadata attribute.
    /// For a pure POCO solution, check out the <see cref="Building"/> entity.
    /// </summary>
    [MetadataType(typeof(EmployeeMetadata))]
    public class Employee
    {
        public virtual int UserId { get; set; }
        public virtual Guid EmployeeId { get; set; }
        public virtual Guid KeyPass { get; set; }
        public virtual string LastName { get; set; }
        public virtual string FirstName { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual long? WorkstationId { get; set; }
        public virtual string FullName { get; set; }
        public virtual Workstation Workstation { get; set; }
    }
}
