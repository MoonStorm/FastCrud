namespace Dapper.FastCrud.Tests.Models.Poco
{
    using System.Collections.Generic;
    using Dapper.FastCrud.Tests.DatabaseSetup;
    using Dapper.FastCrud.Tests.Models.CodeFirst;

    /// <summary>
    /// Entity used for POCO tests.
    /// A fluent style registration is being used for the mappings inside <see cref="CommonDatabaseSetup.SetupOrmConfiguration"/>.
    /// </summary>
    public class Building
    {
        public int? BuildingId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<Workstation> Workstations { get; set; }
    }
}
