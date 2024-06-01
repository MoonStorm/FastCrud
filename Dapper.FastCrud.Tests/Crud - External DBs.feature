Feature: CRUD Tests - Externally Installed DBs

@ExternalDatabase
Scenario Outline: Insert and select all (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	And I query for the count of all the <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the inserted ones
	And the result of the last query count should be <entity count>
	Examples: 
	| database type | entity type | entity count | method type  |
	| SqlAnywhere   | employee    | 4            | asynchronous |
	| PostgreSql    | employee    | 4            | asynchronous |
	| MySql         | employee    | 3            | asynchronous |
	| MySql         | building    | 2            | asynchronous |
	| PostgreSql    | building    | 3            | asynchronous |
	| PostgreSql    | employee    | 4            | synchronous  |
	| MySql         | employee    | 3            | synchronous  |
	| MySql         | building    | 2            | synchronous  |
	| PostgreSql    | building    | 3            | synchronous  |
	| SqlAnywhere   | building    | 3            | synchronous  |

@ExternalDatabase
Scenario Outline: Count with a where clause (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I query for the count of all the inserted building entities using <method type> methods
	Then the result of the last query count should be <entity count>
	Examples: 
	| database type | entity count | method type  |
	| SqlAnywhere   | 4            | asynchronous |
	| PostgreSql    | 4            | asynchronous |
	| MySql         | 3            | asynchronous |
	| PostgreSql    | 4            | synchronous  |
	| MySql         | 3            | synchronous  |
	| SqlAnywhere   | 3            | synchronous  |

@ExternalDatabase
Scenario Outline: Find entities (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for a maximum of <max> <entity type> entities reverse ordered skipping <skip> records
	Then the queried <entity type> entities should be the same as the ones I inserted, in reverse order, starting from <skip> counting <max>
	Examples:
	| database type | entity type | entity count | max | skip | method type  |
	| SqlAnywhere   | workstation | 10           | 1   | 2    | asynchronous |
	| MySql         | workstation | 10           | 1   | 2    | asynchronous |
	| PostgreSql    | workstation | 10           | 1   | 2    | asynchronous |
	| MySql         | workstation | 10           | 1   | 2    | synchronous  |
	| SqlAnywhere   | workstation | 10           | 1   | 2    | synchronous  |
	| PostgreSql    | workstation | 10           | 1   | 2    | synchronous  |
	| MySql         | workstation | 10           | 1   |      | synchronous  |
	| PostgreSql    | workstation | 10           | 1   |      | synchronous  |
	| MySql         | workstation | 10           |     | 2    | synchronous  |
	| PostgreSql    | workstation | 10           |     | 2    | synchronous  |
	| MySql         | workstation | 10           |     |      | synchronous  |
	| PostgreSql    | workstation | 10           |     |      | synchronous  |
	| MySql         | workstation | 10           | 1   | 0    | synchronous  |
	| PostgreSql    | workstation | 10           | 1   | 0    | synchronous  |

@ExternalDatabase
Scenario Outline: Insert and select by primary key (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for the inserted <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the inserted ones
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 2            | asynchronous |
	| PostgreSql    | employee    | 2            | asynchronous |
	| SqlAnywhere   | employee    | 2            | asynchronous |
	| MySql         | workstation | 2            | asynchronous |
	| PostgreSql    | workstation | 2            | asynchronous |
	| MySql         | building    | 2            | asynchronous |
	| PostgreSql    | building    | 2            | asynchronous |
	| MySql         | employee    | 2            | synchronous  |
	| PostgreSql    | employee    | 2            | synchronous  |
	| MySql         | workstation | 2            | synchronous  |
	| PostgreSql    | workstation | 2            | synchronous  |
	| MySql         | building    | 2            | synchronous  |
	| PostgreSql    | building    | 2            | synchronous  |
	| SqlAnywhere   | building    | 2            | synchronous  |

@ExternalDatabase
Scenario Outline: Update by primary keys (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I update all the inserted <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the updated ones
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 2            | asynchronous |
	| SqlAnywhere   | employee    | 2            | asynchronous |
	| PostgreSql    | employee    | 2            | asynchronous |
	| MySql         | workstation | 2            | asynchronous |
	| PostgreSql    | workstation | 2            | asynchronous |
	| MySql         | building    | 2            | asynchronous |
	| PostgreSql    | building    | 2            | asynchronous |
	| MySql         | employee    | 2            | synchronous  |
	| PostgreSql    | employee    | 2            | synchronous  |
	| MySql         | workstation | 2            | synchronous  |
	| PostgreSql    | workstation | 2            | synchronous  |
	| MySql         | building    | 2            | synchronous  |
	| PostgreSql    | building    | 2            | synchronous  |
	| SqlAnywhere   | building    | 2            | synchronous  |

@ExternalDatabase
Scenario Outline: Partial update (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I partially update all the inserted <entity type> entities
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the updated ones
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 3            | asynchronous |
	| PostgreSql    | employee    | 3            | asynchronous |
	| MySql         | employee    | 3            | synchronous  |
	| PostgreSql    | employee    | 3            | synchronous  |
	| SqlAnywhere   | employee    | 3            | synchronous  |

@ExternalDatabase
Scenario Outline: Delete by primary keys (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I delete all the inserted <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the result of the last query count should be 0
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 3            | asynchronous |
	| PostgreSql    | employee    | 3            | asynchronous |
	| SqlAnywhere   | employee    | 3            | asynchronous |
	| MySql         | workstation | 3            | asynchronous |
	| MySql         | workstation | 3            | synchronous  |
	| PostgreSql    | workstation | 3            | asynchronous |
	| MySql         | building    | 3            | asynchronous |
	| MySql         | building    | 3            | synchronous  |
	| PostgreSql    | building    | 3            | asynchronous |
	| MySql         | employee    | 3            | synchronous  |
	| PostgreSql    | employee    | 3            | synchronous  |
	| PostgreSql    | workstation | 3            | synchronous  |
	| PostgreSql    | building    | 3            | synchronous  |
	| SqlAnywhere   | employee    | 3            | synchronous  |
	| SqlAnywhere   | workstation | 3            | synchronous  |
	| SqlAnywhere   | building    | 3            | synchronous  |

@ExternalDatabase
Scenario Outline:  Batch update (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I batch update all the inserted <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the updated ones
	Examples: 
	| database type | entity type | entity count | method type  |
	| PostgreSql    | employee    | 10           | synchronous  |
	| PostgreSql    | employee    | 10           | asynchronous |
	| SqlAnywhere   | employee    | 10           | synchronous  |
	| SqlAnywhere   | employee    | 10           | asynchronous |
	| MySql         | employee    | 10           | synchronous  |
	| MySql         | employee    | 10           | asynchronous |

@ExternalDatabase
Scenario Outline:  Batch delete (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I batch delete all the inserted <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the result of the last query count should be 0
	Examples: 
	| database type | entity type | entity count | method type  |
	| PostgreSql    | workstation | 10           | synchronous  |
	| PostgreSql    | workstation | 10           | asynchronous |
	| SqlAnywhere   | workstation | 10           | synchronous  |
	| SqlAnywhere   | workstation | 10           | asynchronous |
	| MySql         | workstation | 10           | synchronous  |
	| MySql         | workstation | 10           | asynchronous |
