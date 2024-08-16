using System.Text.RegularExpressions;

namespace BridgeDataConsumer.Console.Regexes;

public partial class RegexContainer
{
    [GeneratedRegex(@"^(.+)\.(\d+)$")]
    public static partial Regex ExpectedFileNameRegex();
}
