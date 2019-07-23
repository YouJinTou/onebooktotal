using HtmlAgilityPack;
using OBT.Core.Abstractions;
using OBT.Core.Extensions;
using OBT.Core.Models;
using OBT.Core.Models.Bookogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBT.Core.Scrapers
{
    public class BookogsScraper : IScraper
    {
        private const string BaseUrl = "https://www.bookogs.com";
        private const string TableXPath = "//tr[td='{0}']/td[2]/a/text()";
        private static readonly string ApiUrl = $"{BaseUrl}/api/browse/book?page={{0}}";
        private readonly IHttpService httpService;
        private readonly IRetryServcie retryService;

        public BookogsScraper(IHttpService httpService, IRetryServcie retryService)
        {
            this.httpService = httpService;
            this.retryService = retryService;
        }

        public async Task<IEnumerable<Book>> ScrapeAsync()
        {
            var rootObjects = await this.GetRootObjectsAsync();
            var books = await this.GetBooksAsync(rootObjects);

            return books;
        }

        private class Comp : IEqualityComparer<Rootobject>
        {
            public bool Equals(Rootobject x, Rootobject y)
            {
                return (x.entities[0].title == y.entities[0].title);
            }

            public int GetHashCode(Rootobject obj)
            {
                return obj.entities[0].title.GetHashCode();
            }
        }

        private async Task<IEnumerable<Rootobject>> GetRootObjectsAsync()
        {
            try
            {
                var totalPages = Enumerable.Range(1, await this.GetTotalPages());
                var pageBatches = totalPages.ToBatches(300).Select(b => new { From = b.Min(), To = b.Max() });
                var rootObjects = new ConcurrentBag<Rootobject>();

                foreach (var pageBatch in pageBatches)
                {
                    var apiCallTasks = new List<Task>();

                    for (int p = pageBatch.From; p <= pageBatch.To; p++)
                    {
                        Console.WriteLine($"Page: {p}");

                        var task = Task.Run(async () =>
                        {
                            Func<Task> call = async () => await this.RunApiTaskAsync(rootObjects, p);

                            await this.retryService.RetryAsync(call);
                        });

                        apiCallTasks.Add(task);

                        await Task.Delay(1);
                    }

                    Task.WaitAll(apiCallTasks.ToArray());

                    Console.WriteLine($"Batch from {pageBatch.From} to {pageBatch.To}");

                    var x = rootObjects.Distinct(new Comp()).ToList();
                    await Task.Delay(100);
                }


                return rootObjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private async Task RunApiTaskAsync(ConcurrentBag<Rootobject> rootObjects, int page)
        {
            var url = string.Format(ApiUrl, page);

            Console.WriteLine($"Queueing up: {url}");

            var root = await this.httpService.GetAsync<Rootobject>(url);

            rootObjects.Add(root);

            Console.WriteLine("Added root.");
        }

        private async Task<int> GetTotalPages()
        {
            var root = await this.httpService.GetAsync<Rootobject>(string.Format(ApiUrl, 1));
            var booksPerPage = 40;
            var totalPages = (root.total / booksPerPage) + 1;

            return totalPages;
        }

        private async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Rootobject> rootObjects)
        {
            var books = new List<Book>();

            foreach (var root in rootObjects)
            {
                var scrapeTasks = new List<Task>();

                foreach (var entity in root.entities)
                {
                    var scrapeTask = Task.Run(async () =>
                    {
                        try
                        {
                            var web = new HtmlWeb();
                            var htmlDoc = await web.LoadFromWebAsync($"{BaseUrl}{entity.url}");
                            var node = htmlDoc.DocumentNode;
                            var book = new Book
                            {
                                Authors = this.GetValues(node, "Author"),
                                Format = this.GetValue(node, "Format"),
                                Genre = this.GetValue(node, "Genre"),
                                Isbn10 = this.GetIsbn10(node),
                                Isbn13 = this.GetIsbn13(node),
                                Language = this.GetValue(node, "Language"),
                                Pages = this.GetValue(node, "Listed Page Count").StripNonDigits(),
                                Title = this.GetTitle(entity, node),
                                Year = this.GetValue(node, "This Edition Published").ToYear()
                            };

                            books.Add(book);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    });

                    scrapeTasks.Add(scrapeTask);

                    await Task.Delay(10);
                }

                Task.WaitAll(scrapeTasks.ToArray());
            }

            return books;
        }

        private string GetValue(HtmlNode node, string key)
        {
            return node.SelectSingleNode(string.Format(TableXPath, key))?.InnerText;
        }

        private IEnumerable<string> GetValues(HtmlNode node, string key)
        {
            return node.SelectNodes(string.Format(TableXPath, key))?.Select(n => n.InnerText);
        }

        private string GetTitle(Entity entity, HtmlNode node)
        {
            if (!string.IsNullOrWhiteSpace(entity.title))
            {
                return entity.title;
            }

            var values = new string[]
            {
                node.SelectSingleNode(
                    "//div[@class='item-view-title']/h1/text()")?.InnerText?.Trim(),
                this.GetValue(node, "Title")
            };

            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }

        private string GetIsbn10(HtmlNode node)
        {
            var values = new string[]
            {
                this.GetValue(node, "ISBN-10"),
                this.GetValue(node, "ISBN"),
                this.GetValue(node, "ISBN-13")
            };

            return values.FirstOrDefault(v => v.IsIsbn10());
        }

        private string GetIsbn13(HtmlNode node)
        {
            var values = new string[]
            {
                this.GetValue(node, "ISBN-13"),
                this.GetValue(node, "ISBN"),
                this.GetValue(node, "ISBN-10")
            };

            return values.FirstOrDefault(v => v.IsIsbn13());
        }
    }
}
