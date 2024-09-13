using System.Net.Quic;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Spectre.Console;

namespace SqlSpScaffolder.App;

public static class ScaffoldStoredProcedures
{
    public static void Invoke(IConfigurationRoot configurationRoot, int demoModeLimit)
    {
        var connection = new ServerConnection(configurationRoot.GetConnectionString("DefaultConnection"));
        var databaseName = configurationRoot["DatabaseName"];
        connection.DatabaseName = databaseName;
        var server = new Server(connection);

        try
        {
            var database = server.Databases[databaseName];
            if (database == null)
            {
                throw new SqlServerManagementException($"Database '{configurationRoot["DatabaseName"]}' not found.");
            }

            var outputPath = configurationRoot["OutputPath"] ?? "../scripts";
            var storedProcedureFolder = configurationRoot["StoredProcedureFolder"] ?? "/storedprocedures";
            var functionsFolder = configurationRoot["FunctionsFolder"] ?? "/functions";
            CheckOutputPathExists(outputPath + storedProcedureFolder, outputPath + functionsFolder);

            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var taskStoredProcs = ctx.AddTask("[green]Processing Stored Procedures[/]", maxValue: database.StoredProcedures.Count);
                    var taskFunctions = ctx.AddTask("[green]Processing Functions[/]", maxValue: database.UserDefinedFunctions.Count);

                    while (!ctx.IsFinished)
                    {
                        ScriptStoredProcedures(outputPath + "/storedprocedures", database, taskStoredProcs, demoModeLimit);
                        ScriptFunctions(outputPath + "/functions", database, taskFunctions, demoModeLimit);
                    }
                });
        }
        finally
        {
            // Ensure connection is properly closed when finished
            connection.Disconnect();
        }
    }

    private static void CheckOutputPathExists(string storedProcedureFolder, string functionsFolder)
    {
        if (!Directory.Exists(storedProcedureFolder))
        {
            Directory.CreateDirectory(storedProcedureFolder);
            AnsiConsole.MarkupLine($"[deepskyblue1]Directory created:[/] [white]'{storedProcedureFolder}'[/]");
        }

        if (Directory.Exists(functionsFolder)) return;
        Directory.CreateDirectory(functionsFolder);
        AnsiConsole.MarkupLine($"[deepskyblue1]Directory created:[/] [white]'{functionsFolder}'[/]");
    }

    private static void ScriptStoredProcedures(string outputPath, Database database, ProgressTask task, int demoModeLimit)
    {
        var databaseName = database.Name;
        var itemsScripted = 0;
        foreach (StoredProcedure storedProcedure in database.StoredProcedures)
        {
            task.Increment(1);
            if (storedProcedure.IsSystemObject)
            {
                continue;
            }

            itemsScripted++;
            try
            {
                var script = storedProcedure.Script();
                var fileName = ScriptUtilities.CreateSafeFileName(outputPath, storedProcedure.Name);

                var scriptHeader = ScriptUtilities.ScriptHeader(databaseName);
                
                var fullScript = scriptHeader + Environment.NewLine + ScriptUtilities.JoinScriptParts(script);

                File.WriteAllText(fileName, fullScript);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Error scripting stored procedure[/] [white]{storedProcedure.Name}: {ex.Message}[/]");
            }

            if (ScriptUtilities.DemoLimitReached(demoModeLimit, itemsScripted)) continue;
            task.StopTask();
            break;
        }
        
        task.StopTask();
    }

    private static void ScriptFunctions(string outputPath, Database database, ProgressTask task, int demoModeLimit)
    {
        var databaseName = database.Name;
        var itemsScripted = 0;
        foreach (UserDefinedFunction function in database.UserDefinedFunctions)
        {
            task.Increment(1);
            if (function.IsSystemObject)
            {
                continue;
            }

            itemsScripted++;
            try
            {
                var script = function.Script();
                var fileName = ScriptUtilities.CreateSafeFileName(outputPath, function.Name);

                var scriptHeader = ScriptUtilities.ScriptHeader(databaseName);
                
                var fullScript = scriptHeader + Environment.NewLine + ScriptUtilities.JoinScriptParts(script);

                File.WriteAllText(fileName, fullScript);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Error scripting function [white]{function.Name}[/]");
            }

            if (ScriptUtilities.DemoLimitReached(demoModeLimit, itemsScripted)) continue;
            task.StopTask();
            break;
        }
        
        task.StopTask();
    }
}
