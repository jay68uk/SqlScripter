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
        var server = new Server(connection);

        try
        {
            var database = server.Databases[configurationRoot["DatabaseName"]];
            if (database == null)
            {
                throw new Exception($"Database '{configurationRoot["DatabaseName"]}' not found.");
            }

            var outputPath = configurationRoot["OutputPath"] ?? string.Empty;

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

    private static void ScriptStoredProcedures(string outputPath, Database database, ProgressTask task, int demoModeLimit)
    {
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
                var safeFileName = string.Concat(storedProcedure.Name.Split(Path.GetInvalidFileNameChars()));
                var fileName = Path.Combine(outputPath, $"{safeFileName}.sql");

                var scriptHeader = $@"
                    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{storedProcedure.Name}]') AND type in (N'P'))
                    BEGIN
                        ALTER PROCEDURE [dbo].[{storedProcedure.Name}]
                    END
                    ELSE
                    BEGIN
                        CREATE PROCEDURE [dbo].[{storedProcedure.Name}]
                    END";
                
                var fullScript = scriptHeader + Environment.NewLine + ScriptUtilities.JoinScriptParts(script);

                File.WriteAllText(fileName, fullScript);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Error scripting stored procedure [white]{storedProcedure.Name}: {ex.Message}[/]");
            }

            if (demoModeLimit > 0 && itemsScripted >= demoModeLimit)
            {
                break;
            }
        }
    }
    
    private static void ScriptFunctions(string outputPath, Database database, ProgressTask task, int demoModeLimit)
    {
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
                var safeFileName = string.Concat(function.Name.Split(Path.GetInvalidFileNameChars()));
                var fileName = Path.Combine(outputPath, $"{safeFileName}.sql");

                var scriptHeader = $@"
                    IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{function.Name}]') AND type in (N'FN', N'IF', N'TF'))
                    BEGIN
                        ALTER FUNCTION [dbo].[{function.Name}]
                    END
                    ELSE
                    BEGIN
                        CREATE FUNCTION [dbo].[{function.Name}]
                    END";
                
                var fullScript = scriptHeader + Environment.NewLine + ScriptUtilities.JoinScriptParts(script);

                File.WriteAllText(fileName, fullScript);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Error scripting function [white]{function.Name}: {ex.Message}[/]");
            }

            if (demoModeLimit > 0 && itemsScripted >= demoModeLimit)
            {
                break;
            }
        }
    }
}
