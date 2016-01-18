``Dapper.FastCrud`` is the fastest micro-orm extension for Dapper, built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.

You don't need to target .NET 4.6, but you do need to use a compiler equivalent to the one included inside Visual Studio 2015.

[![moonstorm MyGet Build Status](https://www.myget.org/BuildSource/Badge/moonstorm?identifier=669ece00-23a8-4f36-ad44-95822c66bee2)](https://www.myget.org/)

#### Features:
- Support for LocalDb, Ms Sql Server, MySql, SqLite, PostgreSql
- Entities having composite primary keys are supported
- Multiple entity mappings are supported, useful for partial queries in large denormalized tables and data migrations between different database types.
- All the CRUD methods accept a transaction, a command timeout, and a custom entity mapping.
- Fast pre-computed entity queries
- A useful SQL builder and statement formatter which can be used even if you don't need the CRUD features of this library.
- A generic T4 template for C# is also provided for convenience in the NuGet package Dapper.FastCrud.ModelGenerator.
Code first entities are also supported which can either be decorated with attributes, have their mappings programmatically set, or using your own custom convention.

#### WIKI
[The wiki](https://github.com/MoonStorm/Dapper.FastCRUD/wiki) is a great place for learning more about this library.


#### Speed
Most of us love Dapper for its speed. 
Let's have a look at how ``Fast Crud`` performs against other similar libraries out there (``Dapper.SimpleCRUD v1.9.1``, ``DapperExtensions v1.4.4 ``).

##### Automatic Benchmark Report (Last Run: Monday, November 23, 2015)
|  Library   |  Operation | Count |Time (ms) | Time/op (μs) |
|------------|------------|-------|----------|--------------|
||||||
| Dapper | insert | 30000 | 9,583.96 | 319.47 |
| Fast Crud | insert | 30000 | 10,944.79 | 364.83 |
| Dapper Extensions | insert | 30000 | 13,641.27 | 454.71 |
| Simple Crud | insert | 30000 | 19,165.25 | 638.84 |
||||||
| Dapper | update | 30000 | 6,439.43 | 214.65 |
| Fast Crud | update | 30000 | 6,668.78 | 222.29 |
| Dapper Extensions | update | 30000 | 8,803.26 | 293.44 |
| Simple Crud | update | 30000 | 10,204.28 | 340.14 |
||||||
| Dapper | delete | 30000 | 8,312.94 | 277.10 |
| Fast Crud | delete | 30000 | 8,693.02 | 289.77 |
| Dapper Extensions | delete | 30000 | 10,804.55 | 360.15 |
| Simple Crud | delete | 30000 | 13,642.98 | 454.77 |
||||||
| Dapper | select by id | 30000 | 6,010.58 | 200.35 |
| Fast Crud | select by id | 30000 | 6,172.08 | 205.74 |
| Dapper Extensions | select by id | 30000 | 6,355.57 | 211.85 |
| Simple Crud | select by id | 30000 | 12,912.19 | 430.41 |
||||||
| Dapper | select all | 3 | 217.06 | 72,353.87 |
| Fast Crud | select all | 3 | 262.41 | 87,468.83 |
| Dapper Extensions | select all | 3 | 291.22 | 97,073.00 |
| Simple Crud | select all | 3 | 814.65 | 271,549.57 |


Dapper is used as reference only, for the purpose of observing the overhead of the automatic CRUD features compared to a verbatim SQL construct. The database is re-created at every run, data file is pre-allocated, and the statistics are turned off.
External factors might influence the results, but they should be fairly consistent as the tests are following the same steps and are running on the same number and size of records. 

Environment details: Windows 7, i7 3930K @3.2GHz, 16GB DDR3-1600, SATA600 SSD

##### Install via NuGet and Enjoy !

