namespace Dapper.FastCrud.Benchmarks
{
    using System;
    using System.Data.SqlTypes;
    using Dapper.FastCrud.Tests.Models;

    public class EntityGenerationSteps
    {
        protected SimpleBenchmarkEntity GenerateSimpleBenchmarkEntity(int entityIndex)
        {
            return new SimpleBenchmarkEntity()
            {
                FirstName = $"First Name {entityIndex}",
                LastName = $"Last Name {entityIndex}",
                DateOfBirth = new SqlDateTime(DateTime.Now).Value
            };
        }
    }
}
