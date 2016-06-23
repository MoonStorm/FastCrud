Feature: Relationships
	Tests for the relationship between the entities Workstation -> Employee (single relationship) and Building -> Workstation -> Employee (two level relationship)

@InMemoryDatabase
Scenario Outline: Single relationship parents with children (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the workstation entities combined with the employee entities using <method type> methods
	Then the queried workstation entities should be the same as the local ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@InMemoryDatabase
Scenario Outline:  Single relationship children with parents (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the employee entities combined with the workstation entities using <method type> methods
	Then the queried employee entities should be the same as the local ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@InMemoryDatabase
Scenario Outline: Single relationship children with no parents (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities using <method type> methods
	And I query for all the employee entities combined with the workstation entities using <method type> methods
	Then the queried employee entities should be the same as the local ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@InMemoryDatabase
Scenario Outline: Single relationship parents with no children (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I query for all the workstation entities combined with the employee entities using <method type> methods
	Then the queried workstation entities should be the same as the local ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@InMemoryDatabase
Scenario Outline: Two level relationship grandparents with parents and children (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I insert <entity count> workstation entities parented to existing building entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the building entities combined with workstation and employee entities using <method type> methods
	Then the queried building entities should be the same as the local ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@InMemoryDatabase
Scenario Outline: Two level relationship children with parents and grandparents (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I insert <entity count> workstation entities parented to existing building entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the employee entities combined with workstation and building entities using <method type> methods
	Then the queried employee entities should be the same as the local ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@InMemoryDatabase
Scenario Outline: Two level relationship children with no parents or grandparents (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities using <method type> methods
	And I query for all the employee entities combined with workstation and building entities using <method type> methods
	Then the queried employee entities should be the same as the local ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |