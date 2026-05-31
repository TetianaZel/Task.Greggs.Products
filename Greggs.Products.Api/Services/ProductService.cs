using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Exceptions;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services.Currency;
using Microsoft.Extensions.Options;

namespace Greggs.Products.Api.Services;

public class ProductService : IProductService
{
    private readonly IDataAccess<Product> _productData;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly string _sourceCurrency;

    public ProductService(
        IDataAccess<Product> productData,
        ICurrencyConverter currencyConverter,
        IOptions<CurrencyOptions> options)
    {
        _productData = productData ?? throw new ArgumentNullException(nameof(productData));
        _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));
        _sourceCurrency = options.Value.BaseCurrency;
    }

    public IEnumerable<ProductDto> GetProducts(int? pageStart, int? pageSize, string currency)
    {
        if (pageStart is < 0)
        {
            throw new ValidationException(Constants.ErrorMessages.PageStartNegative);
        }

        if (pageSize is < 0)
        {
            throw new ValidationException(Constants.ErrorMessages.PageSizeNegative);
        }

        var targetCurrency = string.IsNullOrWhiteSpace(currency)
            ? _sourceCurrency
            : currency.Trim().ToUpperInvariant();

        if (!_currencyConverter.IsSupported(targetCurrency))
        {
            throw new ValidationException(string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.CurrencyNotSupported, targetCurrency));
        }

        var products = _productData.List(pageStart, pageSize) ?? Enumerable.Empty<Product>();

        return products.Select(p => new ProductDto
        {
            Name = p.Name,
            Price = _currencyConverter.Convert(p.PriceInPounds, _sourceCurrency, targetCurrency),
            Currency = targetCurrency
        }).ToList();
    }
}