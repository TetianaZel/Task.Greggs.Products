using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Greggs.Products.Api.Services.Currency;

/// <summary>Currency converter backed by a fixed rate table loaded from configuration.</summary>
public class FixedRateCurrencyConverter : ICurrencyConverter
{
    private readonly string _baseCurrency;
    private readonly IDictionary<string, decimal> _rates;

    public FixedRateCurrencyConverter(IOptions<CurrencyOptions> options)
    {
        if (options?.Value == null) throw new ArgumentNullException(nameof(options));

        _baseCurrency = options.Value.BaseCurrency
            ?? throw new InvalidOperationException("BaseCurrency must be configured.");

        _rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            [_baseCurrency] = 1m
        };

        if (options.Value.ExchangeRates != null)
        {
            foreach (var kvp in options.Value.ExchangeRates)
            {
                if (kvp.Value <= 0)
                    throw new InvalidOperationException(
                        $"Exchange rate for '{kvp.Key}' must be greater than zero.");
                _rates[kvp.Key] = kvp.Value;
            }
        }
    }

    public bool IsSupported(string currencyCode)
        => !string.IsNullOrWhiteSpace(currencyCode) && _rates.ContainsKey(currencyCode);

    public decimal Convert(decimal amount, string fromCurrency, string toCurrency)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency)) throw new ArgumentException("Currency code required.", nameof(fromCurrency));
        if (string.IsNullOrWhiteSpace(toCurrency)) throw new ArgumentException("Currency code required.", nameof(toCurrency));

        if (!_rates.TryGetValue(fromCurrency, out var fromRate))
            throw new ArgumentException($"Unsupported currency '{fromCurrency}'.", nameof(fromCurrency));
        if (!_rates.TryGetValue(toCurrency, out var toRate))
            throw new ArgumentException($"Unsupported currency '{toCurrency}'.", nameof(toCurrency));

        var amountInBase = amount / fromRate;
        var converted = amountInBase * toRate;

        return Math.Round(converted, 2, MidpointRounding.ToEven);
    }
}