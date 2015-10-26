``Dapper.FastCrud`` is the fastest micro-orm extension for Dapper, built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.

You don't need to target .NET 4.6 to get these goodies, but you do need to use a compiler equivallent to the one included  inside Visual Studio 2015.

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

##### Automatic Benchmark Report (Last Run: Monday, October 26, 2015)
|  Library   |  Operation | Count |Time (ms) | Time/op (μs) |
|------------|------------|-------|----------|--------------|
||||||
| Dapper | insert | 30000 | 8,285.60 | 276.19 |
| Fast Crud | insert | 30000 | 8,379.56 | 279.32 |
| Dapper Extensions | insert | 30000 | 11,525.15 | 384.17 |
| Simple Crud | insert | 30000 | 17,451.21 | 581.71 |
||||||
| Dapper | update | 30000 | 8,033.25 | 267.78 |
| Fast Crud | update | 30000 | 8,712.39 | 290.41 |
| Dapper Extensions | update | 30000 | 11,774.73 | 392.49 |
| Simple Crud | update | 30000 | 16,401.51 | 546.72 |
||||||
| Dapper | delete | 30000 | 5,319.36 | 177.31 |
| Fast Crud | delete | 30000 | 5,594.43 | 186.48 |
| Dapper Extensions | delete | 30000 | 7,545.36 | 251.51 |
| Simple Crud | delete | 30000 | 9,097.77 | 303.26 |
||||||
| Dapper | select by id | 30000 | 4,483.15 | 149.44 |
| Fast Crud | select by id | 30000 | 4,502.82 | 150.09 |
| Dapper Extensions | select by id | 30000 | 6,600.46 | 220.02 |
| Simple Crud | select by id | 30000 | 9,429.53 | 314.32 |
||||||
| Dapper | select all | 3 | 270.87 | 90,289.93 |
| Fast Crud | select all | 3 | 325.98 | 108,658.40 |
| Dapper Extensions | select all | 3 | 519.32 | 173,105.03 |
| Simple Crud | select all | 3 | 1,102.79 | 367,596.37 |


Dapper is used as reference only, for the purpose of observing the overhead of the automatic CRUD features compared to a verbatim SQL construct. The database is re-created at every run, data file is pre-allocated, and the statistics are turned off.
External factors might influence the results, but they should be fairly consistent as the tests are following the same steps and are running on the same number and size of records. 

Environment details: Windows 7, i7 3930K @3.2GHz, 16GB DDR3-1600, SATA600 SSD

##### Install via NuGet and Enjoy !

