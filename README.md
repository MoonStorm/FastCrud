Simple extensions added to the IDbConnection for convenience and a less error prone experience. This library is roughly twice as fast than Dapper.SimpleCRUD.
For Dapper constructs in general, it is recommended to use Visual Studio 2015 for features such as nameof and string interpolation but that's not a requirement.

The package contains .NET 4.5 and 4.6 DLLs, one of which will be installed based on the target framework in your project. 
For .NET 4.5, the code contains the polyfills for the missing FormattableString class, which is required when targetting that framework version and using string interpolation with the C# 6 compiler in VS 2015.

#### Features:
- Support for LocalDb, Ms Sql Server, MySql, SqLite, PostgreSql
- Entities having composite primary keys are supported
- All the CRUD methods accept a transaction and a command timeout
- Fast pre-computed entity queries
- A generic T4 template is also provided for convenience. Entity domain partitioning and generation can be achieved via separate template configurations. Existing POCO entities are also supported which can be manually decorated with attributes such as Table, Key and DatabaseGenerated. Column name overrides are not supported and not recommended. As you'll end up writing more complex SQL queries, outside of the domain of this library, you'll want to use the nameof operator as much as possible.

#### Usage:
- DapperExtensions.Dialect = SqlDialect.MsSql | MySql | SqLite | PostgreSql
- dbConnection.Insert(newEntity);
- dbConnection.Get()
- dbConnection.Get(new Entity() {Id = 10});
- dbConnection.Update(updatedEntity);
- dbConnection.Delete(entity)
- dbConnection.GetTableName<Entity>();
- dbConnection.Find<Entity>(
        whereClause:$"{nameof(Entity.FirstName)}=@FirstNameParam", 
        orderClause:$"{nameof(Entity.LastName)} DESC", 
		skipRowsCount:10, limitRowsCount:20,
        queryParameters: new {FirstNameParam: "John"});

This is where the power of the C# 6 compiler comes into play, and leaves no chance to mistypings or to problems arising from db entity refactorings.

##### Entity Generation
Entity generation can be easily performed by adjusting your (``*Config.tt``) file. You'll need a LocalDb or an MsSql server that contains the schema. 
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


##### Rules
To understand the logic behind the CRUD operations, let's have a look at an entity ``Employee``, produced by the T4 template.

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


#### Automatic Benchmark Report (Last Run: Saturday, May 16, 2015)
|   |  Operation | Entity Count |Time (ms) |
------------|------------|--------------|-----------
Dapper | update | 20000 | 6,948.49 
Fast Crud | update | 20000 | 7,565.05 
Simple Crud | update | 20000 | 14,862.95 
Dapper | select by id | 20000 | 3,231.41 
Fast Crud | select by id | 20000 | 3,808.02 
Simple Crud | select by id | 20000 | 8,360.29 
Dapper | delete | 20000 | 6,596.93 
Fast Crud | delete | 20000 | 6,768.37  
Simple Crud | delete | 20000 | 10,950.20 
Dapper | insert | 20000 | 7,578.28 
Fast Crud | insert | 20000 | 8,561.58 
Simple Crud | insert | 20000 | 16,229.81 
Dapper | batch select - no filter | 20000 | 51.14 
Fast Crud | batch select - no filter | 20000 | 50.83 
Simple Crud | batch select - no filter | 20000 | 792.37 

Dapper is used as reference only, for the purpose of observing the overhead of the automatic CRUD features compared to a verbatim SQL construct. The database is re-created at every run. 
Database growth and other factors might influence the results, but they should be fairly consistent as the tests are following the same steps and are running on the same number and size of records.

##### Install via NuGet and Enjoy !

