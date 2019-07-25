using MongoDB.Driver;
using OBT.Core.Abstractions;
using OBT.Core.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OBT.Core.DAL.Repositories
{
    public class Repository<T> : IRepository<T>
    {
        private readonly IMongoCollection<T> collection;

        public Repository(DatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            this.collection = database.GetCollection<T>(settings.BooksCollectionName);
        }

        public async Task AddAsync(T item)
        {
            await this.collection.InsertOneAsync(item);
        }

        public async Task AddManyAsync(IEnumerable<T> items)
        {
            var options = new InsertManyOptions
            {
                IsOrdered = false
            };

            await this.collection.InsertManyAsync(items, options);
        }
    }
}
