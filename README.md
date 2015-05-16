      Simple extensions added to the IDbConnection for convenience and a less error prone experience. This library is 2x faster than Dapper.SimpleCRUD.
      For Dapper constructs in general, it is recommended to use Visual Studio 2015 for features such as nameof and string interpolation but that's not a requirement.

      The package contains .NET 4.5 and 4.6 DLLs, one of which will be installed based on the target framework in your project.

      #### Examples of usage:
      - dbConnection.Insert(newEntity);
      - dbConnection.Get()
      - dbConnection.Get(new Entity() {Id = 10});
      - dbConnection.Update(updatedEntity);
      - dbConnection.Delete(entity)
      - dbConnection.Find<Entity>(
              whereClause:$"{nameof(Entity.FirstName}=@FirstNameParam", 
              orderClause:$"{nameof(Entity.LastName)} DESC", 
              queryParameters: new {FirstNameParam: "John"});
        This is where the power of the C# 6 compiler comes into play, and leaves no chance to mistypings or to problems arising from db entity refactorings.
      - dbConnection.GetTableName<Entity>();

      #### Features:
      - Support for LocalDb ans Ms Sql Server (support for more databases will come soon)
      - Entities having composite primary keys are supported
      - All the CRUD methods accept a transaction and a command timeout
      - Fast pre-computed entity mappings
      - A generic T4 template is also provided for convenience. Isolated entity generation via separate template configurations.
        Existing POCO entities are also supported which can be decorated with attributes such as Table, Key and DatabaseGenerated.
		Column name overrides are not supported and not recommended. As you'll end up writing more complex SQL queries, outside of the domain of this library, you'll want to use the nameof operator as much as possible.

	  ##### Install via NuGet and Enjoy !

