namespace Dapper.FastCrud.Tests.Common
{
    using System;
    using Dapper.FastCrud.Tests.Models.CodeFirst;
    using Dapper.FastCrud.Tests.Models.Metadata;
    using Dapper.FastCrud.Tests.Models.Poco;
    using Google.Protobuf.Reflection;
    using TechTalk.SpecFlow;

    [Binding]
    public class StepArgumentTransformations
    {
        [StepArgumentTransformation("asynchronous")]
        public bool AsyncMethodsToBoolean()
        {
            return true;
        }

        [StepArgumentTransformation("synchronous")]
        public bool SyncMethodsToBoolean()
        {
            return false;
        }

        [StepArgumentTransformation("workstation")]
        public Type WorkstationEntityToType()
        {
            return typeof(WorkstationDbEntity);
        }

        [StepArgumentTransformation("employee")]
        public Type EmployeeEntityToType()
        {
            return typeof(EmployeeDbEntity);
        }

        [StepArgumentTransformation("building")]
        public Type BuildingEntityToType()
        {
            return typeof(BuildingDbEntity);
        }

        [StepArgumentTransformation("badge")]
        public Type BadgeEntityToType()
        {
            return typeof(BadgeDbEntity);
        }

    }
}
