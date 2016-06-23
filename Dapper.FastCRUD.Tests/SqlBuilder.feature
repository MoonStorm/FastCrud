Feature: SQL Builder

@InMemoryDatabase
Scenario Outline: Select all columns
	Given I extract the SQL builder for <database type> and <entity type>
	When I construct the select column enumeration
	Then I should get a valid select column enumeration
	Examples: 
	| database type | entity type |
	| LocalDb       | workstation    |
	| PostgreSql    | workstation    |
	| MySql         | workstation    |
	| SqLite        | workstation    |

@InMemoryDatabase
Scenario Outline: Manual query
	Given I extract the SQL builder for <database type> and <entity type>
	When I construct a complex join query for <entity type> using <sql query builder method>
	Then I should get a valid join query statement for <entity type>
	Examples: 
	| database type | entity type | sql query builder method                       |
	| LocalDb       | workstation | individual identifier resolvers                |
	| PostgreSql    | workstation | individual identifier resolvers                |
	| MySql         | workstation | individual identifier resolvers                |
	| SqLite        | workstation | individual identifier resolvers                |
	| LocalDb       | workstation | combined table and column identifier resolvers |
	| PostgreSql    | workstation | combined table and column identifier resolvers |
	| MySql         | workstation | combined table and column identifier resolvers |
	| SqLite        | workstation | combined table and column identifier resolvers |

