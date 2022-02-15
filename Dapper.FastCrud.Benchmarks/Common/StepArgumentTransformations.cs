namespace Dapper.FastCrud.Benchmarks.Common
{
    using System;
    using Dapper.FastCrud.Benchmarks.Models;
    using TechTalk.SpecFlow;

    [Binding]
    internal class StepArgumentTransformations
    {
        [StepArgumentTransformation("benchmark")]
        public Type WorkstationEntityToType()
        {
            return typeof(SimpleBenchmarkEntity);
        }
    }
}
