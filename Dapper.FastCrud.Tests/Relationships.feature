Feature: Relationships
	Tests for the relationship between various entities

@AutomaticBuildServerTest
Scenario Outline: Query single relationship parent with children (build server test)
	Given I have initialized a <database type> database
	When I insert 1 workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for one workstation entity combined with the employee entities using <method type> methods
	Then the queried workstation entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: Query single relationship parent with children (external database)
	Given I have initialized a <database type> database
	When I insert 1 workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for one workstation entity combined with the employee entities using <method type> methods
	Then the queried workstation entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query single relationship one-to-one (build server test)
	Given I have initialized a <database type> database
	When I insert <employee count> employee entities using <method type> methods
	And I assign unique badges to the last inserted <badge count> employee entities using <method type> methods
	And I query for all the employee entities combined with the assigned badge entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | employee count | badge count | method type  |
	| LocalDb       | 10             | 2            | synchronous  |
	| LocalDb       | 10             | 3            | asynchronous |

@ExternalDatabase
Scenario Outline: Query single relationship one-to-one (external database)
	Given I have initialized a <database type> database
	When I insert <employee count> employee entities using <method type> methods
	And I assign unique badges to the last inserted <badge count> employee entities using <method type> methods
	And I query for all the employee entities combined with the assigned badge entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | employee count | badge count | method type  |
	| PostgreSql    | 10             | 2           | synchronous  |
	| PostgreSql    | 10             | 3           | asynchronous |
	| MySql         | 10             | 4           | synchronous  |
	| MySql         | 10             | 6           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query single relationship parents with children (build server test)
	Given I have initialized a <database type> database
	When I insert <parent entity count> workstation entities using <method type> methods
	And I insert <child entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the workstation entities combined with the employee entities using <method type> methods
	Then the queried workstation entities should be the same as the inserted ones
	Examples: 
	| database type | parent entity count | child entity count | method type  |
	| LocalDb       | 10                  | 20                 | synchronous  |
	| LocalDb       | 10                  | 20                 | asynchronous |

@ExternalDatabase
Scenario Outline: Query single relationship parents with children (external database)
	Given I have initialized a <database type> database
	When I insert <parent entity count> workstation entities using <method type> methods
	And I insert <child entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the workstation entities combined with the employee entities using <method type> methods
	Then the queried workstation entities should be the same as the inserted ones
	Examples: 
	| database type | parent entity count | child entity count | method type  |
	| PostgreSql    | 10                  | 20                 | synchronous  |
	| PostgreSql    | 10                  | 20                 | asynchronous |
	| MySql         | 10                  | 20                 | synchronous  |
	| MySql         | 10                  | 20                 | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Count single relationship parents with children (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I insert 3 workstation entities using <method type> methods
	And I query for the count of all the workstation entities combined with the employee entities using <method type> methods
	Then the result of the last query count should be <entity count>
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: Count single relationship parents with children (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I insert 3 workstation entities using <method type> methods
	And I query for the count of all the workstation entities combined with the employee entities using <method type> methods
	Then the result of the last query count should be <entity count>
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: The manual ON clause can be used when relationships and navigation properties are not set up in the mapping (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I insert 3 workstation entities using <method type> methods
	And I query for the count of all the workstation entities combined with the employee entities when no relationships or navigation properties are set up using <method type> methods
	Then the result of the last query count should be <entity count>
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: The manual ON clause can be used when relationships and navigation properties are not set up in the mapping (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I insert 3 workstation entities using <method type> methods
	And I query for the count of all the workstation entities combined with the employee entities when no relationships or navigation properties are set up using <method type> methods
	Then the result of the last query count should be <entity count>
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: A custom join can be used even for navigation properties (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the employee entities combined with the workstation entities when no relationships or navigation properties are set up using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: A custom join can be used even for navigation properties (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the employee entities combined with the workstation entities when no relationships or navigation properties are set up using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline:  Query single relationship children with parents (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the employee entities combined with the workstation entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline:  Query single relationship children with parents (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the employee entities combined with the workstation entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query single relationship children with no parents (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities using <method type> methods
	And I query for all the employee entities combined with the workstation entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: Query single relationship children with no parents (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities using <method type> methods
	And I query for all the employee entities combined with the workstation entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query single relationship parents with no children (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I query for all the workstation entities combined with the employee entities using <method type> methods
	Then the queried workstation entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: Query single relationship parents with no children (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> workstation entities using <method type> methods
	And I query for all the workstation entities combined with the employee entities using <method type> methods
	Then the queried workstation entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query two level relationship grandparents with parents and children (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I insert <entity count> workstation entities parented to existing building entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the building entities combined with workstation and employee entities using <method type> methods
	Then the queried building entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: Query two level relationship grandparents with parents and children (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I insert <entity count> workstation entities parented to existing building entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the building entities combined with workstation and employee entities using <method type> methods
	Then the queried building entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query two level relationship children with parents and grandparents (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I insert <entity count> workstation entities parented to existing building entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the employee entities combined with workstation and building entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: Query two level relationship children with parents and grandparents (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> building entities using <method type> methods
	And I insert <entity count> workstation entities parented to existing building entities using <method type> methods
	And I insert <entity count> employee entities parented to existing workstation entities using <method type> methods
	And I query for all the employee entities combined with workstation and building entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query two relationships back to the same entity (build server test)
	Given I have initialized a <database type> database
	When I insert <referenced entity count> employee entities using <method type> methods
	And I insert <referencing entity count> employee entities as children of promoted manager and supervisor employee entities using <method type> methods
	And I query for all the employee entities combined with themselves as managers and supervisors using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | referenced entity count | referencing entity count | method type  |
	| LocalDb       | 3                       | 2                        | synchronous  |
	| LocalDb       | 10                      | 5                        | asynchronous |

@ExternalDatabase
Scenario Outline: Query two relationships back to the same entity (external database)
	Given I have initialized a <database type> database
	When I insert <referenced entity count> employee entities using <method type> methods
	And I insert <referencing entity count> employee entities as children of promoted manager and supervisor employee entities using <method type> methods
	And I query for all the employee entities combined with themselves as managers and supervisors using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | referenced entity count | referencing entity count | method type  |
	| PostgreSql    | 5                       | 3                        | synchronous  |
	| PostgreSql    | 5                       | 3                        | asynchronous |
	| MySql         | 5                       | 3                        | synchronous  |
	| MySql         | 5                       | 3                        | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query a subset of entities having two relationships back to the same entity (build server test)
	Given I have initialized a <database type> database
	When I insert <referenced entity count> employee entities using <method type> methods
	And I insert <referencing entity count> employee entities as children of promoted manager and supervisor employee entities using <method type> methods
	And I query for the last <query count> inserted employee entities combined with themselves as managers and supervisors using <method type> methods
	Then I should have queried <query count> employee entities
	And the queried employee entities should be the same as the last <query count> inserted ones
	Examples: 
	| database type | referenced entity count | referencing entity count | query count | method type  |
	| LocalDb       | 10                      | 5                        | 11          | synchronous  |
	| LocalDb       | 10                      | 7                        | 13          | asynchronous |

@ExternalDatabase
Scenario Outline: Query a subset of entities having two relationships back to the same entity (external database)
	Given I have initialized a <database type> database
	When I insert <referenced entity count> employee entities using <method type> methods
	And I insert <referencing entity count> employee entities as children of promoted manager and supervisor employee entities using <method type> methods
	And I query for the last <query count> inserted employee entities combined with themselves as managers and supervisors using <method type> methods
	Then I should have queried <query count> employee entities
	And the queried employee entities should be the same as the last <query count> inserted ones
	Examples: 
	| database type | referenced entity count | referencing entity count | query count | method type  |
	| PostgreSql    | 3                       | 2                        | 3           | synchronous  |
	| PostgreSql    | 3                       | 2                        | 3           | asynchronous |
	| MySql         | 20                      | 10                       | 12          | synchronous  |
	| MySql         | 20                      | 10                       | 14          | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Query two level relationship children with no parents or grandparents (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities using <method type> methods
	And I query for all the employee entities combined with workstation and building entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: Query two level relationship children with no parents or grandparents (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities using <method type> methods
	And I query for all the employee entities combined with workstation and building entities using <method type> methods
	Then the queried employee entities should be the same as the inserted ones
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |

@AutomaticBuildServerTest
Scenario Outline: Count two level relationship children with no parents or grandparents (build server test)
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities using <method type> methods
	And I query for the count of all the employee entities strictly linked to workstation and building entities using <method type> methods
	Then the result of the last query count should be 0
	Examples: 
	| database type | entity count | method type  |
	| LocalDb       | 10           | synchronous  |
	| LocalDb       | 10           | asynchronous |

@ExternalDatabase
Scenario Outline: Count two level relationship children with no parents or grandparents (external database)
	Given I have initialized a <database type> database
	When I insert <entity count> employee entities using <method type> methods
	And I query for the count of all the employee entities strictly linked to workstation and building entities using <method type> methods
	Then the result of the last query count should be 0
	Examples: 
	| database type | entity count | method type  |
	| PostgreSql    | 10           | synchronous  |
	| PostgreSql    | 10           | asynchronous |
	| MySql         | 10           | synchronous  |
	| MySql         | 10           | asynchronous |
