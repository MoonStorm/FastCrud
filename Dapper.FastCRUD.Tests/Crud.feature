Feature: CRUD tests

Scenario Outline: Insert and select all
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities
	And I query for all the <entity type> entities
	Then the queried entities should be the same as the ones I inserted
	Examples: 
	| database type | entity type | entity count |
	| LocalDb       | employee    | 1            |
	| PostgreSql    | employee    | 1            |
	| MySql         | employee    | 1            |
	| SqLite        | workstation | 1            |
	| LocalDb       | building    | 1            |
	| MySql         | building    | 1            |
	| PostgreSql    | building    | 1            |
	| SqLite        | building    | 1            |

Scenario Outline: Find
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities
	And I query for a maximum of <max> <entity type> entities ordered by workstation id skipping <skip> records
	Then the queried entities should be the same as the ones I inserted, in reverse order, starting from <skip> counting <max>
	Examples: 
	| database type | entity type | entity count | max | skip |
	| LocalDb       | employee    | 100          | 10  | 20   |
	| MySql         | employee    | 100          | 10  | 20   |
	| PostgreSql    | employee    | 100          | 10  | 20   |
	| SqLite        | workstation | 100          | 10  | 20   |

Scenario Outline: Insert and select by primary key
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities
	And I query for the inserted <entity type> entities
	Then the queried entities should be the same as the ones I inserted
	Examples: 
	| database type | entity type | entity count |
	| LocalDb       | employee    | 1            |
	| MySql         | employee    | 1            |
	| PostgreSql    | employee    | 1            |
	| LocalDb       | workstation | 1            |
	| MySql         | workstation | 1            |
	| PostgreSql    | workstation | 1            |
	| LocalDb       | building    | 1            |
	| MySql         | building    | 1            |
	| SqLite        | building    | 1            |
	| PostgreSql    | building    | 1            |

Scenario Outline: Update by primary keys
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities
	And I update all the inserted <entity type> entities
	And I query for all the <entity type> entities
	Then the queried entities should be the same as the ones I updated
	Examples: 
	| database type | entity type | entity count |
	| LocalDb       | employee    | 1            |
	| MySql         | employee    | 1            |
	| PostgreSql    | employee    | 1            |
	| LocalDb       | workstation | 1            |
	| MySql         | workstation | 1            |
	| PostgreSql    | workstation | 1            |
	| LocalDb       | building    | 1            |
	| MySql         | building    | 1            |
	| SqLite        | building    | 1            |
	| PostgreSql    | building    | 1            |

Scenario Outline: Partial update
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities
	And I partially update all the inserted <entity type> entities
	And I query for all the <entity type> entities
	Then the queried entities should be the same as the ones I updated
	Examples: 
	| database type | entity type | entity count |
	| LocalDb       | employee    | 1            |
	| MySql         | employee    | 1            |
	| PostgreSql    | employee    | 1            |

Scenario Outline: Delete by primary keys
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities
	And I clear all the inserted entities
	And I insert <entity count> <entity type> entities
	And I delete all the inserted <entity type> entities
	And I query for all the <entity type> entities
	Then I should have <entity count> <entity type> entities in the database
	Examples: 
	| database type | entity type | entity count |
	| LocalDb       | employee    | 1            |
	| MySql         | employee    | 1            |
	| PostgreSql    | employee    | 1            |
	| LocalDb       | workstation | 1            |
	| MySql         | workstation | 1            |
	| PostgreSql    | workstation | 1            |
	| LocalDb       | building    | 1            |
	| MySql         | building    | 1            |
	| SqLite        | building    | 1            |
	| PostgreSql    | building    | 1            |
