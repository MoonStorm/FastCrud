Feature: Queries

Scenario Outline: Insert Benchmark
	Given I have initialized a <database type> database
	When I start the stopwatch
	And I insert <entity count> <entity type> using <micro orm>
	And I stop the stopwatch
	And I report the stopwatch value for <micro orm> finished processing <entity count> entities for an operation of type insert
	Then I should have <entity count> <entity type> in the database
	And I cleanup the <database type> database
	Examples: 
	| database type | entity type             | entity count | micro orm       |
	| LocalDb       | single int key entities | 10           | Dapper          |
	| LocalDb       | single int key entities | 10           | Fast Crud       |
	| LocalDb       | single int key entities | 10           | Simple Crud     |
	| LocalDb       | single int key entities | 20000        | Dapper          |
	| LocalDb       | single int key entities | 20000        | Fast Crud       |
	| LocalDb       | single int key entities | 20000        | Simple Crud     |

Scenario Outline: Batch Select No Filter Benchmark
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> using Fast Crud
	And I start the stopwatch
	And I select all the <entity type> using <micro orm>
	And I stop the stopwatch
	And I report the stopwatch value for <micro orm> finished processing <entity count> entities for an operation of type batch select - no filter
	Then I should have queried <entity count> entities
	And the queried entities should be the same as the ones I inserted
	And I cleanup the <database type> database
	Examples: 
	| database type | entity type             | entity count | micro orm       |
	| LocalDb       | single int key entities | 20000        | Dapper          |
	| LocalDb       | single int key entities | 20000      | Fast Crud       |
	| LocalDb       | single int key entities | 20000       | Simple Crud     |

Scenario Outline: Single Delete Benchmark
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> using Fast Crud
	And I start the stopwatch
	And I delete all the inserted <entity type> using <micro orm>
	And I stop the stopwatch
	And I report the stopwatch value for <micro orm> finished processing <entity count> entities for an operation of type delete
	Then I should have 0 <entity type> in the database
	And I cleanup the <database type> database
	Examples: 
	| database type | entity type             | entity count | micro orm       |
	| LocalDb       | single int key entities | 20000        | Dapper          |
	| LocalDb       | single int key entities | 20000      | Fast Crud       |
	| LocalDb       | single int key entities | 20000       | Simple Crud     |

Scenario Outline: Single Select Id Filter Benchmark
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> using Fast Crud
	And I start the stopwatch
	And I select all the <entity type> that I previously inserted using <micro orm>
	And I stop the stopwatch
	And I report the stopwatch value for <micro orm> finished processing <entity count> entities for an operation of type select by id
	Then I should have queried <entity count> entities
	And the queried entities should be the same as the ones I inserted
	And I cleanup the <database type> database
	Examples: 
	| database type | entity type             | entity count | micro orm   |
	| LocalDb       | single int key entities | 20000        | Dapper      |
	| LocalDb       | single int key entities | 20000        | Fast Crud   |
	| LocalDb       | single int key entities | 20000        | Simple Crud |

Scenario Outline: Single Update Benchmark
	Given I have initialized a <database type> database
	When I insert <entity count> <entity type> using Fast Crud
	And I start the stopwatch
	And I update all the <entity type> that I previously inserted using <micro orm>
	And I stop the stopwatch
	And I report the stopwatch value for <micro orm> finished processing <entity count> entities for an operation of type update
	And I select all the <entity type> using Dapper
	Then the queried entities should be the same as the ones I updated
	Then I cleanup the <database type> database
	Examples: 
	| database type | entity type             | entity count | micro orm   |
	| LocalDb       | single int key entities | 20000        | Dapper      |
	| LocalDb       | single int key entities | 20000       | Fast Crud   |
	| LocalDb       | single int key entities | 20000        | Simple Crud |
