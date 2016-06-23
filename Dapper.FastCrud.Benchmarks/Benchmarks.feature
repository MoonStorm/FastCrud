Feature: Queries 

@InMemoryBenchmark
Scenario Outline: Insert Benchmark
	Given I have initialized a <database type> database
	When I start the stopwatch
	And I insert <entity count> <entity type> using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <entity count> operations of type insert
	Then I should have <entity count> <entity type> in the database
	And I cleanup the <database type> database
	Examples: 
	| database type     | entity type        | entity count | orm               |
	| Benchmark LocalDb | benchmark entities | 30000        | Simple Crud       |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper Extensions |
	| Benchmark LocalDb | benchmark entities | 30000        | Fast Crud         |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper            |
	| Benchmark LocalDb | benchmark entities | 30000        | Entity Framework  |

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
#	| LocalDb   | benchmark entities | 20000        | Simple Crud       |
#	| LocalDb   | benchmark entities | 20000        | Dapper Extensions |
#	| LocalDb   | benchmark entities | 20000        | Fast Crud         |
#	| LocalDb   | benchmark entities | 20000        | Dapper            |

@InMemoryBenchmark
Scenario Outline: Batch Select No Filter
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> using ADO .NET
	And I refresh the database connection
	And I start the stopwatch
	And I select all the <entity type> using <orm>
	And I clear all the queried entities
	And I select all the <entity type> using <orm>
	And I clear all the queried entities
	And I select all the <entity type> using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing 3 operations of type select all
	Then I should have queried <entity count> entities
	Then the queried entities should be the same as the local ones
	And I cleanup the <database type> database
	Examples: 
	| database type     | entity type        | entity count | orm               |
	| Benchmark LocalDb | benchmark entities | 30000        | Simple Crud       |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper Extensions |
	| Benchmark LocalDb | benchmark entities | 30000        | Fast Crud         |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper            |
	| Benchmark LocalDb | benchmark entities | 30000        | Entity Framework  |

@InMemoryBenchmark
Scenario Outline: Single Delete Benchmark
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> using ADO .NET
	And I refresh the database connection
	And I start the stopwatch
	And I delete all the inserted <entity type> using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <entity count> operations of type delete
	Then I should have 0 <entity type> in the database
	And I cleanup the <database type> database
	Examples: 
	| database type     | entity type        | entity count | orm               |
	| Benchmark LocalDb | benchmark entities | 30000        | Simple Crud       |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper Extensions |
	| Benchmark LocalDb | benchmark entities | 30000        | Fast Crud         |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper            |
	| Benchmark LocalDb | benchmark entities | 30000        | Entity Framework  |

@InMemoryBenchmark
Scenario Outline: Single Select Id Filter Benchmark
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> using ADO .NET
	And I refresh the database connection
	And I start the stopwatch
	And I select all the <entity type> that I previously inserted using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <entity count> operations of type select by id
	Then I should have queried <entity count> entities
	Then the queried entities should be the same as the local ones
	And I cleanup the <database type> database
	Examples: 
	| database type     | entity type        | entity count | orm               |
	| Benchmark LocalDb | benchmark entities | 30000        | Simple Crud       |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper Extensions |
	| Benchmark LocalDb | benchmark entities | 30000        | Fast Crud         |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper            |
	| Benchmark LocalDb | benchmark entities | 30000        | Entity Framework  |

@InMemoryBenchmark
Scenario Outline: Single Update Benchmark
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> using ADO .NET
	And I refresh the database connection
	And I start the stopwatch
	And I update all the <entity type> that I previously inserted using <orm>
	And I stop the stopwatch
	And I report the stopwatch value for <orm> finished processing <entity count> operations of type update
	And I select all the <entity type> using Dapper
	Then the queried entities should be the same as the local ones
	Then I cleanup the <database type> database
	Examples: 
	| database type     | entity type        | entity count | orm               |
	| Benchmark LocalDb | benchmark entities | 30000        | Simple Crud       |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper Extensions |
	| Benchmark LocalDb | benchmark entities | 30000        | Fast Crud         |
	| Benchmark LocalDb | benchmark entities | 30000        | Dapper            |
	| Benchmark LocalDb | benchmark entities | 30000        | Entity Framework  |
