using OBT.Core.Abstractions;
using OBT.Core.Models;
using OBT.Core.Models.Bookogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OBT.Core.Scrapers
{
    public class BookogsScraper : IScraper
    {
        private readonly IHttpService httpService;

        public BookogsScraper(IHttpService httpService)
        {
            this.httpService = httpService;
        }

        public async Task<IEnumerable<Book>> ScrapeAsync()
        {
            var url = "https://www.bookogs.com/api/browse/book?page=1";
            var x = await this.httpService.GetAsync<Rootobject>(url);

            return null;
        }
    }
}
