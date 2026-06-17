namespace Greggs.Products.Api.Models;

public readonly record struct Money(decimal Amount, Currency Currency)
{
    public override string ToString() => $"{Amount} {Currency.Code}";
}