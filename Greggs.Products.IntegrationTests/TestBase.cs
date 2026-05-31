using Greggs.Products.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Greggs.Products.IntegrationTests
{
    public class TestBase : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Currency:BaseCurrency"] = "GBP",
                    ["Currency:ExchangeRates:EUR"] = "1.11"
                });
            });
        }
    }
}
