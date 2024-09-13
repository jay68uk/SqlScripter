using Microsoft.Extensions.Configuration;
using Spectre.Console;
using SqlSpScaffolder.App;

var limitProcessing = 0;
var config = new ConfigurationBuilder()
  .SetBasePath(Directory.GetCurrentDirectory())
  .AddJsonFile("app.settings", optional: true, reloadOnChange: true)
  .Build();

  AnsiConsole.MarkupLine("[bold yellow]Welcome to Sql Script Scaffolder:[/]");

  if (args.Length > 0 && int.TryParse(args[0], out var demoModeLimit))
  {
    limitProcessing = demoModeLimit;
    AnsiConsole.MarkupLine($"[bold yellow]Demo Mode Limit: {demoModeLimit} [/]");
  }
  
  ScaffoldStoredProcedures.Invoke(config, limitProcessing);

  AnsiConsole.MarkupLine("[bold yellow]Scaffolding completed.[/]");