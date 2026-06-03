using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace Greggs.Products.UnitTests.Services;

public class FixedRateCurrencyConverterTests
{
    private static FixedRateCurrencyConverter CreateSut() =>
        new(Options.Create(new CurrencyOptions { BaseCurrency = Currency.Gbp.Code }));

    [Fact]
    public async Task ConvertAsync_SameCurrency_ReturnsSameAmount()
        => Assert.Equal(1.00m, await CreateSut().ConvertAsync(1m, Currency.Gbp, Currency.Gbp));

    [Theory]
    [InlineData(1.00, 1.11)]
    [InlineData(2.10, 2.33)]
    [InlineData(0.50, 0.56)]
    public async Task ConvertAsync_GbpToEur_AppliesHardcodedRate(decimal gbp, decimal expectedEur)
        => Assert.Equal(expectedEur, await CreateSut().ConvertAsync(gbp, Currency.Gbp, Currency.Eur));

    [Fact]
    public async Task ConvertAsync_WhenFromRateIsZero_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();
        OverrideRate(sut, Currency.Eur.Code, 0m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await sut.ConvertAsync(1m, Currency.Eur, Currency.Gbp));
    }

    private static void OverrideRate(FixedRateCurrencyConverter sut, string currencyCode, decimal rate)
    {
        var field = typeof(FixedRateCurrencyConverter).GetField("_rates", BindingFlags.Instance | BindingFlags.NonPublic);
        var rates = (IDictionary<string, decimal>)field!.GetValue(sut)!;
        rates[currencyCode] = rate;
    }
}