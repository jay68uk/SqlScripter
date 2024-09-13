using System.Collections.Specialized;
using System.Linq;

namespace SqlSpScaffolder.App;

internal static class ScriptUtilities
{
  public static string ScriptHeader(string databaseName) => $@"USE [{databaseName}]" + Environment.NewLine + "GO"; 
  public static string JoinScriptParts(StringCollection scriptParts)
  {
    return string.Join(Environment.NewLine, ConvertToEnumerable(scriptParts));
  }

  private static IEnumerable<string?> ConvertToEnumerable(StringCollection scriptParts)
  {
    foreach (var scriptPart in scriptParts)
    {
      yield return scriptPart!.Replace("CREATE", "CREATE OR ALTER");

      if (scriptPart!.Equals("SET QUOTED_IDENTIFIER ON"))
      {
        yield return "GO";  
      }
    }
  }
  
  public static bool DemoLimitReached(int demoModeLimit, int itemsScripted)
  {
    return demoModeLimit <= 0 || itemsScripted < demoModeLimit;
  }
  
  public static string CreateSafeFileName(string outputPath, string scriptName)
  {
    var safeFileName = string.Concat(scriptName.Split(Path.GetInvalidFileNameChars()));
    var fileName = Path.Combine(outputPath, $"{safeFileName}.sql");
    return fileName;
  }
}