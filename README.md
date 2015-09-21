The fastest micro-orm extension for Dapper.

For Dapper constructs in general, it is recommended to use Visual Studio 2015 for features such as nameof and string interpolation, but that's not a requirement.

The package contains .NET 4.5 and 4.6 DLLs, one of which will be installed based on the target framework in your project. 
For .NET 4.5, the package has a dependency on ``StringInterpolationBridge``, which contains the polyfills required when using string interpolation with the C# 6 compiler in VS 2015. The reason why the polyfills were not directly included in the library is to avoid clashes with other libraries that will sooner or later make use of them.

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

#### Entity generation
Entity generation can be easily performed by installing the NuGet package ``Dapper.FastCrud.ModelGenerator`` and by creating  your own (``*Config.tt``) files that use the generic template provided in this package. Use the sample config template for inspiration. Do not modify the ``GenericModelGenerator.tt`` as that will prevent future upgrades via NuGet. For this approach, you'll need a LocalDb or an MsSql server that contains the schema. 
By default the script looks into the ``app.config`` file for a connection string, but I would strongly advise creating a separate ``.config`` file and adjusting your ``*Config.tt`` accordingly.
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="EntityGeneration" providerName="System.Data.SqlClient" connectionString="Data Source=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\TestDatabase.mdf;Initial Catalog=TestDatabase;Integrated Security=True" />
  </connectionStrings>
</configuration>
```
When this is all done, open up the ``*Config.tt`` file and save it. Alternatively right click on the file and click on ``Run Custom Tool``. Your entities will be generated.

#### CRUD behavior
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
		public int UserId { get; set; }
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public Guid EmployeeId { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public Guid KeyPass { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public DateTime BirthDate { get; set; }
		public int? WorkstationId { get; set; }
	}

```
- The ``Table`` attribute specifies the table used to produce the entity.  
- The ``Key`` attribute is used to mark the properties that represent the primary key, and in this particular case, we're dealing with a composite key.
``Update``, ``Get``, ``Delete`` will always use the properties decorated with a ``Key`` attribute to identify the entity the operation is targeting.
- The ``DatabaseGenerated`` attribute is used for marking properties that are bound to identity columns or have a default value. 
An ``Insert`` operation will always exclude properties decorated with a ``DatabaseGenerated`` attribute, but will update the entity with the database generated values on return.
In case you have primary keys that are not database generated, it's your responsibility to set them up prior to calling ``Insert``.

#### Entity registration at runtime
You don't have to use the T4 template or the attributes to describe your entity. You can register the entities at runtime.
```
    OrmConfiguration.RegisterEntity<Building>()
        .SetTableName("Buildings")
        .SetProperty(building => buildingRenovationDate)
        .SetProperty(building => building.BuildingId,prop => prop.SetPrimaryKey().SetDatabaseGenerated())
        .SetProperty(building => building.Name);

```

#### Multi-mappings
This is a unique concept that helps in data migration accross multiple types of databases, partial updates, and so much more.
Every entity has a default mapping attached, which informs FastCrud about how to construct the SQL queries. This default mapping can be set or queried via ``OrmConfiguration.SetDefaultEntityMapping`` and ``OrmConfiguration.GetDefaultEntityMapping``. 

Alternatively you can have other mappings tweaked for the same entities, which can be passed to any of the CRUD methods. These mappings should be built once and reused, and you can do so by either cloning an existing instance or by creating one from scratch.
```
    var partialUpdateMapping = OrmConfiguration
        .GetDefaultEntityMapping<Employee>()
        .Clone() // clone it if you don't want to modify the default
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
   var partialSetMapping = OrmConfiguration
   			.GetDefaultEntityMapping<CompanyInformation>()
   			. Clone() // clone it if you don't want to modify the default
                        .RemoveAllPropertiesExcluding(
                            nameof(CompanyInformation.Id),
                            nameof(CompanyInformation.Email),
                            nameof(CompanyInformation.Phone),
                            nameof(CompanyInformation.Name));

```

You can also create a mapping that uses a different dialect, useful for migrating data from one database type to another.
```
    var destinationMapping = OrmConfiguration
    	.GetDefaultEntityMapping<CompanyInformation>
    	.Clone() // clone it if you don't want to modify the default
    	.SetDialect(SqlDialect.SqLite);
```
You can then pass this mapping to the insert method.

#### Manual Sql constructs
``OrmConfiguration.GetSqlBuilder<TEntity>()`` gives you access to an SQL builder which is really helpful when you have to construct your own SQL queries.

#### FAQ

###### No database column name overrides?
Short answer: No.

Long answer: There is support in the library for overriding the default database column name bound to a property, both in the form of a standard framework attribute (``ColumnAttribute``) or at runtime (``SetDatabaseColumnName``). I would STRONGLY advise against going down this path. If you do this, all the verbatim clauses that you'll use sooner or later will fail with the ``nameof(Entity.Property)`` operator. You'll need to use the ``ISqlBuilder`` extensively to properly resolve your properties to DB fields. The objects at this level are low level database entities. You shouldn't be afraid to apply rename refactoring if you know you've used ``nameof`` everywhere. Rename refactoring at this level won't impact your business level/DTO entities.

###### What about relationships?
Due to the complexity of JOINs, you'll have to deal with this manually for the time being. Again, make extensive use of ``nameof`` and the sql builder present in this library. The experimental support in the provided T4 template or in the library should not be used at this point.

###### Postgresql dialect should use a table/column delimiter!
While it is very easy for me to add one, I realized it would be a bad idea for general use. If you used delimiters to create your tables, all your queries that use non-delimited names would fail. In my opinion, a rubbish behavior exposed by  Postgresql. For example, if a table had a "Name" field, ``where {nameof(Entity.Name)}='John'`` would stop working. It would need to be changed to ``where \"{nameof(Entity.Name)}\"='John'`` or, if the library had enforced the delimiters, ``where {sqlBuilder.GetColumnName(nameof(Entity.Name))}='John'``. Not pretty. 

However, I understand that this should be the developer's choice, so I will add a way of setting the delimiters at runtime at some point in the future.


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

