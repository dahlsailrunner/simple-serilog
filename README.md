# Simple Serilog
This is a set of demo projects that will demonstrate the use of Serilog in a more complex example than "hello, world" but still be easy to understand.

## Features of the Application
* ASP.NET Core MVC site with using MVC Views (rather than Razor Pages)
* ASP.NET Core Web API in a separate project
* All pages/views except the home page require you to be logged in
* Login authentication is accomplished via OpenID Connect using a public demo of IdentityServer4
* API calls are all authenticated using JWT bearer tokens provided by the demo version of IdentityServer4
* Sample pages and routes are available that include route- and query-based parameters
* Various pages have known/expected exceptions that will occur

## Goals
To demonstrate logging in a real-world (albeit simple example) such that it is easy to automatically include the following:
* Username and claims regarding the logged-in user
* Machine name
* Assembly info
* All exception details 
* Route / path / querystring details
* Shield information from user, but provide "id" for easy lookup for support-type folks

All of the above should be achieved to the extent possible globally without individual log entry calls needing to provide any of that information.

Further, we have 4 different types of log entries that we will care about:
* **Usage** -- identifying usage of certain functions - primarily within the UI
* **Performance** -- tracking the performance of whatever we'd like tracked, including every API method.
* **Errors** -- every error in either location should be automatically logged and fully shielded from users.
* **Diagnostics** -- ad-hoc entries to help in troubleshooting specific issues.

By the end of our journey from the baseline folder to the other folders, we want to 
get the different types of entries routed to different Serilog sinks, as well as having them formatted based on the entry type that is being created.  All of this supports easier analysis and reporting of the entries.

## Baseline
The baseline folder contains the starting point for a bit of a journey to achieve what we're ultimately after.

The baseline has the following characteristics:
* Almost all Serilog config is in the Simple.Serilog shared assembly
* It uses enrichers to include the desired information above, including the ``Serilog.Enrichers.AspnetcoreHttpcontext`` Nuget package for some of the info
* It writes all entries from a single app to a json file for that app (in C:\temp\Logs)
* It uses a Filter for API performance tracking
* It uses an Attribute for UI usage tracking -- applied at the method/action level

## Filters
The content in this folder will modify the Baseline folder to get the different types
of log entries routed to the proper sink.  Instead of file-based sinks, we will 
use SQL Server and Elasticsearch.

The connection string for the SQL sink is in the ``Simple.Serilog->SerilogHelpers.cs`` file.  Out of the box here it uses a local SQL Express instance with a database called Logging.

The ELK stack URL is in the same ``Simple.Serilog->SerilogHelpers.cs`` and uses a URI of http://localhost:9200 for the Elasticsearch endpoint.  The docker container ``sebp/elk`` is used to run the ELK stack locally.

## Formatters

