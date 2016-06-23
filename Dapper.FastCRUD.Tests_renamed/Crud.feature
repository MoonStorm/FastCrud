Feature: CRUD tests

@InMemoryDatabase
Scenario Outline:  Batch update (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I batch update a maximum of <max> <entity type> entities skipping <skip> and using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples: 
	| database type | entity type | entity count | skip | max | method type  |
	| LocalDb       | employee    | 10           | 3    | 2   | synchronous  |
	| LocalDb       | employee    | 10           | 3    | 2   | asynchronous |
	| SqLite        | workstation | 10           | 3    | 2   | synchronous  |
	| SqLite        | workstation | 10           | 3    | 2   | asynchronous |

@ExternalDatabase
Scenario Outline:  Batch update (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I batch update a maximum of <max> <entity type> entities skipping <skip> and using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples: 
	| database type | entity type | entity count | skip | max | method type  |
	| PostgreSql    | employee    | 10           | 3    | 2   | synchronous  |
	| PostgreSql    | employee    | 10           | 3    | 2   | asynchronous |
	| MySql         | employee    | 10           | 3    | 2   | synchronous  |
	| MySql         | employee    | 10           | 3    | 2   | asynchronous |

@InMemoryDatabase
Scenario Outline:  Batch delete (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I batch delete a maximum of <max> <entity type> entities skipping <skip> and using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples: 
	| database type | entity type | entity count | skip | max | method type  |
	| LocalDb       | workstation    | 10           | 3    | 2   | synchronous  |
	| LocalDb       | workstation    | 10           | 3    | 2   | asynchronous |
	| SqLite        | workstation | 10           | 3    | 2   | synchronous  |
	| SqLite        | workstation | 10           | 3    | 2   | asynchronous |

@ExternalDatabase
Scenario Outline:  Batch delete (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I batch delete a maximum of <max> <entity type> entities skipping <skip> and using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples: 
	| database type | entity type | entity count | skip | max | method type  |
	| PostgreSql    | workstation    | 10           | 3    | 2   | synchronous  |
	| PostgreSql    | workstation    | 10           | 3    | 2   | asynchronous |
	| MySql         | workstation    | 10           | 3    | 2   | synchronous  |
	| MySql         | workstation    | 10           | 3    | 2   | asynchronous |

@InMemoryDatabase
Scenario Outline: Insert and select all (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	And I query for the count of all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	And the database count of the queried entities should be <entity count>
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 1            | asynchronous |
	| LocalDb       | workstation | 1            | asynchronous |
	| SqLite        | workstation | 1            | asynchronous |
	| LocalDb       | building    | 1            | asynchronous |
	| SqLite        | building    | 1            | asynchronous |
	| LocalDb       | employee    | 1            | synchronous |
	| LocalDb       | workstation | 1            | synchronous |
	| SqLite        | workstation | 1            | synchronous |
	| LocalDb       | building    | 1            | synchronous |
	| SqLite        | building    | 1            | synchronous |

@InMemoryDatabase
Scenario Outline: Conditional count (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I query for the count of all the inserted building entities using <method type> methods
	Then the database count of the queried entities should be <entity count>
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 6            | asynchronous |
	| LocalDb       | 6            | synchronous  |

@ExternalDatabase
Scenario Outline: Insert and select all (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	And I query for the count of all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
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

@InMemoryDatabase
Scenario Outline: Find (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for a maximum of <max> <entity type> entities reverse ordered skipping <skip> records
	Then the queried entities should be the same as the ones I inserted, in reverse order, starting from <skip> counting <max>
	Examples: 
	| database type | entity type | entity count | max  | skip | method type  |
	| LocalDb       | workstation | 10           | 1    | 2    | asynchronous |
	| SqLite        | workstation | 10           | 1    | 2    | asynchronous |
	| LocalDb       | workstation | 10           | 1    | 2    | synchronous  |
	| SqLite        | workstation | 10           | 1    | 2    | synchronous  |
	| LocalDb       | workstation | 10           | 1    | NULL | synchronous  |
	| SqLite        | workstation | 10           | 1    | NULL | synchronous  |
	| LocalDb       | workstation | 10           | NULL | 2    | synchronous  |
	| SqLite        | workstation | 10           | NULL | 2    | synchronous  |
	| LocalDb       | workstation | 10           | NULL | NULL | synchronous  |
	| SqLite        | workstation | 10           | NULL | NULL | synchronous  |
	| LocalDb       | workstation | 10           | 1    | 0    | synchronous  |
	| SqLite        | workstation | 10           | 1    | 0    | synchronous  |

@ExternalDatabase
Scenario Outline: Find (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for a maximum of <max> <entity type> entities reverse ordered skipping <skip> records
	Then the queried entities should be the same as the ones I inserted, in reverse order, starting from <skip> counting <max>
	Examples:
	| database type | entity type | entity count | max  | skip | method type  |
	| MySql         | workstation | 10           | 1    | 2    | asynchronous |
	| PostgreSql    | workstation | 10           | 1    | 2    | asynchronous |
	| MySql         | workstation | 10           | 1    | 2    | synchronous  |
	| PostgreSql    | workstation | 10           | 1    | 2    | synchronous  |
	| MySql         | workstation | 10           | 1    | NULL | synchronous  |
	| PostgreSql    | workstation | 10           | 1    | NULL | synchronous  |
	| MySql         | workstation | 10           | NULL | 2    | synchronous  |
	| PostgreSql    | workstation | 10           | NULL | 2    | synchronous  |
	| MySql         | workstation | 10           | NULL | NULL | synchronous  |
	| PostgreSql    | workstation | 10           | NULL | NULL | synchronous  |
	| MySql         | workstation | 10           | 1    | 0    | synchronous  |
	| PostgreSql    | workstation | 10           | 1    | 0    | synchronous  |

@InMemoryDatabase
Scenario Outline: Insert and select by primary key (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I query for the inserted <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
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
	Then the queried entities should be the same as the local ones
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

@InMemoryDatabase
Scenario Outline: Update by primary keys (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I update all the inserted <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones	
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 1            | asynchronous |
	| LocalDb       | workstation | 1            | asynchronous |
	| LocalDb       | building    | 1            | asynchronous |
	| LocalDb       | employee    | 1            | synchronous  |
	| LocalDb       | workstation | 1            | synchronous  |
	| SqLite        | building    | 1            | asynchronous |
	| LocalDb       | building    | 1            | synchronous  |
	| SqLite        | building    | 1            | synchronous  |

@ExternalDatabase
Scenario Outline: Update by primary keys (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I update all the inserted <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 1            | asynchronous |
	| PostgreSql    | employee    | 1            | asynchronous |
	| MySql         | workstation | 1            | asynchronous |
	| PostgreSql    | workstation | 1            | asynchronous |
	| MySql         | building    | 1            | asynchronous |
	| PostgreSql    | building    | 1            | asynchronous |
	| MySql         | employee    | 1            | synchronous  |
	| PostgreSql    | employee    | 1            | synchronous  |
	| MySql         | workstation | 1            | synchronous  |
	| PostgreSql    | workstation | 1            | synchronous  |
	| MySql         | building    | 1            | synchronous  |
	| PostgreSql    | building    | 1            | synchronous  |

@InMemoryDatabase
Scenario Outline: Partial update (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I partially update all the inserted <entity type> entities
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 3            | asynchronous |
	| LocalDb       | employee    | 3            | synchronous  |

@ExternalDatabase
Scenario Outline: Partial update (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I partially update all the inserted <entity type> entities
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 3            | asynchronous |
	| PostgreSql    | employee    | 3            | asynchronous |
	| MySql         | employee    | 3            | synchronous  |
	| PostgreSql    | employee    | 3            | synchronous  |

@InMemoryDatabase
Scenario Outline: Delete by primary keys (in-memory database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I delete all the inserted <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples: 
	| database type | entity type | entity count | method type  |
	| LocalDb       | employee    | 3            |asynchronous |
	| LocalDb       | workstation | 3            |asynchronous |
	| LocalDb       | building    | 3            |asynchronous |
	| SqLite        | building    | 3            |asynchronous |
	| SqLite        | building    | 3            |synchronous |
	| LocalDb       | employee    | 3            |synchronous |
	| LocalDb       | workstation | 3            |synchronous |
	| LocalDb       | building    | 3            |synchronous |

@ExternalDatabase
Scenario Outline: Delete by primary keys (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <method type> methods
	And I delete all the inserted <entity type> entities using <method type> methods
	And I query for all the <entity type> entities using <method type> methods
	Then the queried entities should be the same as the local ones
	Examples:
	| database type | entity type | entity count | method type  |
	| MySql         | employee    | 3            |asynchronous |
	| PostgreSql    | employee    | 3            |asynchronous |
	| MySql         | workstation | 3            |asynchronous |
	| MySql         | workstation | 3            |synchronous |
	| PostgreSql    | workstation | 3            |asynchronous |
	| MySql         | building    | 3            |asynchronous |
	| MySql         | building    | 3            |synchronous |
	| PostgreSql    | building    | 3            |asynchronous |
	| MySql         | employee    | 3            |synchronous |
	| PostgreSql    | employee    | 3            |synchronous |
	| PostgreSql    | workstation | 3            |synchronous |
	| PostgreSql    | building    | 3            |synchronous |
