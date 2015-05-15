namespace Dapper.FastCrud.Tests.Features
{
    using System;
    using Dapper.FastCrud.Tests.Models;

    public class EntityGenerationSteps
    {
        protected SingleIntPrimaryKeyEntity GenerateSingleIntPrimaryKeyEntity(int entityIndex)
        {
            return new SingleIntPrimaryKeyEntity()
            {
                FirstName = string.Format("First Name {0}", entityIndex),
                LastName = string.Format("Last Name {0}", entityIndex),
                DateOfBirth = DateTime.Now.Date
            };
        }

    }
}
