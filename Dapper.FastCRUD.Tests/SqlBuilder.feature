Feature: SQL Builder

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

