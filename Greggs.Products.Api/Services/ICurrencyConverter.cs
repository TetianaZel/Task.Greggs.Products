using System.Threading;
using System.Threading.Tasks;
using Currency = Greggs.Products.Api.Models.Currency;

namespace Greggs.Products.Api.Services;

public interface ICurrencyConverter
{
    Task<decimal> ConvertAsync(decimal amount, Currency from, Currency to, CancellationToken cancellationToken = default);
}