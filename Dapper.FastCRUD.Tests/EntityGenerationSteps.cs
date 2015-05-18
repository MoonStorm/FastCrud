namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Data.SqlTypes;
    using Dapper.FastCrud.Tests.Models;

    public class EntityGenerationSteps
    {
        protected Employee GenerateEmployeeEntity(int entityIndex, int? workstationId = null)
        {
            return new Employee()
            {
                WorkstationId = workstationId,
                FirstName = $"First Name {entityIndex}",
                LastName = $"Last Name {entityIndex}",
                BirthDate = new SqlDateTime(DateTime.Now).Value
            };
        }
    }
}
