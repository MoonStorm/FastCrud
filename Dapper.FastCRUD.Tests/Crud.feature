Feature: CRUD tests

Scenario Outline: Insert and select all (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	And I query for the count of all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the ones I inserted
	And the database count of the queried entities should be <entity count>
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 1            | asynchronous |
	| SqLite        | workstation | 1            | asynchronous |
	| LocalDb       | building    | 1            | asynchronous |
	| SqLite        | building    | 1            | asynchronous |
	| LocalDb       | employee    | 1            | synchronous |
	| SqLite        | workstation | 1            | synchronous |
	| LocalDb       | building    | 1            | synchronous |
	| SqLite        | building    | 1            | synchronous |

@ExternalDatabase
Scenario Outline: Insert and select all (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	And I query for the count of all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the ones I inserted
	And the database count of the queried entities should be <entity count>
	Examples: 
	| database type | entity type | entity count | method type  |
	| PostgreSql    | employee    | 1            | asynchronous |
	| MySql         | employee    | 1            | asynchronous |
	| MySql         | building    | 1            | asynchronous |
	| PostgreSql    | building    | 1            | asynchronous |
	| PostgreSql    | employee    | 1            | synchronous |
	| MySql         | employee    | 1            | synchronous |
	| MySql         | building    | 1            | synchronous |
	| PostgreSql    | building    | 1            | synchronous |


Scenario Outline: Find (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for a maximum of <max> <entity type> entities reverse ordered skipping <skip> records
	Then the queried entities should be the same as the ones I inserted, in reverse order, starting from <skip> counting <max>
	Examples: 
	| database type | entity type | entity count | max | skip | method type  |
	| LocalDb       | workstation | 100          | 10  | 20   | asynchronous |
	| SqLite        | workstation | 100          | 10  | 20   | asynchronous |
	| LocalDb       | workstation | 100          | 10  | 20   | synchronous |
	| SqLite        | workstation | 100          | 10  | 20   | synchronous |

@ExternalDatabase
Scenario Outline: Find (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for a maximum of <max> <entity type> entities reverse ordered skipping <skip> records
	Then the queried entities should be the same as the ones I inserted, in reverse order, starting from <skip> counting <max>
	Examples:
	| database type | entity type | entity count | max | skip | method type  |
	| MySql         | workstation | 100          | 10  | 20   | asynchronous |
	| PostgreSql    | workstation | 100          | 10  | 20   | asynchronous |
	| MySql         | workstation | 100          | 10  | 20   | synchronous |
	| PostgreSql    | workstation | 100          | 10  | 20   | synchronous |


Scenario Outline: Insert and select by primary key (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for the inserted <entity type> entities using <method type> methods
	Then the queried entities should be the same as the ones I inserted
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 1            |asynchronous |
	| LocalDb       | workstation | 1            |asynchronous |
	| LocalDb       | building    | 1            |asynchronous |
	| SqLite        | building    | 1            |asynchronous |
	| LocalDb       | employee    | 1            |synchronous |
	| LocalDb       | workstation | 1            |synchronous |
	| LocalDb       | building    | 1            |synchronous |
	| SqLite        | building    | 1            |synchronous |

@ExternalDatabase
Scenario Outline: Insert and select by primary key (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for the inserted <entity type> entities using <method type> methods
	Then the queried entities should be the same as the ones I inserted
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 1            |asynchronous |
	| PostgreSql    | employee    | 1            |asynchronous |
	| MySql         | workstation | 1            |asynchronous |
	| PostgreSql    | workstation | 1            |asynchronous |
	| MySql         | building    | 1            |asynchronous |
	| PostgreSql    | building    | 1            |asynchronous |
	| MySql         | employee    | 1            |synchronous |
	| PostgreSql    | employee    | 1            |synchronous |
	| MySql         | workstation | 1            |synchronous |
	| PostgreSql    | workstation | 1            |synchronous |
	| MySql         | building    | 1            |synchronous |
	| PostgreSql    | building    | 1            |synchronous |


Scenario Outline: Update by primary keys (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I update all the inserted <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the ones I updated
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 1            |asynchronous |
	| LocalDb       | workstation | 1            |asynchronous |
	| LocalDb       | building    | 1            |asynchronous |
	| LocalDb       | employee    | 1            |synchronous |
	| LocalDb       | workstation | 1            |synchronous |
	| SqLite        | building    | 1            |asynchronous |
	| LocalDb       | building    | 1            |synchronous |
	| SqLite        | building    | 1            |synchronous |

@ExternalDatabase
Scenario Outline: Update by primary keys (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I update all the inserted <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the ones I updated
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 1            |asynchronous |
	| PostgreSql    | employee    | 1            |asynchronous |
	| MySql         | workstation | 1            |asynchronous |
	| PostgreSql    | workstation | 1            |asynchronous |
	| MySql         | building    | 1            |asynchronous |
	| PostgreSql    | building    | 1            |asynchronous |
	| MySql         | employee    | 1            |synchronous |
	| PostgreSql    | employee    | 1            |synchronous |
	| MySql         | workstation | 1            |synchronous |
	| PostgreSql    | workstation | 1            |synchronous |
	| MySql         | building    | 1            |synchronous |
	| PostgreSql    | building    | 1            |synchronous |


Scenario Outline: Partial update (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I partially update all the inserted <entity type> entities
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the ones I updated
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 1            |asynchronous |
	| LocalDb       | employee    | 1            |synchronous |

@ExternalDatabase
Scenario Outline: Partial update (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I partially update all the inserted <entity type> entities
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the ones I updated
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 1            |asynchronous |
	| PostgreSql    | employee    | 1            |asynchronous |
	| MySql         | employee    | 1            |synchronous |
	| PostgreSql    | employee    | 1            |synchronous |


Scenario Outline: Delete by primary keys (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I clear all the inserted entities
	And I insert <entity count> <entity type> entities using <method type> methods
	And I delete all the inserted <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then I should have <entity count> <entity type> entities in the database
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 1            |asynchronous |
	| LocalDb       | workstation | 1            |asynchronous |
	| LocalDb       | building    | 1            |asynchronous |
	| SqLite        | building    | 1            |asynchronous |
	| SqLite        | building    | 1            |synchronous |
	| LocalDb       | employee    | 1            |synchronous |
	| LocalDb       | workstation | 1            |synchronous |
	| LocalDb       | building    | 1            |synchronous |

@ExternalDatabase
Scenario Outline: Delete by primary keys (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I clear all the inserted entities
	And I insert <entity count> <entity type> entities using <method type> methods
	And I delete all the inserted <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then I should have <entity count> <entity type> entities in the database
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 1            |asynchronous |
	| PostgreSql    | employee    | 1            |asynchronous |
	| MySql         | workstation | 1            |asynchronous |
	| MySql         | workstation | 1            |synchronous |
	| PostgreSql    | workstation | 1            |asynchronous |
	| MySql         | building    | 1            |asynchronous |
	| MySql         | building    | 1            |synchronous |
	| PostgreSql    | building    | 1            |asynchronous |
	| MySql         | employee    | 1            |synchronous |
	| PostgreSql    | employee    | 1            |synchronous |
	| PostgreSql    | workstation | 1            |synchronous |
	| PostgreSql    | building    | 1            |synchronous |
