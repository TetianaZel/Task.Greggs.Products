using System.Net;
using System.Net.Http.Json;
using Greggs.Products.Api.Models;

namespace Greggs.Products.IntegrationTests;

/// <summary>
/// End-to-end tests that boot the real ASP.NET Core pipeline in-memory
/// (routing, DI, model binding, JSON serialisation, configuration).
/// </summary>
public class ProductControllerTests : IClassFixture<TestBase>
{
    private readonly TestBase _factory;

    public ProductControllerTests(TestBase factory) => _factory = factory;

    [Fact]
    public async Task Get_DefaultsToGbp_AndReturnsProducts()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/product?pageSize=3");

        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

        Assert.NotNull(products);
        Assert.Equal(3, products!.Count);
        Assert.All(products, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.True(p.Price > 0m);
            Assert.Equal("GBP", p.Currency);
        });
    }

    [Fact]
    public async Task Get_WithEur_AppliesConfiguredRate()
    {
        var client = _factory.CreateClient();

        // First product in the (fake) data layer is the Sausage Roll @ £1.00
        var response = await client.GetAsync("/product?pageStart=0&pageSize=1&currency=EUR");

        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

        var sausageRoll = Assert.Single(products!);
        Assert.Equal("Sausage Roll", sausageRoll.Name);
        Assert.Equal("EUR", sausageRoll.Currency);
        Assert.Equal(1.11m, sausageRoll.Price); // 1.00 GBP * 1.11
    }

    [Theory]
    [InlineData("gbp", "GBP")]
    [InlineData("eur", "EUR")]
    [InlineData("EUR", "EUR")]
    public async Task Get_CurrencyQueryParam_IsCaseInsensitiveAndNormalised(string input, string expected)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/product?pageSize=1&currency={input}");

        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        Assert.Equal(expected, products!.Single().Currency);
    }

    [Fact]
    public async Task Get_UnknownCurrency_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/product?currency=ZZZ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_RespectsPaging()
    {
        var client = _factory.CreateClient();

        var firstPage = await client.GetFromJsonAsync<List<ProductDto>>("/product?pageStart=0&pageSize=2");
        var secondPage = await client.GetFromJsonAsync<List<ProductDto>>("/product?pageStart=2&pageSize=2");

        Assert.Equal(2, firstPage!.Count);
        Assert.Equal(2, secondPage!.Count);
        Assert.NotEqual(firstPage[0].Name, secondPage[0].Name);
    }

    [Fact]
    public async Task Get_ResponseIsJson()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/product");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}