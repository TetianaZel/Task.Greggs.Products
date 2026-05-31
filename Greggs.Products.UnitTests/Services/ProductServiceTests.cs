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
                   .Returns(new[] { new Product { Name = "Sausage Roll", PriceInPounds = 1m } }.ToAsyncEnumerable());
        _converter.Setup(c => c.ConvertAsync(1m, Currency.Gbp, Currency.Eur, It.IsAny<CancellationToken>())).ReturnsAsync(1.11m);

        var result = (await CreateSut().GetProductsAsync(0, 5, "eur")).Single();

        Assert.Equal("Sausage Roll", result.Name);
        Assert.Equal(1.11m, result.Price);
        Assert.Equal("EUR", result.Currency);
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