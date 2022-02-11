``Dapper.FastCrud`` is the fastest micro-orm extension for Dapper, built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.
Visual Studio 2019 and above is recommended. 

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
  - Fluent validation for POCO objects
  - Semi-POCO using metadata objects
- Extensibility points are also provided.
- What to expect when working with Dapper.FastCrud in the DAL? Type safety, clean code, less mistakes, more peace of mind... ummm, here's a sample:
```
    var queryParams = new 
    {
        FirstName = "John",
        Street = "Creek Street"
    };

    var persons = dbConnection.Find<Person>(statement => statement
        .WithAlias("person")
        .InnerJoin<Person, Address>(join => join
                                            .FromAlias("person")
                                            .ToAlias("address")
                                            .MapResults())
        .Where($"{nameof(Person.FirstName):of person} = {nameof(queryParams.FirstName):P} AND {nameof(Address.Street):of address} = {nameof(queryParams.Street):P}")  
        .OrderBy($"{nameof(Person.LastName):of person} DESC")  
        .Skip(10)  
        .Top(20)  
        .WithParameters(queryParams);
```

#### Release Notes
- 3.0-preview [![Build Status](https://moonstorm.visualstudio.com/Dapper.FastCrud/_apis/build/status/Master%20Branch%20Build%20Pipeline?branchName=master)](https://moonstorm.visualstudio.com/Dapper.FastCrud/_build/latest?definitionId=8&branchName=master)
  - Main library:
    - Added support for .NET Standard 2.1
    - Extended support for the MetadataType attribute in .NET Standard 2.1
    - Bulk update can now be used with parameters.
    - Format specifier ":P" added for SQL parameters.
    - Format specifiers extended to support resolution via aliases in JOINs (e.g. "nameof(prop):of alias").
    - Methods adjusted for nullable support.
    - Added support for multiple references to the same target using the InverseProperty attribute.
    - [Breaking change] The fluent mapping setup has changed for setting up relationships.
    - Support for self referenced entities.
    - Support for multiple references to the same target.
    - Support for queries with JOINs that don't require the presence of a relationship set up in the mappings.
    - Extended the functionality of the Sql "formattables", exposed via the Sql static class, to allow for easy access to both the raw resolved names and their SQL ready counterparts.
    - [Breaking change] Clean separation for the formatter and the sql builder. As a result, the access to the formatter got moved out of the ISqlBuilder and into the Sql static class.
    - The GET method supports joins.
    - The main entity can now be aliased. It is now recommended to alias every entity in a JOIN for easy targeting in the WHERE clause.
    - The limit of 7 entities in a JOIN was removed.
    - A stable preview is available on NuGet (visible in VS when "Include preleleases" is checked in the Package Manager panel). 
  - Model generator (database first):
    - [Breaking change] Added support for self referenced entities.
    - [Breaking change] Added support for multiple references to the same target using the InverseProperty attribute.
    - [Breaking change] Better handling of columns representing reserved keywords in C#.
    - Support for new csproj style projects.
    - Fixed a problem preventing it from being used in VS2019 and later.
    - Pending NuGet publish.
  - Tests:
    - All the tests have been reviewed and most got refactored.
    - Workaround added for the badly implemented serialization context in Specflow.
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
Most of us love Dapper for its speed. 
Let's have a look at how ``Fast Crud`` performs against other similar libraries out there:  

- ``Dapper.SimpleCRUD v1.13.0``
- ``DapperExtensions v1.5.0 ``
- ``Entity Framework v6.1.3`` was included as well, even though it's in a different class of ORMs.
In an attempt to keep it on the same playing field, its proxy generation was turned off.

##### Automatic Benchmark Report (Last Run: Sunday, January 03, 2016)

|  Library   |  Operation | Op Count |Time (ms) | Time/op (μs) |
|------------|------------|----------|----------|--------------|
||||||
| Dapper | insert | 30000 | 10,016.88 | 333.90 |
| Fast Crud | insert | 30000 | 10,431.64 | 347.72 |
| Dapper Extensions | insert | 30000 | 13,272.40 | 442.41 |
| Simple Crud | insert | 30000 | 19,954.31 | 665.14 |
| Entity Framework | insert | 30000 | 43,636.47 | 1,454.55 |
||||||
| Dapper | update | 30000 | 6,439.43 | 214.65 |
| Fast Crud | update | 30000 | 6,668.78 | 222.29 |
| Dapper Extensions | update | 30000 | 8,803.26 | 293.44 |
| Simple Crud | update | 30000 | 10,204.28 | 340.14 |
| Entity Framework | update | 30000 | 39,954.88 | 1,331.83 |
||||||
| Dapper | delete | 30000 | 8,312.94 | 277.10 |
| Fast Crud | delete | 30000 | 8,693.02 | 289.77 |
| Dapper Extensions | delete | 30000 | 10,804.55 | 360.15 |
| Simple Crud | delete | 30000 | 13,642.98 | 454.77 |
| Entity Framework | delete | 30000 | 35,973.40 | 1,199.11 |
||||||
| Dapper | select by id | 30000 | 6,010.58 | 200.35 |
| Fast Crud | select by id | 30000 | 6,172.08 | 205.74 |
| Dapper Extensions | select by id | 30000 | 6,355.57 | 211.85 |
| Simple Crud | select by id | 30000 | 12,912.19 | 430.41 |
| Entity Framework | select by id | 30000 | 21,126.72 | 704.22 |
||||||
| Dapper | select all | 3 | 217.06 | 72,353.87 |
| Fast Crud | select all | 3 | 262.41 | 87,468.83 |
| Dapper Extensions | select all | 3 | 291.22 | 97,073.00 |
| Simple Crud | select all | 3 | 814.65 | 271,549.57 |
| Entity Framework | select all | 3 | 5,431.27 | 1,810,423.27 |

Dapper is used as reference only, for the purpose of observing the overhead of the automatic SQL generation compared to verbatim  constructs. The database is re-created at every run, data file is pre-allocated, and the statistics are turned off.
The tests are following the same steps and are running on the same number and size of records.

Environment details: Windows 7, i7 3930K @3.2GHz, 16GB DDR3-1600, SATA600 SSD  

You can find more details about how to run the benchmarks yourself in the wiki.

##### Install [the main library](https://www.nuget.org/packages/Dapper.FastCrud/) and [the model generator](https://www.nuget.org/packages/Dapper.FastCrud.ModelGenerator/) via NuGet and enjoy!

