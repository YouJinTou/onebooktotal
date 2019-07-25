using System.Collections.Generic;
using System.Threading.Tasks;

namespace OBT.Core.Abstractions
{
    public interface IRepository<T>
    {
        Task AddAsync(T item);

        Task AddManyAsync(IEnumerable<T> items);
    }
}
