using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using OBT.Core.Abstractions;
using OBT.Core.DAL.Models.Mongo;
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
        private readonly IRepository<DbBook> books;
        private readonly ILogger<BookogsScraper> logger;

        public BookogsScraper(
            IHttpService httpService, IRepository<DbBook> books, ILogger<BookogsScraper> logger)
        {
            this.httpService = httpService;
            this.books = books;
            this.logger = logger;
        }

        public async Task<IEnumerable<Book>> ScrapeAsync()
        {
            try
            {
                var rootObjects = await this.GetRootObjectsAsync();
                var books = await this.GetBooksAsync(rootObjects);

                await this.PersistBooksAsync(books);

                return books;
            }
            catch (Exception ex)
            {
                this.logger.LogCritical(ex.ToString());

                throw;
            }
        }

        private async Task<IEnumerable<Rootobject>> GetRootObjectsAsync()
        {
            var totalPages = Enumerable.Range(1, await this.GetTotalPages());
            var rootObjects = new ConcurrentBag<Rootobject>();

            for (int p = 0; p < totalPages.Count(); p++)
            {
                try
                {
                    Func<Task> call = async () =>
                    {
                        var url = string.Format(ApiUrl, p);
                        var root = await this.httpService.GetAsync<Rootobject>(url);

                        rootObjects.Add(root);
                    };

                    await call.RetryAsync();
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Page {p}. Dump: {ex}");
                }
            }

            return rootObjects;
        }

        private async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Rootobject> rootObjects)
        {
            var books = new ConcurrentBag<Book>();

            foreach (var root in rootObjects)
            {
                foreach (var entity in root.entities)
                {
                    try
                    {
                        Func<Task> call = async () => await this.GetBookAsync(books, entity);

                        await call.RetryAsync(delay: 1000);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"Book \"{entity.title}\" failed. Dump {ex}");
                    }
                }
            }

            return books;
        }

        private async Task PersistBooksAsync(IEnumerable<Book> books)
        {
            var dbBooks = new List<DbBook>();

            foreach (var book in books)
            {
                dbBooks.Add(new DbBook
                {
                    Title = book.Title,
                    Authors = book.Authors,
                    Format = book.Format,
                    Genre = book.Genre,
                    Isbn10 = book.Isbn10,
                    Isbn13 = book.Isbn13,
                    Language = book.Language,
                    Pages = book.Pages,
                    Year = book.Year
                });
            }

            await this.books.AddManyAsync(dbBooks);
        }

        private async Task<int> GetTotalPages()
        {
            var root = await this.httpService.GetAsync<Rootobject>(string.Format(ApiUrl, 1));
            var booksPerPage = 40;
            var totalPages = (root.total / booksPerPage) + 1;

            return totalPages;
        }

        private async Task GetBookAsync(ConcurrentBag<Book> books, Entity entity)
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
                Year = this.GetYear(node).ToYear()
            };

            books.Add(book);
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

        private string GetYear(HtmlNode node)
        {
            var values = new string[]
            {
                this.GetValue(node, "This Edition Published"),
                this.GetValue(node, "First Published"),
                this.GetValue(node, "Copyright")
            };

            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }
    }
}
