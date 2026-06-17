using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Exceptions;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Greggs.Products.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IDataAccess<Product>> _dataAccess = new();
    private readonly Mock<ICurrencyConverter> _converter = new();
    private readonly IOptions<CurrencyOptions> _options =
        Options.Create(new CurrencyOptions { BaseCurrency = Currency.Gbp.Code });

    private ProductService CreateSut()
    {
        return new ProductService(_dataAccess.Object, _converter.Object, _options);
    }

    [Fact]
    public async Task GetProducts_PassesPagingToDataAccess()
    {
        _dataAccess.Setup(d => d.List(2, 3)).Returns(AsyncEnumerable.Empty<Product>());

        _ = (await CreateSut().GetProductsAsync(2, 3, Currency.Gbp.Code)).ToList();

        _dataAccess.Verify(d => d.List(2, 3), Times.Once);
    }

    [Fact]
    public async Task GetProducts_ConvertsPriceAndTagsCurrency()
    {
        _dataAccess.Setup(d => d.List(0, 5))
                   .Returns(new[] { new Product { Name = "Sausage Roll", Price = new Money(1m, Currency.Gbp) } }.ToAsyncEnumerable());
        _converter.Setup(c => c.ConvertAsync(new Money(1m, Currency.Gbp), Currency.Eur, It.IsAny<CancellationToken>())).ReturnsAsync(new Money(1.11m, Currency.Eur));

        var result = (await CreateSut().GetProductsAsync(0, 5, "eur")).Single();

        Assert.Equal("Sausage Roll", result.Name);
        Assert.Equal(1.11m, result.Price);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public async Task GetProducts_BaseCurrencyEur_StillConvertsFromGbp_NotFromConfiguredBase()
    {
        // BaseCurrency only sets the DEFAULT display/target currency. Prices are stored in GBP
        // (Product.Price.Currency), so conversion must always be FROM GBP - even when the base is EUR.
        var options = Options.Create(new CurrencyOptions { BaseCurrency = Currency.Eur.Code });
        _dataAccess.Setup(d => d.List(0, 5))
                   .Returns(new[] { new Product { Name = "Sausage Roll", Price = new Money(1m, Currency.Gbp) } }.ToAsyncEnumerable());
        _converter.Setup(c => c.ConvertAsync(new Money(1m, Currency.Gbp), Currency.Eur, It.IsAny<CancellationToken>())).ReturnsAsync(new Money(1.11m, Currency.Eur));

        var sut = new ProductService(_dataAccess.Object, _converter.Object, options);

        // No currency requested -> target defaults to the configured base (EUR).
        var result = (await sut.GetProductsAsync(0, 5, null)).Single();

        Assert.Equal(1.11m, result.Price);
        Assert.Equal("EUR", result.Currency);
        _converter.Verify(c => c.ConvertAsync(new Money(1m, Currency.Gbp), Currency.Eur, It.IsAny<CancellationToken>()), Times.Once);
        _converter.Verify(c => c.ConvertAsync(It.Is<Money>(m => m.Currency == Currency.Eur), It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetProducts_UnsupportedCurrency_ThrowsValidationException_AndShortCircuits()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() => CreateSut().GetProductsAsync(0, 5, "USD"));
        var expected = string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.CurrencyNotSupported, "USD");
        Assert.Equal(expected, ex.Message);

        _dataAccess.Verify(d => d.List(It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task GetProducts_NegativePageStart_ThrowsValidationException()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => CreateSut().GetProductsAsync(-1, 5, Currency.Gbp.Code));
        Assert.Equal(Constants.ErrorMessages.PageStartNegative, ex.Message);
    }

    [Fact]
    public async Task GetProducts_NegativePageSize_ThrowsValidationException()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => CreateSut().GetProductsAsync(0, -1, Currency.Gbp.Code));
        Assert.Equal(Constants.ErrorMessages.PageSizeNegative, ex.Message);
    }
}