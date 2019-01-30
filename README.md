# fluentmigrator-bp3ext
BP3's extensions to [FluentMigrator](https://github.com/fluentmigrator/fluentmigrator).

## Background

BP3 recommends all database development to be done using migrations (see 
[Evolutionary Database Design](https://martinfowler.com/articles/evodb.html) by Martin Fowler).
The specific choice of migration tooling will depend upon the technology stack being used, but for Microsoft stacks
then FluentMigrator is recommended.

FluentMigrator does a great job with the database schema but doesn't handle Stored Procedures quite so well.
It is in the nature of the sort of DB development that BP3 engages in that Stored Procedures are potentially
more important than they would be for other Developers. Thus, the classes that you will find here extend 
the FluentMigrator base Migration class to provide better support for working with Stored Procedures.

## Using BP3 Extensions

1. Add the classes found here to your FluentMigrator project
1. Have your migration classes extend the `BP3Migration` class (from the `BP3Migrations.Migrations` package) rather than 
the `Migration` class (from the `FluentMigrator` package)
1. Make sure to call the `BP3Migration` constructor with the name of the project folder
1. For each Stored Procedure file instantiate a `ProcedureFile` and add it to a `List<ProcedureFile>` - don't forget to specify version and revision as required
1. Call the `CreateStoredProcedure` method of the `SPHelper` member variable with the list of ProcedureFiles in the `Up` method
1. Call the `DropStoredProcedure` method of the `SPHelper` member variable with the list of ProcedureFiles in the `Down` method

## How it works

The assumptions underpinning this implementation are

* Stored Procedures should have a version, e.g. v1, v2, etc
* A new version should be created each time the interface to the Stored Procedure changes
* The old version is not removed when a new version is created so as to suppport old code
* Stored Procedures can have revisions, e.g. '000', '001', etc
* For every Stored Procedure version the latest revision should be the current one

## Best practice

* When you create a new version of a Stored Procedure go back and create a new revision of the previous version that calls the new version you just created
