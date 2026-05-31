using System.Collections.Generic;
using Greggs.Products.Api.Models;

namespace Greggs.Products.Api.Services;

public interface IProductService
{
    /// <summary>
    /// Returns the latest menu of products, optionally paged, with prices converted to <paramref name="currency"/>.
    /// </summary>
    IEnumerable<ProductDto> GetProducts(int? pageStart, int? pageSize, string currency);
}