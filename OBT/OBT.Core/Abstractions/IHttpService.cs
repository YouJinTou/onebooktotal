using System.Threading.Tasks;

namespace OBT.Core.Abstractions
{
    public interface IHttpService
    {
        Task<T> GetAsync<T>(string url);

        Task<T> PostAsync<T>(string url, object body);
    }
}
