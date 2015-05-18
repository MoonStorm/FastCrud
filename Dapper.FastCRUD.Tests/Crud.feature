Feature: Crud 

Scenario Outline: Insert and select all
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities
	And I query for all the employee entities
	Then the queried entities should be the same as the ones I inserted
	Examples: 
	| database type | entity count | 
	| LocalDb       | 1            |

Scenario Outline: Find
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities
	And I query for a maximum of <max> employee entities ordered by workstation id skipping <skip> records
	Then the queried entities should be the same as the ones I inserted, in reverse order, starting from <skip> counting <max>
	Examples: 
	| database type | entity count | max | skip |
	| LocalDb       | 100          | 10  | 20   |

Scenario Outline: Insert and select by composite primary key
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities
	And I query for the inserted employee entities
	Then the queried entities should be the same as the ones I inserted
	Examples: 
	| database type | entity count |
	| LocalDb       | 1            |

Scenario Outline: Update by composite keys
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities
	And I update all the inserted employee entities
	And I query for all the employee entities
	Then the queried entities should be the same as the ones I updated
	Examples: 
	| database type | entity count |
	| LocalDb       | 1            |

Scenario Outline: Delete by composite keys
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities
	And I delete all the inserted employee entities
	And I query for all the employee entities
	Then I should have 0 employee entities in the database
	Examples: 
	| database type | entity count |
	| LocalDb       | 1            |
