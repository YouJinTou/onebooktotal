using HtmlAgilityPack;
using OBT.Core.Abstractions;
using OBT.Core.Extensions;
using OBT.Core.Models;
using OBT.Core.Models.Bookogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBT.Core.Scrapers
{
    public class BookogsScraper : IScraper
    {
        private static readonly string BaseUrl = "https://www.bookogs.com";
        private static readonly string ApiUrl = $"{BaseUrl}/api/browse/book?page={{0}}";
        private static readonly string TableXPath = "//tr[td='{0}']/td[2]/a/text()";
        private readonly IHttpService httpService;

        public BookogsScraper(IHttpService httpService)
        {
            this.httpService = httpService;
        }

        public async Task<IEnumerable<Book>> ScrapeAsync()
        {
            var rootObjects = await this.GetRootObjectsAsync();
            var books = await this.GetBooksAsync(rootObjects);

            return books;
        }

        private async Task<IEnumerable<Rootobject>> GetRootObjectsAsync()
        {
            try
            {
                var totalPages = Enumerable.Range(1, await this.GetTotalPages());
                var pages = totalPages.ToBatches(40).Select(b => new { From = b.Min(), To = b.Max() });
                var rootObjects = new List<Rootobject>();
                var failedCalls = new List<int>();

                foreach (var page in pages)
                {
                    var apiCallTasks = new List<Task>();

                    for (int p = page.From; p <= page.To; p++)
                    {
                        Console.WriteLine($"Page: {p}");

                        var apiCallTask = Task.Run(async () =>
                        {
                            await this.RunApiTaskAsync(failedCalls, rootObjects, p);
                        });

                        apiCallTasks.Add(apiCallTask);

                        await Task.Delay(1);
                    }

                    Task.WaitAll(apiCallTasks.ToArray());

                    Console.WriteLine($"Batch from {page.From} to {page.To}");

                    await Task.Delay(5000);
                }

                Console.WriteLine("Running failed tasks...");

                for (int i = 0; i < failedCalls.Count; i++)
                {
                    await this.RunApiTaskAsync(failedCalls, rootObjects, failedCalls[i]);
                }

                return rootObjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private async Task RunApiTaskAsync(ICollection<int> failedCalls, List<Rootobject> rootObjects, int p)
        {
            try
            {
                var url = string.Format(ApiUrl, p);

                Console.WriteLine($"Queueing up: {url}");

                var root = await this.httpService.GetAsync<Rootobject>(url);

                rootObjects.Add(root);

                Console.WriteLine("Added root.");
            }
            catch (Exception ex)
            {
                failedCalls.Add(p);

                Console.WriteLine($"Failed page: {p}");
            }
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
