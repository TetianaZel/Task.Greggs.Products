using System;
using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Xunit;

namespace Greggs.Products.UnitTests.Services;

public class ProductServiceTests
{
    private sealed class FakeDataAccess : IDataAccess<Product>
    {
        public int? LastPageStart;
        public int? LastPageSize;
        public IEnumerable<Product> Result = Array.Empty<Product>();

        public IEnumerable<Product> List(int? pageStart, int? pageSize)
        {
            LastPageStart = pageStart;
            LastPageSize = pageSize;
            return Result;
        }
    }

    [Fact]
    public void GetProducts_PassesPagingToDataAccess()
    {
        var data = new FakeDataAccess();
        var sut = new ProductService(data);

        _ = sut.GetProducts(2, 3).ToList();

        Assert.Equal(2, data.LastPageStart);
        Assert.Equal(3, data.LastPageSize);
    }

    [Fact]
    public void GetProducts_MapsDomainToDto()
    {
        var data = new FakeDataAccess
        {
            Result = new[] { new Product { Name = "Sausage Roll", PriceInPounds = 1m } }
        };
        var sut = new ProductService(data);

        var result = sut.GetProducts(0, 5).Single();

        Assert.Equal("Sausage Roll", result.Name);
        Assert.Equal(1m, result.Price);
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(0, -1)]
    public void GetProducts_NegativePaging_Throws(int pageStart, int pageSize)
    {
        var sut = new ProductService(new FakeDataAccess());
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetProducts(pageStart, pageSize).ToList());
    }
}