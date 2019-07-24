using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                .AddLogging(lb => lb.AddConsole())
                .AddTransient<IHttpService, HttpService>()
                .BuildServiceProvider();
            var httpService = serviceProvider.GetService<IHttpService>();
            var logger = serviceProvider.GetService<ILogger<BookogsScraper>>();
            var bookogs = new BookogsScraper(httpService, logger);

            bookogs.ScrapeAsync().Wait();
        }
    }
}
