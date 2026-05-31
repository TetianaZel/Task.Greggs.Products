namespace Greggs.Products.Api.Services.Currency;

public interface ICurrencyConverter
{
    bool IsSupported(string currencyCode);
    decimal Convert(decimal amount, string fromCurrency, string toCurrency);
}