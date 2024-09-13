# SQL Scripter
Produces scripts for stored procedures and functions within a given SQL database.

By default the scripts are stored in /scripts/storedprocedures and scripts/functions. These folders are created if they don't already exist.

## Setup
In appsettings.json set the "DefaultConnection" to the server instance e.g. "localhost,4134".

### Demo mode
This is enabled by passing a command line argument of the number which limits the number of scripts to produce.
e.g. to limit it to 10 stored procedures and 10 functions
> dotnet run 10

**Visual Studio**
<p>Add the following into launchSettings.json to run/debug from the IDE</p>

```
  "DemoMode": {
    "commandName": "Project",
    "commandLineArgs": "10"
  }
```
