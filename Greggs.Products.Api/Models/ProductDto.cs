namespace Greggs.Products.Api.Models;

/// <summary>API response model for a product, including the currency the price is expressed in.</summary>
public class ProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
}