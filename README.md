``Dapper.FastCrud`` is the fastest micro-orm extension for Dapper, built around essential features of the C# 6 / VB 14 that have finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.

When using this library, a compiler equivalent to the one included inside Visual Studio 2015 is required. 

#### Features:
- Support for LocalDb, Ms Sql Server, MySql, SqLite, PostgreSql
- Entities having composite primary keys are supported
- Multiple entity mappings are supported, useful for partial queries in large denormalized tables and data migrations between different database types.
- All the CRUD methods accept a transaction, a command timeout, and a custom entity mapping.
- Fast pre-computed entity queries
- Compatible with component model data annotations.
- Opt-in relationships.
- A useful SQL builder and statement formatter which can be used even if you don't need the CRUD features of this library.
- A generic T4 template for C# is also provided for convenience in the NuGet package Dapper.FastCrud.ModelGenerator.
Code first entities are also supported which can either be decorated with attributes, have their mappings programmatically set, or using your own custom convention.

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

##### Install via NuGet and Enjoy !

