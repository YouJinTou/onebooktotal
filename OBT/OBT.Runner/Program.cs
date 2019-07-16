using Microsoft.Extensions.DependencyInjection;
using OBT.Core.Abstractions;
using OBT.Core.Scrapers;
using OBT.Core.Services;

namespace OBT.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddTransient<IHttpService, HttpService>()
                .BuildServiceProvider();
            var httpService = serviceProvider.GetService<IHttpService>();
            var bookogs = new BookogsScraper(httpService);

            bookogs.ScrapeAsync().Wait();
        }
    }
}
