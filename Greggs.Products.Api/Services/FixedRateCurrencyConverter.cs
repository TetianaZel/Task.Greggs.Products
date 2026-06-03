using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.Models;
using Microsoft.Extensions.Options;

namespace Greggs.Products.Api.Services;

/// <summary>Currency converter backed by a fixed rate table.</summary>
/// <remarks>
/// NOTE: Rates are hardcoded here for the purposes of this task only.
/// In production they would not live in source/config; they would be fetched
/// from a 3rd party FX provider (and likely cached/refreshed on a schedule).
/// </remarks>
public class FixedRateCurrencyConverter : ICurrencyConverter
{
    // Rates expressed relative to the base currency (1 unit of base => value units of target).
    // Hardcoded for the task only - replace with a 3rd party FX feed in production.
    private static readonly IReadOnlyDictionary<string, decimal> HardcodedRates =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            [Currency.Gbp.Code] = 1m,
            [Currency.Eur.Code] = 1.11m
        };

    private readonly Currency _baseCurrency;
    private readonly IDictionary<string, decimal> _rates;

    public FixedRateCurrencyConverter(IOptions<CurrencyOptions> options)
    {
        if (options?.Value == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (!Currency.TryParse(options.Value.BaseCurrency, out _baseCurrency))
        {
            throw new InvalidOperationException(Constants.ErrorMessages.BaseCurrencyInvalid);
        }

        _rates = new Dictionary<string, decimal>(HardcodedRates, StringComparer.OrdinalIgnoreCase)
        {
            [_baseCurrency.Code] = 1m
        };
    }

    public ValueTask<decimal> ConvertAsync(decimal amount, Currency from, Currency to, CancellationToken cancellationToken = default)
    {
        if (from == to)
        {
            return new ValueTask<decimal>(amount);
        }

        if (!_rates.TryGetValue(from.Code, out var fromRate))
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.UnsupportedCurrency, from.Code), nameof(from));
        }

        if (!_rates.TryGetValue(to.Code, out var toRate))
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.UnsupportedCurrency, to.Code), nameof(to));
        }

        if (fromRate <= 0m)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.InvalidExchangeRate, from.Code));
        }

        if (toRate < 0m)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.InvalidExchangeRate, to.Code));
        }

        var amountInBase = amount / fromRate;
        var converted = amountInBase * toRate;

        return new ValueTask<decimal>(Math.Round(converted, 2, MidpointRounding.ToEven));
    }
}