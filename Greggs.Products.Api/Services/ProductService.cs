using System;
using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services.Currency;

namespace Greggs.Products.Api.Services;

public class ProductService : IProductService
{
    private const string SourceCurrency = "GBP";

    private readonly IDataAccess<Product> _productData;
    private readonly ICurrencyConverter _currencyConverter;

    public ProductService(IDataAccess<Product> productData, ICurrencyConverter currencyConverter)
    {
        _productData = productData ?? throw new ArgumentNullException(nameof(productData));
        _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));
    }

    public IEnumerable<ProductDto> GetProducts(int? pageStart, int? pageSize, string currency)
    {
        if (pageStart is < 0) throw new ArgumentOutOfRangeException(nameof(pageStart));
        if (pageSize is < 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var targetCurrency = string.IsNullOrWhiteSpace(currency)
            ? SourceCurrency
            : currency.Trim().ToUpperInvariant();

        if (!_currencyConverter.IsSupported(targetCurrency))
            throw new ArgumentException($"Currency '{targetCurrency}' is not supported.", nameof(currency));

        var products = _productData.List(pageStart, pageSize) ?? Enumerable.Empty<Product>();

        return products.Select(p => new ProductDto
        {
            Name = p.Name,
            Price = _currencyConverter.Convert(p.PriceInPounds, SourceCurrency, targetCurrency),
            Currency = targetCurrency
        }).ToList();
    }
}