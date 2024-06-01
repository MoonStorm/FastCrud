Feature: CRUD Tests - In-Memory Databases

@AutomaticBuildServerTest
Scenario Outline: Insert and select all (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	And I query for the count of all the <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the inserted ones
	And the result of the last query count should be <entity count>
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 5            | asynchronous |
	| LocalDb       | workstation | 4            | asynchronous |
	| SqLite        | workstation | 6            | asynchronous |
	| LocalDb       | building    | 3            | asynchronous |
	| SqLite        | building    | 4            | asynchronous |
	| LocalDb       | employee    | 3            | synchronous  |
	| LocalDb       | workstation | 4            | synchronous  |
	| SqLite        | workstation | 2            | synchronous  |
	| LocalDb       | building    | 4            | synchronous  |
	| SqLite        | building    | 2            | synchronous  |

@AutomaticBuildServerTest
Scenario Outline: Count with a where clause (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I query for the count of all the inserted building entities using <method type> methods
	Then the result of the last query count should be <entity count>
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 6            | asynchronous |
	| LocalDb       | 6            | synchronous  |

@AutomaticBuildServerTest
Scenario Outline: Find entities (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for a maximum of <max> <entity type> entities reverse ordered skipping <skip> records
	Then the queried <entity type> entities should be the same as the ones I inserted, in reverse order, starting from <skip> counting <max>
	Examples: 
	| database type | entity type | entity count | max | skip | method type  |
	| LocalDb       | workstation | 10           | 1   | 2    | asynchronous |
	| SqLite        | workstation | 10           | 1   | 2    | asynchronous |
	| LocalDb       | workstation | 10           | 1   | 2    | synchronous  |
	| SqLite        | workstation | 10           | 1   | 2    | synchronous  |
	| LocalDb       | workstation | 10           | 1   |      | synchronous  |
	| SqLite        | workstation | 10           | 1   |      | synchronous  |
	| LocalDb       | workstation | 10           |     | 2    | synchronous  |
	| SqLite        | workstation | 10           |     | 2    | synchronous  |
	| LocalDb       | workstation | 10           |     |      | synchronous  |
	| SqLite        | workstation | 10           |     |      | synchronous  |
	| LocalDb       | workstation | 10           | 1   | 0    | synchronous  |
	| SqLite        | workstation | 10           | 1   | 0    | synchronous  |

@AutomaticBuildServerTest
Scenario Outline: Insert and select by primary key (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for the inserted <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the inserted ones
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 2            | asynchronous |
	| LocalDb       | workstation | 2            | asynchronous |
	| LocalDb       | building    | 2            | asynchronous |
	| SqLite        | building    | 2            | asynchronous |
	| LocalDb       | employee    | 2            | synchronous  |
	| LocalDb       | workstation | 2            | synchronous  |
	| LocalDb       | building    | 2            | synchronous  |
	| SqLite        | building    | 2            | synchronous  |

@AutomaticBuildServerTest
Scenario Outline: Update by primary keys (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I update all the inserted <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the updated ones
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 2            | asynchronous |
	| LocalDb       | workstation | 2            | asynchronous |
	| LocalDb       | building    | 2            | asynchronous |
	| LocalDb       | employee    | 2            | synchronous  |
	| LocalDb       | workstation | 2            | synchronous  |
	| SqLite        | building    | 2            | asynchronous |
	| LocalDb       | building    | 2            | synchronous  |
	| SqLite        | building    | 2            | synchronous  |

@AutomaticBuildServerTest
Scenario Outline: Partial update (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I partially update all the inserted <entity type> entities
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the updated ones
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 3            | asynchronous |
	| LocalDb       | employee    | 3            | synchronous  |

@AutomaticBuildServerTest
Scenario Outline: Delete by primary keys (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I delete all the inserted <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the result of the last query count should be 0
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 3            | asynchronous |
	| LocalDb       | workstation | 3            | asynchronous |
	| LocalDb       | building    | 3            | asynchronous |
	| SqLite        | building    | 3            | asynchronous |
	| SqLite        | building    | 3            | synchronous  |
	| LocalDb       | employee    | 3            | synchronous  |
	| LocalDb       | workstation | 3            | synchronous  |
	| LocalDb       | building    | 3            | synchronous  |

@AutomaticBuildServerTest
Scenario Outline:  Batch update (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I batch update all the inserted <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the queried <entity type> entities should be the same as the updated ones
	Examples: 
	| database type | entity type | entity count | skip | max | method type  |
	| LocalDb       | employee    | 10           | 3    | 2   | synchronous  |
	| LocalDb       | employee    | 10           | 3    | 2   | asynchronous |
	| SqLite        | workstation | 10           | 3    | 2   | synchronous  |
	| SqLite        | workstation | 10           | 3    | 2   | asynchronous |

@AutomaticBuildServerTest
Scenario Outline:  Batch delete (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I batch delete all the inserted <entity type> entities using <method type> methods
	And I query for all the inserted <entity type> entities using <method type> methods
	Then the result of the last query count should be 0
	Examples: 
	| database type | entity type | entity count | skip | max | method type  |
	| LocalDb       | workstation | 10           | 3    | 2   | synchronous  |
	| LocalDb       | workstation | 10           | 3    | 2   | asynchronous |
	| SqLite        | workstation | 10           | 3    | 2   | synchronous  |
	| SqLite        | workstation | 10           | 3    | 2   | asynchronous |
