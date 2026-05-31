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
}