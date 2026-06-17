using System.Threading;
using System.Threading.Tasks;
using Currency = Greggs.Products.Api.Models.Currency;
using Money = Greggs.Products.Api.Models.Money;

namespace Greggs.Products.Api.Services;

public interface ICurrencyConverter
{
    ValueTask<Money> ConvertAsync(Money amount, Currency to, CancellationToken cancellationToken = default);
}