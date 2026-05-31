using System;
using System.Collections.Generic;

namespace Greggs.Products.Api.Services.Currency;

public class CurrencyOptions
{
    public const string SectionName = "Currency";

    public string BaseCurrency { get; set; } = "GBP";

    public IDictionary<string, decimal> ExchangeRates { get; set; }
        = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
}