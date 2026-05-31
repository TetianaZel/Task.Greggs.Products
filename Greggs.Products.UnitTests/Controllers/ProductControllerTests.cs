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
    public void Get_DelegatesToService_AndReturnsOk()
    {
        var expected = new[] { new ProductDto { Name = "Sausage Roll", Price = 1m, Currency = "GBP" } };
        _service
            .Setup(s => s.GetProducts(0, 5, "EUR"))
            .Returns(expected);

        var action = CreateSut().Get(0, 5, "EUR");

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Same(expected, ok.Value);
        _service.Verify(s => s.GetProducts(0, 5, "EUR"), Times.Once);
    }

    [Fact]
    public void Get_UsesDefaultQueryValues_WhenNoneSupplied()
    {
        _service
            .Setup(s => s.GetProducts(
                Constants.Defaults.PageStart,
                Constants.Defaults.PageSize,
                Constants.Defaults.Currency))
            .Returns(System.Array.Empty<ProductDto>());

        var action = CreateSut().Get();

        Assert.IsType<OkObjectResult>(action.Result);
        _service.Verify(
            s => s.GetProducts(
                Constants.Defaults.PageStart,
                Constants.Defaults.PageSize,
                Constants.Defaults.Currency),
            Times.Once);
    }

    [Fact]
    public void Get_WhenServiceThrowsValidationException_PropagatesToMiddleware()
    {
        _service
            .Setup(s => s.GetProducts(0, 5, "ZZZ"))
            .Throws(new ValidationException("Currency 'ZZZ' is not supported."));

        var sut = CreateSut();

        var ex = Assert.Throws<ValidationException>(() => sut.Get(0, 5, "ZZZ"));
        Assert.Equal("Currency 'ZZZ' is not supported.", ex.Message);
    }

    [Fact]
    public void Get_WhenServiceThrowsUnexpectedException_PropagatesToMiddleware()
    {
        _service
            .Setup(s => s.GetProducts(0, 5, "GBP"))
            .Throws(new System.InvalidOperationException("data layer offline"));

        var sut = CreateSut();

        Assert.Throws<System.InvalidOperationException>(() => sut.Get(0, 5, "GBP"));
    }
}