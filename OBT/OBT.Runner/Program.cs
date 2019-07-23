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
                .AddTransient<IRetryServcie, RetryService>()
                .BuildServiceProvider();
            var httpService = serviceProvider.GetService<IHttpService>();
            var retryService = serviceProvider.GetService<IRetryServcie>();
            var bookogs = new BookogsScraper(httpService, retryService);

            bookogs.ScrapeAsync().Wait();
        }
    }
}
