namespace Dapper.FastCrud.Tests.Features
{
    using System;
    using System.Data.SqlTypes;
    using Dapper.FastCrud.Tests.Models;

    public class EntityGenerationSteps
    {
        protected SingleIntPrimaryKeyEntity GenerateSingleIntPrimaryKeyEntity(int entityIndex)
        {
            return new SingleIntPrimaryKeyEntity()
            {
                FirstName = $"First Name {entityIndex}",
                LastName = $"Last Name {entityIndex}",
                DateOfBirth = new SqlDateTime(DateTime.Now).Value
            };
        }

    }
}
