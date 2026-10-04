using System.Text.RegularExpressions;

namespace Evently.Modules.Events.Domain.TicketTypes;

internal static partial class TicketTypeDesign
{
    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColorRegex();

    public static bool IsValidHexColor(string value) => HexColorRegex().IsMatch(value);
}
