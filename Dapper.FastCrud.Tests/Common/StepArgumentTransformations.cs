namespace Dapper.FastCrud.Tests.Common
{
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
    }
}
