namespace Greggs.Products.Api.Models;

public readonly record struct Currency(string Code)
{
    public static readonly Currency Gbp = new("GBP");
    public static readonly Currency Eur = new("EUR");

    public static bool TryParse(string code, out Currency currency)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            switch (code.Trim().ToUpperInvariant())
            {
                case "GBP":
                    currency = Gbp;
                    return true;
                case "EUR":
                    currency = Eur;
                    return true;
            }
        }

        currency = default;
        return false;
    }

    public override string ToString() => Code;
}