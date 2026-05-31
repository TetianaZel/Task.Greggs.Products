using System.Collections.Generic;
using Greggs.Products.Api.Controllers;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Greggs.Products.UnitTests.Controllers;

public class ProductControllerTests
{
    private sealed class StubService : IProductService
    {
        public int? PageStart;
        public int? PageSize;
        public IEnumerable<ProductDto> Result = new[] { new ProductDto { Name = "X", Price = 1m } };

        public IEnumerable<ProductDto> GetProducts(int? pageStart, int? pageSize)
        {
            PageStart = pageStart; PageSize = pageSize;
            return Result;
        }
    }

    [Fact]
    public void Get_ReturnsOkWithProductsFromService()
    {
        var svc = new StubService();
        var sut = new ProductController(svc, NullLogger<ProductController>.Instance);

        var action = sut.Get(0, 5);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value);
        Assert.Single(products);
        Assert.Equal(0, svc.PageStart);
        Assert.Equal(5, svc.PageSize);
    }
}