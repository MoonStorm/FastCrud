Feature: Queries

Scenario Outline: Benchmark
	Given I have initialized a MsSql database
	And I started the stopwatch
	When I insert <entity count> <entity type> using <micro orm>
	And I stop the stopwatch
	And I report the stopwatch value for <micro orm>
	Then I should have <entity count> <entity type> in the database
	Examples: 
	| entity type             | entity count | micro orm   |
	| single int key entities | 1            | Dapper      |
	| single int key entities | 10            | Dapper      |
	| single int key entities | 1            | Fast Crud   |
	| single int key entities | 10            | Fast Crud   |
	| single int key entities | 1            | Simple Crud |
	| single int key entities | 10            | Simple Crud |
	| single int key entities | 20000        | Dapper      |
	| single int key entities | 20000        | Fast Crud   |
	| single int key entities | 20000        | Simple Crud |
