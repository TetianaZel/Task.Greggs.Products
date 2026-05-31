using System;
using System.Collections.Generic;
using Greggs.Products.Api.Services.Currency;
using Microsoft.Extensions.Options;
using Xunit;

namespace Greggs.Products.UnitTests.Services.Currency;

public class FixedRateCurrencyConverterTests
{
    private static FixedRateCurrencyConverter CreateSut(decimal gbpToEur = 1.11m) =>
        new(Options.Create(new CurrencyOptions
        {
            BaseCurrency = "GBP",
            ExchangeRates = new Dictionary<string, decimal> { ["EUR"] = gbpToEur }
        }));

    [Fact]
    public void Convert_SameCurrency_ReturnsSameAmount()
        => Assert.Equal(1.00m, CreateSut().Convert(1m, "GBP", "GBP"));

    [Theory]
    [InlineData(1.00, 1.11)]
    [InlineData(2.10, 2.33)]
    [InlineData(0.50, 0.56)]
    public void Convert_GbpToEur_AppliesConfiguredRate(decimal gbp, decimal expectedEur)
        => Assert.Equal(expectedEur, CreateSut().Convert(gbp, "GBP", "EUR"));

    [Fact]
    public void IsSupported_IsCaseInsensitive()
    {
        var sut = CreateSut();
        Assert.True(sut.IsSupported("gbp"));
        Assert.True(sut.IsSupported("eur"));
        Assert.False(sut.IsSupported("usd"));
        Assert.False(sut.IsSupported(""));
    }

    [Fact]
    public void Convert_UnsupportedCurrency_Throws()
        => Assert.Throws<ArgumentException>(() => CreateSut().Convert(1m, "GBP", "USD"));

    [Fact]
    public void Ctor_NonPositiveRate_Throws()
        => Assert.Throws<InvalidOperationException>(() => CreateSut(0m));
}