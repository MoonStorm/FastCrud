The logic in these files might seem confusing at first. But let's try to shed some light on what's actually going on in here.

Aggregated Options
-------------------
Start with the aggregated options in StatementOptions/Aggregated. 
These are the final options that get funnelled down from the public surface.
These options are directly used by the internal algorithms.
The main one is called AggregatedSqlStatementOptions and contains all the options relevant to the main query.
It also holds instances of AggregatedRelationalSqlStatementOptions, where information and options related to the JOINed entities are being stored.


Aggregated Options Builders
---------------------------
All the aggregated options above can be set through the aggregated options builders living inside StatementOptions/Builders/Aggregated.
Same as the options, these classes aree stil internal as they're not meant to be exposed publicly.
They are used to set up options in a fluent way (e.g. TStatementBuilder SetOption(optionValue)), 
  reason why they need to be set up from above with the ACTUAL builder instance to be returned (see the abstract TStatementBuilder Builder property).


Situational Options Setters
----------------------------
Situational options setters cover a subset of what is exposed out of the aggregated options builder.
  For example, for ALL the statements, the IStandardSqlStatementOptionsSetter will expose methods that are made available to all the statements.
  Another example would be the IRangedConditionalSqlStatementOptionsSetter, which is only exposed where a range of entities are expected to be returned by the statement.
They are publicly exposed stright from the StatementOptions folder.


Situational Options Builders
-----------------------------
The situational options builders have several roles:
  1. Their public interface combines a set of situational options setters, exposing them together as a group.
  2. Their empty internal implementation sits on top of the full aggregated options builder, in order to have the set of situational options setters resolve directly onto the aggregated options builder. 
  3. They also set themselves as the return builder for the fluent pattern.
To simplify the layout, every situational builder lives together with their corresponding public interface in files inside the StatementOptions/Builders folder.
