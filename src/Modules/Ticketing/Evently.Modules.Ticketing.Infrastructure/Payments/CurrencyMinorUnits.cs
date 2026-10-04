namespace Evently.Modules.Ticketing.Infrastructure.Payments;

/// <summary>
/// Converts major currency amounts into Stripe's minor units.
/// See https://docs.stripe.com/currencies#zero-decimal for the special cases.
/// </summary>
internal static class CurrencyMinorUnits
{
    private static readonly HashSet<string> ZeroDecimalCurrencies =
    [
        "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA",
        "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF"
    ];

    private static readonly HashSet<string> ThreeDecimalCurrencies = ["BHD", "JOD", "KWD", "OMR", "TND"];

    public static long ToMinorUnits(decimal amount, string currency)
    {
        string normalizedCurrency = currency.ToUpperInvariant();

        decimal factor = normalizedCurrency switch
        {
            var c when ThreeDecimalCurrencies.Contains(c) => 1000m,
            var c when ZeroDecimalCurrencies.Contains(c) => 1m,
            _ => 100m
        };

        return (long)Math.Round(amount * factor, MidpointRounding.AwayFromZero);
    }
}
