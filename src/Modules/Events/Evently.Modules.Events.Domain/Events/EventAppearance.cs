using System.Text.RegularExpressions;

namespace Evently.Modules.Events.Domain.Events;

internal static partial class EventAppearance
{
    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColorRegex();

    public static bool IsValidHexColor(string value) => HexColorRegex().IsMatch(value);
}
