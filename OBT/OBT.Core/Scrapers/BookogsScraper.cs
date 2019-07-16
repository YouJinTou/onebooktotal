using HtmlAgilityPack;
using OBT.Core.Abstractions;
using OBT.Core.Extensions;
using OBT.Core.Models;
using OBT.Core.Models.Bookogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OBT.Core.Scrapers
{
    public class BookogsScraper : IScraper
    {
        private static readonly string BaseUrl = "https://www.bookogs.com";
        private static readonly string ApiUrl = $"{BaseUrl}/api/browse/book?page={0}";
        private static readonly string TableXPath = "//tr[td='{0}']/td[2]/a/text()";
        private readonly IHttpService httpService;

        public BookogsScraper(IHttpService httpService)
        {
            this.httpService = httpService;
        }

        public async Task<IEnumerable<Book>> ScrapeAsync()
        {
            var page = 0;
            var books = new List<Book>();

            while (true)
            {
                var root = await this.httpService.GetAsync<Rootobject>(string.Format(ApiUrl, page));

                if (root.entities.Length == 0)
                {
                    break;
                }

                foreach (var entity in root.entities)
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
                            Title = this.GetTitle(node),
                            Year = this.GetValue(node, "This Edition Published").ToYear()
                        };

                        books.Add(book);
                    }
                    catch
                    {
                    }
                }

                page++;
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

        private string GetTitle(HtmlNode node)
        {
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
