namespace Dapper.FastCrud.Benchmarks.Targets.DapperExtensions
{
    using global::Dapper.FastCrud.Benchmarks.Models;
    using global::DapperExtensions.Mapper;

    internal class SimpleBenchmarkEntityMapper: ClassMapper<SimpleBenchmarkEntity>
    {
        public SimpleBenchmarkEntityMapper()
        {
            this.TableName = "SimpleBenchmarkEntities";
            this.Map(entity => entity.Id).Column("Id").Key(KeyType.Identity);
            this.Map(entity => entity.FirstName).Column("FirstName");
            this.Map(entity => entity.LastName).Column("LastName");
            this.Map(entity => entity.DateOfBirth).Column("DateOfBirth");
        }
    }
}
