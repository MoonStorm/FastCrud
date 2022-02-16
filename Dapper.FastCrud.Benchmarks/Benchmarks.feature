Feature: Benchmarks

Scenario Outline: Insert benchmark
	Given I have initialized a <database type> database
	When I start the stopwatch
	And I insert <entity count> <entity type> using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <entity count> operations of type insert
	Then I should have <entity count> <entity type> in the database
	And I cleanup the <database type> database
	Examples: 
	| database type     | entity type        | entity count | orm                               |
	| Benchmark LocalDb | benchmark entities | 10000        | Simple Crud                       |
	| Benchmark LocalDb | benchmark entities | 10000        | Dapper Extensions                 |
	| Benchmark LocalDb | benchmark entities | 10000        | Fast Crud                         |
	| Benchmark LocalDb | benchmark entities | 10000        | Dapper                            |
	| Benchmark LocalDb | benchmark entities | 10000        | Entity Framework - single op/call |
	# unrealistic
	# | Benchmark LocalDb | benchmark entities | 10000        | Entity Framework - batch          |

#Scenario Outline: Batch Select No Filter No Warmup Benchmark
#	Given I have initialized a <database type> database
#	When I insert <entity count> <entity type> using ADO .NET
#	And I refresh the database connection
#	And I start the stopwatch
#	And I select all the <entity type> using <orm>
#	And I stop the stopwatch
#	And I report the stopwatch value for <orm> finished processing 1 operations of type select all - no warmup
#	Then I should have queried <entity count> entities
#	And the queried entities should be the same as the ones I inserted
#	And I cleanup the <database type> database
#	Examples: 
#	| database type | entity type        | entity count | orm         |
#	| LocalDb   | benchmark entities | 100000        | Simple Crud       |
#	| LocalDb   | benchmark entities | 100000        | Dapper Extensions |
#	| LocalDb   | benchmark entities | 100000        | Fast Crud         |
#	| LocalDb   | benchmark entities | 100000        | Dapper            |

Scenario Outline: Select all
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using ADO .NET
	And I refresh the database connection
	And I start the stopwatch
	And I select all the <entity type> entities using <orm> <op count> times
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <op count> operations of type select all
	Then I should have queried <entity count> <entity type> entities
	# already tested for consistency (this takes a long time)
	#And the queried <entity type> entities should be shallowly the same as the inserted ones
	And I cleanup the <database type> database
	Examples: 
	| database type     | entity type | entity count | op count | orm               |
	| Benchmark LocalDb | benchmark   | 10           | 10000    | Simple Crud       |
	| Benchmark LocalDb | benchmark   | 10           | 10000    | Dapper Extensions |
	| Benchmark LocalDb | benchmark   | 10           | 10000    | Fast Crud         |
	| Benchmark LocalDb | benchmark   | 10           | 10000    | Dapper            |
	| Benchmark LocalDb | benchmark   | 10           | 10000    | Entity Framework  |

Scenario Outline: Delete by id
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using ADO .NET
	And I refresh the database connection
	And I start the stopwatch
	And I delete all the inserted <entity type> entities using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <entity count> operations of type delete
	Then I should have 0 <entity type> entities in the database
	And I cleanup the <database type> database
	Examples: 
	| database type     | entity type | entity count | orm                               |
	| Benchmark LocalDb | benchmark   | 10000        | Simple Crud                       |
	| Benchmark LocalDb | benchmark   | 10000        | Dapper Extensions                 |
	| Benchmark LocalDb | benchmark   | 10000        | Fast Crud                         |
	| Benchmark LocalDb | benchmark   | 10000        | Dapper                            |
	| Benchmark LocalDb | benchmark   | 10000        | Entity Framework - single op/call |
	# unrealistic
	#| Benchmark LocalDb | benchmark   | 10000        | Entity Framework - batch          |

Scenario Outline: Select by id
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using ADO .NET
	And I refresh the database connection
	And I start the stopwatch
	And I select all the <entity type> entities that I previously inserted using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <entity count> operations of type select by id
	Then I should have queried <entity count> <entity type> entities
	# already tested for consistency (this takes a long time)
	#And the queried <entity type> entities should be shallowly the same as the inserted ones
	And I cleanup the <database type> database
	Examples: 
	| database type     | entity type | entity count | orm               |
	| Benchmark LocalDb | benchmark   | 10000        | Simple Crud       |
	| Benchmark LocalDb | benchmark   | 10000        | Dapper Extensions |
	| Benchmark LocalDb | benchmark   | 10000        | Fast Crud         |
	| Benchmark LocalDb | benchmark   | 10000        | Dapper            |
	| Benchmark LocalDb | benchmark   | 10000        | Entity Framework  |

Scenario Outline: Update by id
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> entities using <orm>
	And I refresh the database connection
	And I start the stopwatch
	And I update all the <entity type> entities that I previously inserted using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <entity count> operations of type update
	And I select all the <entity type> entities using Dapper 1 times
	Then I should have queried <entity count> <entity type> entities
	# already tested for consistency (this takes a long time)
	#Then the queried <entity type> entities should be shallowly the same as the updated ones
	Then I cleanup the <database type> database
	Examples: 
	| database type     | entity type | entity count | orm                               |
	| Benchmark LocalDb | benchmark   | 10000        | Simple Crud                       |
	| Benchmark LocalDb | benchmark   | 10000        | Dapper Extensions                 |
	| Benchmark LocalDb | benchmark   | 10000        | Fast Crud                         |
	| Benchmark LocalDb | benchmark   | 10000        | Dapper                            |
	| Benchmark LocalDb | benchmark   | 10000        | Entity Framework - single op/call |
	#unrealistic
	# | Benchmark LocalDb | benchmark   | 10000        | Entity Framework - batch         |
