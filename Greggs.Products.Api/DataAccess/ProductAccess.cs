using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Greggs.Products.Api.Models;

namespace Greggs.Products.Api.DataAccess;

/// <summary>
/// DISCLAIMER: This is only here to help enable the purpose of this exercise, this doesn't reflect the way we work!
/// </summary>
public class ProductAccess : IDataAccess<Product>
{
    private static readonly IEnumerable<Product> ProductDatabase = new List<Product>()
    {
        new() { Name = "Sausage Roll", Price = new Money(1m, Currency.Gbp) },
        new() { Name = "Vegan Sausage Roll", Price = new Money(1.1m, Currency.Gbp) },
        new() { Name = "Steak Bake", Price = new Money(1.2m, Currency.Gbp) },
        new() { Name = "Yum Yum", Price = new Money(0.7m, Currency.Gbp) },
        new() { Name = "Pink Jammie", Price = new Money(0.5m, Currency.Gbp) },
        new() { Name = "Mexican Baguette", Price = new Money(2.1m, Currency.Gbp) },
        new() { Name = "Bacon Sandwich", Price = new Money(1.95m, Currency.Gbp) },
        new() { Name = "Coca Cola", Price = new Money(1.2m, Currency.Gbp) }
    };

    public async IAsyncEnumerable<Product> List(int? pageStart, int? pageSize)
    {
        var queryable = ProductDatabase.AsQueryable();

        if (pageStart.HasValue)
            queryable = queryable.Skip(pageStart.Value);

        if (pageSize.HasValue)
            queryable = queryable.Take(pageSize.Value);

        foreach (var product in queryable)
        {
            yield return product;
        }

        await Task.CompletedTask;
    }
}