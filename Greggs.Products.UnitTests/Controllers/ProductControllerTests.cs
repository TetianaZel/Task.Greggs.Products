using System;
using Greggs.Products.Api.Controllers;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Greggs.Products.UnitTests.Controllers;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _service = new(MockBehavior.Strict);

    private ProductController CreateSut() => new(_service.Object, NullLogger<ProductController>.Instance);

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
    public void Get_InvalidArgument_Returns400_WithMessage()
    {
        _service
            .Setup(s => s.GetProducts(It.IsAny<int>(), It.IsAny<int>(), "ZZZ"))
            .Throws(new ArgumentException("bad currency"));

        var action = CreateSut().Get(0, 5, "ZZZ");

        var bad = Assert.IsType<BadRequestObjectResult>(action.Result);
        Assert.Equal("bad currency", bad.Value);
    }
}