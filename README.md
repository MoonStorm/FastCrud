You hate verbatim SQL queries with zero type safety for your code but you love their speed? ``Dapper.FastCrud`` is a fast orm built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.

#### What to expect when working with Dapper.FastCrud in the DAL? 
Type safety, clean code, less prone to errors, more peace of mind, while still being close to the metal. Here's a sample:
```csharp
// Create paramters for the query
var queryParams = new 
{
    FirstName = "John",
    Street = "Creek Street"
};

// Get persons using the above created query parameters
var persons = dbConnection.Find<Person>(statement => statement
   .WithAlias("person")
   .Include<Address>(join =>
        join.InnerJoin()
            .WithAlias("address"))
   .Where($@"
        {nameof(Person.FirstName):of person} = {nameof(queryParams.FirstName):P} 
        AND {nameof(Address.Street):of address} = {nameof(queryParams.Street):P}")
   .OrderBy($"{nameof(Person.LastName):of person} DESC")  
   .Skip(10)
   .Top(20)
   .WithParameters(queryParams);
```

#### Features:
- Support for LocalDb, Ms Sql Server, MySql, SqLite, PostgreSql and SAP/Sybase SQL Anywhere.
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


#### Active Versions
- 3.2 [![Build Status](https://moonstorm.visualstudio.com/FastCrud/_apis/build/status/Release%20Branch%20Build%20Pipeline?repoName=MoonStorm%2FFastCrud&branchName=release)](https://moonstorm.visualstudio.com/FastCrud/_build/latest?definitionId=10&repoName=MoonStorm%2FFastCrud&branchName=release)
Check the [release notes](https://github.com/MoonStorm/FastCrud/wiki/Release-notes) for information about the recent changes or the readme info published on NuGet. 

#### WIKI
[The wiki](https://github.com/MoonStorm/FastCrud/wiki) is a great place for learning more about this library.


#### Speed
Let's have a look at some popular ORMs out there and benchmark their speed:  

- ``Dapper.SimpleCRUD v2.3.0``
- ``DapperExtensions v1.7.0 ``
- ``Entity Framework Core v7.0.3`` 

##### Automatic Benchmark Report (Last Run: Friday, May 31, 2024)

|  Library   |  Operation | Op Count |Time (ms) | Time/op (μs) |
|------------|------------|----------|----------|--------------|
| <a name="new_entry_marker"/> |
||||||
| Dapper | insert | 10000 | 1,333.28 | 133.33 |
| Fast Crud | insert | 10000 | 1,396.15 | 139.62 |
| Dapper Extensions | insert | 10000 | 1,513.42 | 151.34 |
| Simple Crud | insert | 10000 | 1,490.36 | 149.04 |
| Entity Framework - single op/call | insert | 10000 | 15,056.93 | 1,505.69 |
||||||
| Dapper | update | 10000 | 1,290.22 | 129.02 |
| Fast Crud | update | 10000 | 1,314.45 | 131.45 |
| Dapper Extensions | update | 10000 | 1,541.99 | 154.20 |
| Simple Crud | update | 10000 | 1,381.38 | 138.14 |
| Entity Framework - single op/call | update | 10000 | 27,445.40 | 2,744.54 |
||||||
| Dapper | delete | 10000 | 1,209.75 | 120.98 |
| Fast Crud | delete | 10000 | 1,234.92 | 123.49 |
| Dapper Extensions | delete | 10000 | 1,327.86 | 132.79 |
| Simple Crud | delete | 10000 | 1,306.30 | 130.63 |
| Entity Framework - single op/call | delete | 10000 | 2,097.67 | 209.77 |
||||||
| Dapper | select by id | 10000 | 569.29 | 56.93 |
| Fast Crud | select by id | 10000 | 570.54 | 56.35 |
| Dapper Extensions | select by id | 10000 | 1,941.33 | 194.13 |
| Simple Crud | select by id | 10000 | 600.45 | 60.04 |
| Entity Framework | select by id | 10000 | 902.95 | 90.30 |
||||||
| Dapper | select all | 10000 | 532.22 | 53.22 |
| Fast Crud | select all | 10000 | 574.99 | 57.50 |
| Dapper Extensions | select all | 10000 | 3,012.01 | 301.20 |
| Simple Crud | select all | 10000 | 611.34 | 61.13 |
| Entity Framework | select all | 10000 | 1,465.61 | 146.56 |

Dapper is included for reference. All the libs involved get their own internal cache cleared before each run, the benchmark database is re-created, data file gets pre-allocated, and the statistics are turned off.
The tests are following the same steps and are running in the same environment on the same number and size of records.

You can find more details about how to run the benchmarks yourself in the wiki.

##### Install [the main library](https://www.nuget.org/packages/Dapper.FastCrud/) and [the model generator](https://www.nuget.org/packages/Dapper.FastCrud.ModelGenerator/) via NuGet and enjoy!

