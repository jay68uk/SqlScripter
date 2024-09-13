using System.Collections.Specialized;
using System.Linq;

namespace SqlSpScaffolder.App;

internal class ScriptUtilities
{
  public static string JoinScriptParts(StringCollection scriptParts)
  {
    return string.Join(Environment.NewLine, ConvertToEnumerable(scriptParts));
  }

  private static IEnumerable<string?> ConvertToEnumerable(StringCollection scriptParts)
  {
    foreach (var scriptPart in scriptParts)
    {
      yield return scriptPart;
    }
  }
}