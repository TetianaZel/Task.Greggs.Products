using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Greggs.Products.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    /// <summary>Gets the latest list of products, with prices in the requested currency (default GBP).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Get([FromQuery] int pageStart = Constants.Defaults.PageStart, [FromQuery] int pageSize = Constants.Defaults.PageSize,
        [FromQuery] string currency = Constants.Defaults.Currency, CancellationToken cancellationToken = default)
    {
        return Ok(await _productService.GetProductsAsync(pageStart, pageSize, currency, cancellationToken));
    }
}