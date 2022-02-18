You hate verbatim SQL queries with zero type safety for your code but you love the speed? ``Dapper.FastCrud`` is a fast orm built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.
Visual Studio 2019 and above is recommended. 

## What to expect when working with Dapper.FastCrud in the DAL? 
Type safety, clean code, less prone to errors, more peace of mind, while still being close to the metal. Here's a sample for 3.0:
```
    var queryParams = new 
    {
        FirstName = "John",
        Street = "Creek Street"
    };

    var persons = dbConnection.Find<Person>(statement => statement
        .WithAlias("person")
        .Include<Address>(join => join.InnerJoin()
                                      .WithAlias("address"))
        .Where($"{nameof(Person.FirstName):of person} = {nameof(queryParams.FirstName):P} AND {nameof(Address.Street):of address} = {nameof(queryParams.Street):P}")  
        .OrderBy($"{nameof(Person.LastName):of person} DESC")  
        .Skip(10)  
        .Top(20)  
        .WithParameters(queryParams);
```

## Features:
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
