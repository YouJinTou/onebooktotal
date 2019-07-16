using OBT.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OBT.Core.Abstractions
{
    public interface IScraper
    {
        Task<IEnumerable<Book>> ScrapeAsync();
    }
}
