The fastest micro-orm extension for Dapper.

For Dapper constructs in general, it is recommended to use Visual Studio 2015 for features such as nameof and string interpolation, but that's not a requirement.

The package contains .NET 4.5 and 4.6 DLLs, one of which will be installed based on the target framework in your project. 
For .NET 4.5, the package will also install the dependency ``StringInterpolationBridge``, which contains the polyfills required when using string interpolation with the C# 6 compiler in VS 2015.

#### Features:
- Support for LocalDb, Ms Sql Server, MySql, SqLite, PostgreSql
- Entities having composite primary keys are supported
- Multiple entity mappings are supported, useful for partial queries in large denormalized tables and data migrations between different database types.
- All the CRUD methods accept a transaction, a command timeout, and a custom entity mapping.
- Fast pre-computed entity queries
- A simple Sql builder with alias support is provided, which is very useful when manual SQL queries are unavoidable.
- A generic T4 template is also provided for convenience in the NuGet package ``Dapper.FastCrud.ModelGenerator``. Entity domain partitioning and generation can be achieved via separate template configurations. 
Code first entities are also supported which can either be decorated with attributes such as Table, Key and DatabaseGenerated, or can have their mappings programmatically set.

#### Usage:
- using Dapper.FastCrud
- OrmConfiguration.DefaultDialect = SqlDialect.MsSql | MySql | SqLite | PostgreSql
- dbConnection.Insert(newEntity);
- dbConnection.Get()
- dbConnection.Get(new Entity() {Id = 10});
- dbConnection.Update(updatedEntity);
- dbConnection.Delete(entity)
- dbConnection.Count()
- dbConnection.Find<Entity>(
        whereClause:$"{nameof(Entity.FirstName)}=@FirstNameParam", 
        orderClause:$"{nameof(Entity.LastName)} DESC", 
		skipRowsCount:10, limitRowsCount:20,
        queryParameters: new {FirstNameParam: "John"});
This is where the power of the C# 6 compiler comes into play, and leaves no chance to mistypings or to problems arising from db entity refactorings.

#### POCOs
You can provide information about the entities that are going to be used in DB operations, without decorating them with attributes, using ``OrmConfiguration.RegisterEntity`` or  ``OrmConfiguration.SetDefaultEntityMapping``.
```
OrmConfiguration.RegisterEntity<Building>()
                .SetTableName("Buildings")
                .SetProperty(nameof(Building.BuildingId), PropertyMappingOptions.KeyProperty)
                .SetProperty(nameof(Building.Name));

```
Alternatively you can let the library create the default mappings, which can then be queried via ``OrmConfiguration.GetDefaultEntityMapping``, tweaked and set back again as defaults or for multi-mapping purposes. 


#### Entity Generation
Entity generation can be easily performed by installing the NuGet package ``Dapper.FastCrud.ModelGenerator`` and adjusting your (``*Config.tt``) files. Use the sample config template for inspiration. Do not modify the ``GenericModelGenerator.tt`` as that will prevent future upgrades via NuGet. You'll need a LocalDb or an MsSql server that contains the schema. 
By default the script looks into the ``app.config`` file for a connection string, but I would strongly advise to use a separate ``.config`` file and adjust the template config accordingly.
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="EntityGeneration" providerName="System.Data.SqlClient" connectionString="Data Source=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\TestDatabase.mdf;Initial Catalog=TestDatabase;Integrated Security=True" />
  </connectionStrings>
</configuration>
```
When this is all done, open up the ``*Config.tt`` file and save it. Alternatively right click on the file and click on ``Run Custom Tool``. Your entities will be generated.

#### Rules
To understand the logic behind the CRUD operations, let's have a look at an entity ``Employee``, produced by the T4 template. The same rules apply for code first entities with mappings set at runtime.
```
    /// <summary>
    /// A class which represents the Employee table.
    /// </summary>
	[Table("Employees")]
	public partial class Employee
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public virtual int UserId { get; set; }
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public virtual Guid EmployeeId { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public virtual Guid KeyPass { get; set; }
		public virtual string LastName { get; set; }
		public virtual string FirstName { get; set; }
		public virtual DateTime BirthDate { get; set; }
		public virtual int? WorkstationId { get; set; }
	}

```
- The ``Table`` attribute specifies the table used to produce the entity.  
- The ``Key`` attribute is used to mark the properties that represent the primary key, and in this particular case, we're dealing with a composite key.
``Update``, ``Get``, ``Delete`` will always use the properties decorated with a ``Key`` attribute to identify the entity the operation is targeting.
- The ``DatabaseGenerated`` attribute is generated for database fields that are either identity columns or have a default value. 
An ``Insert`` operation will always exclude properties decorated with a ``DatabaseGenerated`` attribute, but will update the entity with the database generated values on return.
In case you have primary keys and other fields that are not database generated, it's your responsibility to set them up prior to calling ``Insert``.
- While this was not produced by the T4 template, you can use the ``Column`` attribute to override the column name, if you so desire, in a code first approach.

#### Code First
You don't have to use the T4 template or the attributes to describe your entity. You can register the entities at runtime.
```
    OrmConfiguration.RegisterEntity<Building>()
        .SetTableName("Buildings")
        .SetProperty(building => buildingRenovationDate)
        .SetProperty(building => building.BuildingId,prop => prop.SetPrimaryKey()..SetDatabaseGenerated().SetDatabaseColumnName("Id"))
        .SetProperty(building => building.Name, prop=> prop.SetDatabaseColumnName("BuildingName"));

```

#### Multi-Mappings
This is a unique concept that helps in data migration accross multiple types of databases, partial updates, and so much more.
Every entity has a default mapping attached, which informs FastCrud about how to construct the SQL queries. This default mapping can be set or queried via ``OrmConfiguration.SetDefaultEntityMapping`` and ``OrmConfiguration.GetDefaultEntityMapping``. 

Alternatively you can have other mappings tweaked for the same entities, which can be passed to any of the CRUD methods. These mappings should be built once and reused, and you can do so by either cloning an existing instance or by creating one from scratch.
```
    var partialUpdateMapping = OrmConfiguration
        .GetDefaultEntityMapping<Employee>()
        .Clone()
        .UpdatePropertiesExcluding(prop=>prop.IsExcludedFromUpdates=true, nameof(Employee.LastName));
    databaseConnection.Update(
        new Employee { 
            Id=id, 
            LastName="The only field that is going to get updated"
        }, 
        entityMappingOverride:customMapping);
```
In the previous example, only the ``LastName`` field will be updated. 

You can also remove entire property mappings, which allows you to work with a subset of your pre-generated entity for any db operations, useful for large denormalized tables:
```
   var partialSetMapping =
        OrmConfiguration.GetDefaultEntityMapping<CompanyInformation>()
                        .RemoveAllPropertiesExcluding(
                            nameof(CompanyInformation.Id),
                            nameof(CompanyInformation.Email),
                            nameof(CompanyInformation.Phone),
                            nameof(CompanyInformation.Name));

```
This custom mapping override can then be passed to any CRUD methods, just be careful not to use it for inserts.

You can also create a mapping that uses a different dialect, useful for migrating data from one database type to another.
```
    var destinationMapping = OrmConfiguration
    	.GetDefaultEntityMapping<CompanyInformation>
    	.SetDialect(SqlDialect.SqLite);
```
You can then pass this mapping to the insert method.

#### Manual Sql Constructs
``OrmConfiguration.GetSqlBuilder<TEntity>()`` gives you access to an SQL builder which is really helpful when you have to construct your own SQL queries.

#### Speed
Most of us love Dapper for its speed. 
Without taking this away from you, Dapper.FastCRUD helps by giving you type safety and can even be used as foundation for  your DAL repositories.
Let's have a look at how it performs against other similar libraries out there (``Dapper.SimpleCRUD v1.8.7``, ``DapperExtensions v1.4.4 ``).

##### Automatic Benchmark Report (Last Run: Thursday, May 28, 2015)
|           |  Operation | Count |Time (ms) | Time/op (μs) |
------------|------------|-------|----------|--------------
Dapper | update | 20000 | 4,219.74 | 210.99  
Fast Crud | update | 20000 | 4,254.11 | 212.71  
Dapper Extensions | update | 20000 | 5,673.44 | 283.67  
Simple Crud | update | 20000 | 7,250.60 | 362.53  
Dapper | select by id | 20000 | 2,120.96 | 106.05  
Fast Crud | select by id | 20000 | 2,502.21 | 125.11  
Dapper Extensions | select by id | 20000 | 3,558.76 | 177.94  
Simple Crud | select by id | 20000 | 5,893.90 | 294.70  
Dapper | delete | 20000 | 3,824.91 | 191.25  
Fast Crud | delete | 20000 | 3,717.07 | 185.85  
Dapper Extensions | delete | 20000 | 4,622.52 | 231.13  
Simple Crud | delete | 20000 | 5,953.60 | 297.68  
Dapper | insert | 20000 | 4,352.63 | 217.63  
Fast Crud | insert | 20000 | 4,877.35 | 243.87  
Dapper Extensions | insert | 20000 | 6,106.09 | 305.30  
Simple Crud | insert | 20000 | 9,029.64 | 451.48  
Dapper | select all | 3 | 134.95 | 44,984.57  
Fast Crud | select all | 3 | 185.02 | 61,672.73  
Dapper Extensions | select all | 3 | 203.24 | 67,745.60  
Simple Crud | select all | 3 | 875.43 | 291,808.73  


Dapper is used as reference only, for the purpose of observing the overhead of the automatic CRUD features compared to a verbatim SQL construct. The database is re-created at every run, data file is pre-allocated, and the statistics are turned off.
External factors might influence the results, but they should be fairly consistent as the tests are following the same steps and are running on the same number and size of records. 
Environment details: Windows 7, i7 3930K @3.2GHz, 16GB DDR3-1600, SATA600 SSD

##### Install via NuGet and Enjoy !

