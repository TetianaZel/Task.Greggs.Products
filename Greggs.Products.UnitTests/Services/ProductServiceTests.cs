using System;
using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Greggs.Products.Api.Services.Currency;
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

    private sealed class FakeConverter : ICurrencyConverter
    {
        public decimal Rate = 1m;
        public bool SupportAll = true;
        public bool IsSupported(string c) => SupportAll;
        public decimal Convert(decimal amount, string from, string to) => Math.Round(amount * Rate, 2);
    }

    [Fact]
    public void GetProducts_PassesPagingToDataAccess()
    {
        var data = new FakeDataAccess();
        var sut = new ProductService(data, new FakeConverter());

        _ = sut.GetProducts(2, 3, "GBP").ToList();

        Assert.Equal(2, data.LastPageStart);
        Assert.Equal(3, data.LastPageSize);
    }

    [Fact]
    public void GetProducts_ConvertsPriceAndTagsCurrency()
    {
        var data = new FakeDataAccess
        {
            Result = new[] { new Product { Name = "Sausage Roll", PriceInPounds = 1m } }
        };
        var sut = new ProductService(data, new FakeConverter { Rate = 1.11m });

        var result = sut.GetProducts(0, 5, "eur").Single();

        Assert.Equal("Sausage Roll", result.Name);
        Assert.Equal(1.11m, result.Price);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public void GetProducts_DefaultsToGbpWhenCurrencyMissing()
    {
        var data = new FakeDataAccess
        {
            Result = new[] { new Product { Name = "Yum Yum", PriceInPounds = 0.70m } }
        };
        var sut = new ProductService(data, new FakeConverter { Rate = 1m });

        var result = sut.GetProducts(null, null, null).Single();

        Assert.Equal("GBP", result.Currency);
        Assert.Equal(0.70m, result.Price);
    }

    [Fact]
    public void GetProducts_UnsupportedCurrency_Throws()
    {
        var sut = new ProductService(new FakeDataAccess(), new FakeConverter { SupportAll = false });
        Assert.Throws<ArgumentException>(() => sut.GetProducts(0, 5, "USD").ToList());
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(0, -1)]
    public void GetProducts_NegativePaging_Throws(int pageStart, int pageSize)
    {
        var sut = new ProductService(new FakeDataAccess(), new FakeConverter());
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetProducts(pageStart, pageSize, "GBP").ToList());
    }
}