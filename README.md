``Dapper.FastCrud`` is the fastest micro-orm extension for Dapper. It is built around essential features of the C# 6 that has finally raised the simplicity of raw SQL constructs to acceptable maintenance levels. These features leave no chance to mistypings or problems arising from db entity refactorings.

[![moonstorm MyGet Build Status](https://www.myget.org/BuildSource/Badge/moonstorm?identifier=669ece00-23a8-4f36-ad44-95822c66bee2)](https://www.myget.org/)

#### Features:
- Support for LocalDb, Ms Sql Server, MySql, SqLite, PostgreSql
- Entities having composite primary keys are supported
- Multiple entity mappings are supported, useful for partial queries in large denormalized tables and data migrations between different database types.
- All the CRUD methods accept a transaction, a command timeout, and a custom entity mapping.
- Fast pre-computed entity queries
- A simple Sql builder with alias support is provided, which is very useful when manual SQL queries are unavoidable.
- A generic T4 template is also provided for convenience in the NuGet package ``Dapper.FastCrud.ModelGenerator``. Entity domain partitioning and generation can be achieved via separate template configurations. 
Code first entities are also supported which can either be decorated with attributes such as Table, Key and DatabaseGenerated, or can have their mappings programmatically set.

#### WIKI
[The wiki](https://github.com/MoonStorm/Dapper.FastCRUD/wiki) is a great place for learning more about this library.


#### Speed
Most of us love Dapper for its speed. 
Let's have a look at how ``Fast Crud`` performs against other similar libraries out there (``Dapper.SimpleCRUD v1.9.1``, ``DapperExtensions v1.4.4 ``).

##### Automatic Benchmark Report (Last Run: Wednesday, October 21, 2015)
|  Library   |  Operation | Count |Time (ms) | Time/op (μs) |
|------------|------------|-------|----------|--------------|
||||||
| Dapper | insert | 30000 | 9,673.86 | 322.46 |
| Fast Crud | insert | 30000 | 9,872.64 | 329.09 |
| Dapper Extensions | insert | 30000 | 12,274.04 | 409.13 |
| Simple Crud | insert | 30000 | 18,435.22 | 614.51 |
||||||
| Dapper | update | 30000 | 9,669.60 | 322.32 |
| Fast Crud | update | 30000 | 9,826.25 | 327.54 |
| Dapper Extensions | update | 30000 | 14,889.08 | 496.30 |
| Simple Crud | update | 30000 | 18,865.06 | 628.84 |
||||||
| Dapper | delete | 30000 | 7,343.11 | 244.77  |
| Fast Crud | delete | 30000 | 7,553.49 | 251.78  |
| Dapper Extensions | delete | 30000 | 10,001.75 | 333.39  |
| Simple Crud | delete | 30000 | 13,295.85 | 443.20  |
||||||
| Dapper | select by id | 30000 | 5,577.01 | 185.90  |
| Fast Crud | select by id | 30000 | 5,931.15 | 197.70  |
| Dapper Extensions | select by id | 30000 | 8,061.54 | 268.72  |
| Simple Crud | select by id | 30000 | 11,682.05 | 389.40  |
||||||
| Dapper | select all | 3 | 279.12 | 93,040.73  |
| Fast Crud | select all | 3 | 286.60 | 95,533.40  |
| Dapper Extensions | select all | 3 | 291.21 | 97,070.13  |
| Simple Crud | select all | 3 | 818.81 | 272,935.37  |


Dapper is used as reference only, for the purpose of observing the overhead of the automatic CRUD features compared to a verbatim SQL construct. The database is re-created at every run, data file is pre-allocated, and the statistics are turned off.
External factors might influence the results, but they should be fairly consistent as the tests are following the same steps and are running on the same number and size of records. 

Environment details: Windows 7, i7 3930K @3.2GHz, 16GB DDR3-1600, SATA600 SSD

##### Install via NuGet and Enjoy !

