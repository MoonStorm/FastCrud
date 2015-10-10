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
Without taking this away from you, Dapper.FastCRUD helps by giving you type safety and can even be used as foundation for  your DAL repositories.
Let's have a look at how it performs against other similar libraries out there (``Dapper.SimpleCRUD v1.8.7``, ``DapperExtensions v1.4.4 ``).

##### Automatic Benchmark Report (Last Run: Thursday, May 28, 2015)
|           |  Operation | Count |Time (ms) | Time/op (μs) |
------------|------------|-------|----------|--------------
Dapper | update | 20000 | 4,219.74 | 210.99  
Fast Crud | update | 20000 | 4,254.11 | 212.71  
Dapper Extensions | update | 20000 | 5,673.44 | 283.67  
Simple Crud | update | 20000 | 7,250.60 | 362.53  
Dapper | select by id | 20000 | 2,120.96 | 106.05  
Fast Crud | select by id | 20000 | 2,502.21 | 125.11  
Dapper Extensions | select by id | 20000 | 3,558.76 | 177.94  
Simple Crud | select by id | 20000 | 5,893.90 | 294.70  
Dapper | delete | 20000 | 3,824.91 | 191.25  
Fast Crud | delete | 20000 | 3,717.07 | 185.85  
Dapper Extensions | delete | 20000 | 4,622.52 | 231.13  
Simple Crud | delete | 20000 | 5,953.60 | 297.68  
Dapper | insert | 20000 | 4,352.63 | 217.63  
Fast Crud | insert | 20000 | 4,877.35 | 243.87  
Dapper Extensions | insert | 20000 | 6,106.09 | 305.30  
Simple Crud | insert | 20000 | 9,029.64 | 451.48  
Dapper | select all | 3 | 134.95 | 44,984.57  
Fast Crud | select all | 3 | 185.02 | 61,672.73  
Dapper Extensions | select all | 3 | 203.24 | 67,745.60  
Simple Crud | select all | 3 | 875.43 | 291,808.73  


Dapper is used as reference only, for the purpose of observing the overhead of the automatic CRUD features compared to a verbatim SQL construct. The database is re-created at every run, data file is pre-allocated, and the statistics are turned off.
External factors might influence the results, but they should be fairly consistent as the tests are following the same steps and are running on the same number and size of records. 
Environment details: Windows 7, i7 3930K @3.2GHz, 16GB DDR3-1600, SATA600 SSD

##### Install via NuGet and Enjoy !

