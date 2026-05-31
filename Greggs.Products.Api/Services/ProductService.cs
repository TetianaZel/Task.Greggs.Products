using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Exceptions;
using Greggs.Products.Api.Models;
using Microsoft.Extensions.Options;

namespace Greggs.Products.Api.Services;

public class ProductService : IProductService
{
    private readonly IDataAccess<Product> _productData;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly Currency _sourceCurrency;

    public ProductService(
        IDataAccess<Product> productData,
        ICurrencyConverter currencyConverter,
        IOptions<CurrencyOptions> options)
    {
        _productData = productData ?? throw new ArgumentNullException(nameof(productData));
        _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));

        if (!Currency.TryParse(options.Value.BaseCurrency, out _sourceCurrency))
        {
            throw new InvalidOperationException(Constants.ErrorMessages.BaseCurrencyInvalid);
        }
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(int? pageStart, int? pageSize, string currency, CancellationToken cancellationToken = default)
    {
        if (pageStart is < 0)
        {
            throw new ValidationException(Constants.ErrorMessages.PageStartNegative);
        }

        if (pageSize is < 0)
        {
            throw new ValidationException(Constants.ErrorMessages.PageSizeNegative);
        }

        Currency targetCurrency;
        if (string.IsNullOrWhiteSpace(currency))
        {
            targetCurrency = _sourceCurrency;
        }
        else if (!Currency.TryParse(currency, out targetCurrency))
        {
            throw new ValidationException(string.Format(CultureInfo.InvariantCulture, Constants.ErrorMessages.CurrencyNotSupported, currency));
        }

        var products = _productData.List(pageStart, pageSize) ?? Enumerable.Empty<Product>();

        var results = new List<ProductDto>();
        foreach (var p in products)
        {
            var price = await _currencyConverter.ConvertAsync(p.PriceInPounds, _sourceCurrency, targetCurrency, cancellationToken).ConfigureAwait(false);
            results.Add(new ProductDto
            {
                Name = p.Name,
                Price = price,
                Currency = targetCurrency.Code
            });
        }

        return results;
    }
}