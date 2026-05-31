using System;
using System.Globalization;
using System.Linq;
using Greggs.Products.Api;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Exceptions;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Greggs.Products.Api.Services.Currency;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Greggs.Products.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IDataAccess<Product>> _dataAccess = new();
    private readonly Mock<ICurrencyConverter> _converter = new();
    private readonly IOptions<CurrencyOptions> _options =
        Options.Create(new CurrencyOptions { BaseCurrency = Constants.Defaults.Currency });

    private ProductService CreateSut()
    {
        return new ProductService(_dataAccess.Object, _converter.Object, _options);
    }

    [Fact]
    public void GetProducts_PassesPagingToDataAccess()
    {
        _dataAccess.Setup(d => d.List(2, 3)).Returns(Array.Empty<Product>());
        _converter.Setup(c => c.IsSupported(Constants.Defaults.Currency)).Returns(true);

        _ = CreateSut().GetProducts(2, 3, Constants.Defaults.Currency).ToList();

        _dataAccess.Verify(d => d.List(2, 3), Times.Once);
    }

    [Fact]
    public void GetProducts_ConvertsPriceAndTagsCurrency()
    {
        _dataAccess.Setup(d => d.List(0, 5))
                   .Returns(new[] { new Product { Name = "Sausage Roll", PriceInPounds = 1m } });
        _converter.Setup(c => c.IsSupported("EUR")).Returns(true);
        _converter.Setup(c => c.Convert(1m, Constants.Defaults.Currency, "EUR")).Returns(1.11m);

        var result = CreateSut().GetProducts(0, 5, "eur").Single();

        Assert.Equal("Sausage Roll", result.Name);
        Assert.Equal(1.11m, result.Price);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public void GetProducts_UnsupportedCurrency_ThrowsValidationException_AndShortCircuits()
    {
        _converter.Setup(c => c.IsSupported("USD")).Returns(false);

        var ex = Assert.Throws<ValidationException>(() => CreateSut().GetProducts(0, 5, "USD").ToList());
        var expected = string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.CurrencyNotSupported, "USD");
        Assert.Equal(expected, ex.Message);

        _dataAccess.Verify(d => d.List(It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public void GetProducts_NegativePageStart_ThrowsValidationException()
    {
        var ex = Assert.Throws<ValidationException>(
            () => CreateSut().GetProducts(-1, 5, Constants.Defaults.Currency).ToList());
        Assert.Equal(Constants.ErrorMessages.PageStartNegative, ex.Message);
    }

    [Fact]
    public void GetProducts_NegativePageSize_ThrowsValidationException()
    {
        var ex = Assert.Throws<ValidationException>(
            () => CreateSut().GetProducts(0, -1, Constants.Defaults.Currency).ToList());
        Assert.Equal(Constants.ErrorMessages.PageSizeNegative, ex.Message);
    }
}