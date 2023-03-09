You hate verbatim SQL queries with zero type safety for your code but you love the speed? ``Dapper.FastCrud`` is a fast orm built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.
Visual Studio 2019 and above is recommended. 

## What to expect when working with Dapper.FastCrud in the DAL? 
Type safety, clean code, less prone to errors, more peace of mind, while still being close to the metal. Here's a sample for 3.x:
```
    var queryParams = new 
    {
        FirstName = "John",
        Street = "Creek Street"
    };

    var persons = dbConnection.Find<Person>(statement => statement
        .WithAlias("person")
        .Include<Address>(join => join
            .InnerJoin()
            .WithAlias("address"))
        .Where($@"
            {nameof(Person.FirstName):of person} = {nameof(queryParams.FirstName):P} 
            AND {nameof(Address.Street):of address} = {nameof(queryParams.Street):P}")  
        .OrderBy($"{nameof(Person.LastName):of person} DESC")  
        .Skip(10)  
        .Top(20)  
        .WithParameters(queryParams);
```

## Features:
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

## Release Notes for 3.1
- Added support for SQL Anywhere
- Added support for 'rowversion' type columns for the SQL Server dialect.
- Dependencies, tests and benchmarks updated.

## Release Notes for 3.0
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
  - SQL statements no longer require the presence of a relationship preset in the mappings. You can join with whatever you want, using whatever navigation properties you want (or none) and with any ON clause you desire.
  - Added support for self referenced entities (via InverseProperty attribute / fluent mappings / directly in the query).
  - Added support for one-to-one relationships (via InverseProperty attribute / fluent mappings / directly in the query).
  - Added support for multiple references to the same target (via InverseProperty attribute / fluent mappings / directly in the query).

