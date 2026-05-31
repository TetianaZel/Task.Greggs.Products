using System;
using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;

namespace Greggs.Products.Api.Services;

public class ProductService : IProductService
{
    private readonly IDataAccess<Product> _productData;

    public ProductService(IDataAccess<Product> productData)
    {
        _productData = productData ?? throw new ArgumentNullException(nameof(productData));
    }

    public IEnumerable<ProductDto> GetProducts(int? pageStart, int? pageSize)
    {
        if (pageStart is < 0) throw new ArgumentOutOfRangeException(nameof(pageStart));
        if (pageSize is < 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var products = _productData.List(pageStart, pageSize) ?? Enumerable.Empty<Product>();

        return products
            .Select(p => new ProductDto { Name = p.Name, Price = p.PriceInPounds })
            .ToList();
    }
}