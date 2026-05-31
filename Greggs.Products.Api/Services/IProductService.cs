using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.Models;

namespace Greggs.Products.Api.Services;

public interface IProductService
{
    /// <summary>
    /// Returns the latest menu of products, optionally paged, with prices converted to <paramref name="currency"/>.
    /// </summary>
    Task<IEnumerable<ProductDto>> GetProductsAsync(int? pageStart, int? pageSize, string currency, CancellationToken cancellationToken = default);
}