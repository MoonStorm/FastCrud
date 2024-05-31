namespace Dapper.FastCrud.Tests.DatabaseSetup
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Dapper.FastCrud.Tests.Common;
    using Dapper.FastCrud.Tests.Models.Poco;
    using Dapper.FastCrud.Validations;
    using Microsoft.Extensions.Configuration;

    public class CommonDatabaseSetup
    {
        /// <summary>
        /// Retrieves the connection string from the configuration file.
        /// </summary>
        protected string GetConnectionStringFor(IConfiguration configuration, string connectionStringKey)
        {
            Validate.NotNull(configuration, nameof(configuration));

            var connectionString = configuration[$"connectionStrings:add:{connectionStringKey}:connectionString"];
            return connectionString;
        }

        protected void SetupOrmConfiguration(SqlDialect dialect)
        {
            OrmConfiguration.DefaultDialect = dialect;

            // setup any entities that are left via fluent registration
            OrmConfiguration.RegisterEntity<BuildingDbEntity>()
                            .SetTableName("Buildings")
                            .SetProperty(building => building.BuildingId, propMapping => propMapping.SetPrimaryKey().SetDatabaseGenerated(DatabaseGeneratedOption.Identity).SetDatabaseColumnName("Id"))
                            .SetProperty(building => building.Name, propMapping => propMapping.SetDatabaseColumnName("BuildingName"))
                            .SetProperty(building => building.Description) // test mapping w/o name
                            .SetParentChildrenRelationship(
                                building => building.Workstations,
                                workstation => workstation.BuildingId);

            // tweak some dapper mappings that still don't work properly for all the database flavors
            SqlMapper.RemoveTypeMap(typeof(DateOnly));
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

            SqlMapper.RemoveTypeMap(typeof(DateOnly?));
            SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());

            SqlMapper.RemoveTypeMap(typeof(TimeOnly));
            SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

            SqlMapper.RemoveTypeMap(typeof(TimeOnly?));
            SqlMapper.AddTypeHandler(new NullableTimeOnlyTypeHandler());
        }

    }
}
