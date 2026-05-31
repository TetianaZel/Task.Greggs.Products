using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api;
using Greggs.Products.Api.Controllers;
using Greggs.Products.Api.Exceptions;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Greggs.Products.UnitTests.Controllers;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _service = new(MockBehavior.Strict);

    private ProductController CreateSut()
    {
        return new ProductController(_service.Object);
    }

    [Fact]
    public async Task Get_DelegatesToService_AndReturnsOk()
    {
        var expected = new[] { new ProductDto { Name = "Sausage Roll", Price = 1m, Currency = "GBP" } };
        _service
            .Setup(s => s.GetProductsAsync(0, 5, "EUR", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var action = await CreateSut().Get(0, 5, "EUR");

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Same(expected, ok.Value);
        _service.Verify(s => s.GetProductsAsync(0, 5, "EUR", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_UsesDefaultQueryValues_WhenNoneSupplied()
    {
        _service
            .Setup(s => s.GetProductsAsync(
                Constants.Defaults.PageStart,
                Constants.Defaults.PageSize,
                Constants.Defaults.Currency,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Array.Empty<ProductDto>());

        var action = await CreateSut().Get();

        Assert.IsType<OkObjectResult>(action.Result);
        _service.Verify(
            s => s.GetProductsAsync(
                Constants.Defaults.PageStart,
                Constants.Defaults.PageSize,
                Constants.Defaults.Currency,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Get_WhenServiceThrowsValidationException_PropagatesToMiddleware()
    {
        var expectedMessage = string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.CurrencyNotSupported, "ZZZ");
        _service
            .Setup(s => s.GetProductsAsync(0, 5, "ZZZ", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(expectedMessage));

        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.Get(0, 5, "ZZZ"));
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task Get_WhenServiceThrowsUnexpectedException_PropagatesToMiddleware()
    {
        _service
            .Setup(s => s.GetProductsAsync(0, 5, "GBP", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.InvalidOperationException("data layer offline"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<System.InvalidOperationException>(() => sut.Get(0, 5, "GBP"));
    }
}