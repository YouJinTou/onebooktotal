using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OBT.Core.Abstractions;
using OBT.Core.Config;
using OBT.Core.DAL.Models.Mongo;
using OBT.Core.DAL.Repositories;
using OBT.Core.Scrapers;
using OBT.Core.Services;
using System.IO;

namespace OBT.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var serviceProvider = new ServiceCollection()
                .Configure<DatabaseSettings>(config.GetSection("databaseSettings"))
                .AddSingleton(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value)
                .AddHttpClient()
                .AddLogging(lb => lb.AddConsole())
                .AddTransient<IHttpService, HttpService>()
                .AddTransient<IRepository<DbBook>, Repository<DbBook>>()
                .BuildServiceProvider();
            var httpService = serviceProvider.GetService<IHttpService>();
            var books = serviceProvider.GetService<IRepository<DbBook>>();
            var logger = serviceProvider.GetService<ILogger<BookogsScraper>>();
            var bookogs = new BookogsScraper(httpService, books, logger);

            bookogs.ScrapeAsync().Wait();
        }
    }
}
