Simple extensions added to the IDbConnection for convenience and a less error prone experience. This library is 2x faster than Dapper.SimpleCRUD.
For Dapper constructs in general, it is recommended to use Visual Studio 2015 for features such as nameof and string interpolation but that's not a requirement.

The package contains .NET 4.5 and 4.6 DLLs, one of which will be installed based on the target framework in your project. 
For .NET 4.5, the code contains the polyfills for the missing FormattableString class, which is required when targetting that framework version and using string interpolation with the C# 6 compiler in VS 2015.

#### Examples of usage:
- dbConnection.Insert(newEntity);
- dbConnection.Get()
- dbConnection.Get(new Entity() {Id = 10});
- dbConnection.Update(updatedEntity);
- dbConnection.Delete(entity)
- dbConnection.GetTableName<Entity>();
- dbConnection.Find<Entity>(
        whereClause:$"{nameof(Entity.FirstName}=@FirstNameParam", 
        orderClause:$"{nameof(Entity.LastName)} DESC", 
        queryParameters: new {FirstNameParam: "John"});

This is where the power of the C# 6 compiler comes into play, and leaves no chance to mistypings or to problems arising from db entity refactorings.

#### Features:
- Support for LocalDb ans Ms Sql Server (support for more databases will come soon)
- Entities having composite primary keys are supported
- All the CRUD methods accept a transaction and a command timeout
- Fast pre-computed entity mappings
- A generic T4 template is also provided for convenience. Isolated entity generation via separate template configurations. Existing POCO entities are also supported which can be decorated with attributes such as Table, Key and DatabaseGenerated. Column name overrides are not supported and not recommended. As you'll end up writing more complex SQL queries, outside of the domain of this library, you'll want to use the nameof operator as much as possible.

#### Automatic Benchmark Report (Last Run: Friday, May 15, 2015 10:47:09 PM)
| Micro ORM |  Operation | Entity Count |Time (ms) |
------------|------------|--------------|-----------
Simple Crud | update | 20000 | 11,994.01 
Fast Crud | update | 20000 | 7,430.18 
Dapper | update | 20000 | 7,355.58 
Simple Crud | select by id | 20000 | 8,286.10 
Fast Crud | select by id | 20000 | 3,924.62 
Dapper | select by id | 20000 | 3,854.52 
Simple Crud | delete | 20000 | 8,835.07 
Fast Crud | delete | 20000 | 6,562.12 
Dapper | delete | 20000 | 6,609.66 
Simple Crud | insert | 20000 | 13,701.60 
Fast Crud | insert | 20000 | 7,673.79 
Dapper | insert | 20000 | 8,069.94 
Simple Crud | insert | 10 | 74.44 
Fast Crud | insert | 10 | 15.20 
Dapper | insert | 10 | 30.74 
Simple Crud | batch select - no filter | 20000 | 706.35 
Fast Crud | batch select - no filter | 20000 | 68.87 
Dapper | batch select - no filter | 20000 | 50.20 
Simple Crud | Insert | 20000 | 10,090.01 
Fast Crud | Insert | 20000 | 6,515.31 
Dapper | Insert | 20000 | 5,720.08 
Simple Crud | Insert | 10 | 648.31 
Fast Crud | Insert | 10 | 44.62 
Dapper | Insert | 10 | 186.58 

Considering the features that the libray has added, Dapper is used as reference only. The database is dropped and restored every run. 
Database growth and other factors might influence the results, but they should be fairly consistent as the tests are running on the same number and size of records.

##### Install via NuGet and Enjoy !

