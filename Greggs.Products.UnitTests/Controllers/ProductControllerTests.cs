using System;
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
        public string Currency;
        public IEnumerable<ProductDto> Result = new[] { new ProductDto { Name = "X", Price = 1m, Currency = "GBP" } };
        public Exception ToThrow;

        public IEnumerable<ProductDto> GetProducts(int? pageStart, int? pageSize, string currency)
        {
            PageStart = pageStart; PageSize = pageSize; Currency = currency;
            if (ToThrow != null) throw ToThrow;
            return Result;
        }
    }

    [Fact]
    public void Get_ReturnsOkWithProductsFromService()
    {
        var svc = new StubService();
        var sut = new ProductController(svc, NullLogger<ProductController>.Instance);

        var action = sut.Get(0, 5, "EUR");

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value);
        Assert.Equal("EUR", svc.Currency);
    }

    [Fact]
    public void Get_InvalidArgument_Returns400()
    {
        var svc = new StubService { ToThrow = new ArgumentException("bad currency") };
        var sut = new ProductController(svc, NullLogger<ProductController>.Instance);

        var action = sut.Get(0, 5, "ZZZ");

        var bad = Assert.IsType<BadRequestObjectResult>(action.Result);
        Assert.Equal("bad currency", bad.Value);
    }
}