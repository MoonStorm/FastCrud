For a full dev friendly experience, it is recommended to install 
the Specflow plugin for the IDE of your choice.

However, irrespective whether the SpecFlow plugin is available or not, the .feature files will
automatically get transpiled from the Gherkin syntax to common NUnit tests at build time, 
allowing them to be discoverable by the IDE and command line tools.

Tests tagged with @AutomaticBuildServerTest are the ones that receive immediate focus, 
since they get executed automatically at every check-in due to the minimal 
environment changes required (e.g. in-memory databases).

Tests tagged with @ExternalDatabase need to be executed manually due to the special environment 
requirements (e.g. a server needs to be installed and configured).
