namespace Dapper.FastCrud.Tests
{
    using System;
    using System.Data.SqlTypes;
    using Dapper.FastCrud.Tests.Models;

    public class EntityGenerationSteps
    {
        private int entityIndex = 1;
        private static readonly Random rnd = new Random();

        protected Employee GenerateEmployeeEntity()
        {
            return new Employee()
            {
                WorkstationId = entityIndex++,
                FirstName = $"First Name {entityIndex}",
                LastName = $"Last Name {entityIndex}",
                BirthDate = new DateTime(rnd.Next(2000,2010), rnd.Next(1,12),rnd.Next(1,28), rnd.Next(0,23), rnd.Next(0,59), rnd.Next(0,59))
            };
        }

        protected Workstation GenerateWorkstationEntity()
        {
            return new Workstation()
            {
                Name = $"Workstation {entityIndex++}"
            };
        }

        protected Building GenerateBuildingEntity()
        {
            return new Building()
            {
                BuildingId = entityIndex,
                Name = $"Building {entityIndex++}"
            };
        }

    }
}
