You hate verbatim SQL queries with zero type safety for your code but you love the speed? ``Dapper.FastCrud`` is a fast orm built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.
Visual Studio 2019 and above is recommended. 

#### What to expect when working with Dapper.FastCrud in the DAL? 
Type safety, clean code, less mistakes, more peace of mind, while still being close to the metal. Here's a sample for 3.0:
```
    var queryParams = new 
    {
        FirstName = "John",
        Street = "Creek Street"
    };

    var persons = dbConnection.Find<Person>(statement => statement
        .WithAlias("person")
        .Include<Address>(join => join.InnerJoin().WithAlias("address"))
        .Where($"{nameof(Person.FirstName):of person} = {nameof(queryParams.FirstName):P} AND {nameof(Address.Street):of address} = {nameof(queryParams.Street):P}")  
        .OrderBy($"{nameof(Person.LastName):of person} DESC")  
        .Skip(10)  
        .Top(20)  
        .WithParameters(queryParams);
```

#### Features:
- Support for LocalDb, Ms Sql Server, MySql, SqLite, PostgreSql.
- Entities having composite primary keys are supported, however note that the CRUD operations only support UNIQUE primary keys.
- Multiple entity mappings are supported, useful for partial queries in large denormalized tables and data migrations between different database types.
- All the CRUD methods accept a transaction, a command timeout, and a custom entity mapping.
- Fast pre-computed entity queries for simple CRUD operations.
- Compatible with component model data annotations.
- Opt-in relationships. As of 3.0, self referenced entities and multiple joins to the same target are also supported via aliases.
- A set of "formattables" are also included, which can be used even if you don't need the CRUD features of this library but you want to take advantage of the DB mappings.
- A generic T4 template for C# is also provided for convenience in the NuGet package Dapper.FastCrud.ModelGenerator.
- The following mapping styles are supported:
  - Database first (limited to SQL Server)
  - Code first, using model data annotations (preferred)
  - Fluent registration for POCO objects
  - Semi-POCO using metadata objects
- Extensibility points are also provided.

#### Active Versions
- 3.0-preview [![Build Status](https://moonstorm.visualstudio.com/Dapper.FastCrud/_apis/build/status/Master%20Branch%20Build%20Pipeline?branchName=master)](https://moonstorm.visualstudio.com/Dapper.FastCrud/_build/latest?definitionId=8&branchName=master)
  - Main library:  
    - Added support for .NET Standard 2.1
    - Extended support for the MetadataType attribute in .NET Standard 2.1
    - Bulk update can now be used with parameters.
    - Format specifier ":P" added for SQL parameters.
    - Format specifiers extended to support resolution via aliases in JOINs (e.g. "nameof(prop):of alias").
    - Methods adjusted for nullable support.
    - [Breaking change] Clean separation for the formatter and the sql builder. As a result, the access to the formatter got moved out of the ISqlBuilder and into the Sql static class.
    - Extended the functionality of the Sql "formattables", exposed via the Sql static class, to allow for easy access to both the raw resolved names and their SQL ready counterparts.
    - Relationships have been reworked:
      - [Breaking change] The fluent mapping setup has changed for setting up relationships.
      - The limit of 7 entities in a JOIN was removed.
      - The main entity and the JOINed entities can now be aliased. It is now recommended to do so when working with multiple entities in a statement for easy targeting in the WHERE clause.
      - JOIN support has been extended to the GET and COUNT methods.
      - When joins are used in a COUNT statement, DISTINCT is used.
      - SQL statements no longer require the presence of a relationship preset in the mappings. You can join with whatever you want, using whatever navigation properties you want (or none) and with any ON clause you desire.
      - Added support for self referenced entities (via InverseProperty attribute / fluent mappings / directly in the query).
      - Added support for one-to-one relationships (via InverseProperty attribute / fluent mappings / directly in the query).
      - Added support for multiple references to the same target (via InverseProperty attribute / fluent mappings / directly in the query).
    - A preview version has been published on NuGet.
  - Model generator (database first):
    - [Breaking change] Added support for self referenced entities.
    - [Breaking change] Added support for multiple references to the same target using the InverseProperty attribute.
    - [Breaking change] Better handling of columns representing reserved keywords in C#.
    - Support for new csproj style projects.
    - Fixed a problem preventing it from being used in VS2019 and later.
    - Pending NuGet publish.
  - Tests:
    - All the tests have been reviewed and most got refactored.
    - Workaround added for the [badly implemented serialization context in Specflow](https://github.com/SpecFlowOSS/SpecFlow/issues/1534).
    - Better model registration samples in the test project for:
      - POCO/fluent mapping
      - code first
      - metadata classes
      - database first
  - Wiki:
    - To be updated
- 2.6 [![Build Status](https://moonstorm.visualstudio.com/Dapper.FastCrud/_apis/build/status/Release%20Branch%20Build%20Pipeline?branchName=release)](https://moonstorm.visualstudio.com/Dapper.FastCrud/_build/latest?definitionId=10&branchName=release)
  - Upgraded the Dapper dependency.
  - Added support for .NET Standard 2.0 and .NET Framework 4.6.1
  - InsertAsync now calling the corresponding async Dapper method.
  - Added support for TimeSpan.
  - CI relocated.


#### WIKI
[The wiki](https://github.com/MoonStorm/Dapper.FastCRUD/wiki) is a great place for learning more about this library.


#### Speed
Let's have a look at some popular ORMs out there and benchmark their speed:  

- ``Dapper.SimpleCRUD v2.3.0``
- ``DapperExtensions v1.6.3 ``
- ``Entity Framework Core v6.0.2`` 

##### Automatic Benchmark Report (Last Run: Wednesday, February 16, 2022)

|  Library   |  Operation | Op Count |Time (ms) | Time/op (μs) |
|------------|------------|----------|----------|--------------|
| <a name="new_entry_marker"/> |
||||||
| Dapper | insert | 10000 | 2,974.40 | 297.44 |
| Fast Crud | insert | 10000 | 3,172.67 | 317.27 |
| Dapper Extensions | insert | 10000 | 3,529.16 | 352.92 |
| Simple Crud | insert | 10000 | 3,110.40 | 311.04 |
| Entity Framework - single op/call | insert | 10000 | 119,784.63 | 11,978.46 |
||||||
| Dapper | update | 10000 | 3,319.82 | 331.98 |
| Fast Crud | update | 10000 | 3,276.99 | 327.70 |
| Dapper Extensions | update | 10000 | 3,657.30 | 365.73 |
| Simple Crud | update | 10000 | 3,150.76 | 315.08 |
| Entity Framework - single op/call | update | 10000 | 235,678.77 | 23,567.88 |
||||||
| Dapper | delete | 10000 | 2,869.64 | 286.96 |
| Fast Crud | delete | 10000 | 2,916.99 | 291.70 |
| Dapper Extensions | delete | 10000 | 3,106.24 | 310.62 |
| Simple Crud | delete | 10000 | 3,120.13 | 312.01 |
| Entity Framework - single op/call | delete | 10000 | 7,326.03 | 732.60 |
||||||
| Dapper | select by id | 10000 | 1,499.67 | 149.97 |
| Fast Crud | select by id | 10000 | 1,861.50 | 186.15 |
| Dapper Extensions | select by id | 10000 | 2,026.22 | 202.62 |
| Simple Crud | select by id | 10000 | 2,113.70 | 211.37 |
| Entity Framework | select by id | 10000 | 3,665.71 | 366.57 |
||||||
| Dapper | select all | 10000 | 1,449.41 | 144.94 |
| Fast Crud | select all | 10000 | 1,748.86 | 174.89 |
| Dapper Extensions | select all | 10000 | 1,762.69 | 176.27 |
| Simple Crud | select all | 10000 | 2,281.44 | 228.14 |
| Entity Framework | select all | 10000 | 6,233.53 | 623.35 |

Dapper is included for reference. All the libs involved get their own internal cache cleared before each run, the benchmark database is re-created, data file gets pre-allocated, and the statistics are turned off.
The tests are following the same steps and are running in the same environment on the same number and size of records.
We're happy to see that most light ORMs have matured enough over the years and caught up with ``FastCrud`` and even exceeded its speed in some cases. 
The really heavy ones are still trailing far behind.

You can find more details about how to run the benchmarks yourself in the wiki.

##### Install [the main library](https://www.nuget.org/packages/Dapper.FastCrud/) and [the model generator](https://www.nuget.org/packages/Dapper.FastCrud.ModelGenerator/) via NuGet and enjoy!

