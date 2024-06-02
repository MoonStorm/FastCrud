Feature: SQL Formatter

@AutomaticBuildServerTest
Scenario Outline: Simple query using the FastCrud formatter
	Then I should be able to construct a simple manual query for the dialect <database dialect> using the FastCrud SQL formatter
	Examples: 
	| database dialect |
	| MsSql            |
	| PostgreSql       |

@AutomaticBuildServerTest
Scenario Outline: Query with JOINs using the FastCrud formatter
	Then I should be able to construct a manual query involving multiple entities for the dialect <database dialect> using the FastCrud SQL formatter
	Examples: 
	| database dialect |
	| MsSql            |
	| PostgreSql       |

@AutomaticBuildServerTest
Scenario: Query with JOINs using the standard .NET formatter and specifiers
	Then I should be able to construct a manual query involving multiple entities using the standard formatter

@AutomaticBuildServerTest
Scenario Outline: Query using schema qualified entities
	Then I should be able to construct a manual query involving schema qualified code first entities for the dialect <database dialect> using the standard formatter
	Examples: 
	| database dialect |
	| MsSql            |
	| PostgreSql       |
