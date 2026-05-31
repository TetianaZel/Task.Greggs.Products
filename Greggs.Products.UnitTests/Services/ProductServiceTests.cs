using System;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Greggs.Products.Api.Services.Currency;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Greggs.Products.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IDataAccess<Product>> _dataAccess = new(MockBehavior.Strict);
    private readonly Mock<ICurrencyConverter> _converter = new(MockBehavior.Strict);
    private readonly Mock<IOptions<CurrencyOptions>> _options = new(MockBehavior.Strict);

    private ProductService CreateSut() => new(_dataAccess.Object, _converter.Object, _options.Object);

    [Fact]
    public void GetProducts_PassesPagingToDataAccess()
    {
        _dataAccess.Setup(d => d.List(2, 3)).Returns(Array.Empty<Product>()).Verifiable();
        _converter.Setup(c => c.IsSupported("GBP")).Returns(true);
        _options.Setup(o => o.Value).Returns(new CurrencyOptions { BaseCurrency = "GBP" });

        _ = CreateSut().GetProducts(2, 3, "GBP").ToList();

        _dataAccess.Verify(d => d.List(2, 3), Times.Once);
    }

    [Fact]
    public void GetProducts_ConvertsPriceAndTagsCurrency()
    {
        _dataAccess
            .Setup(d => d.List(0, 5))
            .Returns(new[] { new Product { Name = "Sausage Roll", PriceInPounds = 1m } });
        _converter.Setup(c => c.IsSupported("EUR")).Returns(true);
        _options.Setup(o => o.Value).Returns(new CurrencyOptions { BaseCurrency = "GBP" });
        _converter.Setup(c => c.Convert(1m, "GBP", "EUR")).Returns(1.11m);

        var result = CreateSut().GetProducts(0, 5, "eur").Single();

        Assert.Equal("Sausage Roll", result.Name);
        Assert.Equal(1.11m, result.Price);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public void GetProducts_DefaultsToGbpWhenCurrencyMissing()
    {
        _dataAccess
            .Setup(d => d.List(null, null))
            .Returns(new[] { new Product { Name = "Yum Yum", PriceInPounds = 0.70m } });
        _options.Setup(o => o.Value).Returns(new CurrencyOptions { BaseCurrency = "GBP" });
        _converter.Setup(c => c.IsSupported("GBP")).Returns(true);
        _converter.Setup(c => c.Convert(0.70m, "GBP", "GBP")).Returns(0.70m);

        var result = CreateSut().GetProducts(null, null, null).Single();

        Assert.Equal("GBP", result.Currency);
        Assert.Equal(0.70m, result.Price);
    }

    [Fact]
    public void GetProducts_UnsupportedCurrency_Throws()
    {
        _converter.Setup(c => c.IsSupported("USD")).Returns(false);
        _options.Setup(o => o.Value).Returns(new CurrencyOptions { BaseCurrency = "GBP" });
        var ex = Assert.Throws<ArgumentException>(() => CreateSut().GetProducts(0, 5, "USD").ToList());
        Assert.Contains("USD", ex.Message);
        _dataAccess.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(0, -1)]
    public void GetProducts_NegativePaging_Throws(int pageStart, int pageSize)
    {
        _options.Setup(o => o.Value).Returns(new CurrencyOptions { BaseCurrency = "GBP" });
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateSut().GetProducts(pageStart, pageSize, "GBP").ToList());

        _dataAccess.VerifyNoOtherCalls();
        _converter.VerifyNoOtherCalls();
    }
}