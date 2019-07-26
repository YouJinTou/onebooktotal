using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OBT.Core.Abstractions;
using OBT.Core.Config;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OBT.Core.DAL.Repositories
{
    public class Repository<T> : IRepository<T>
    {
        private readonly IMongoCollection<T> collection;
        private readonly ILogger<Repository<T>> logger;

        public Repository(DatabaseSettings settings, ILogger<Repository<T>> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            this.collection = database.GetCollection<T>(settings.BooksCollectionName);
        }

        public async Task AddAsync(T item)
        {
            try
            {
                await this.collection.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);

                throw;
            }
        }

        public async Task AddManyAsync(IEnumerable<T> items, bool ignoreErrors = false)
        {
            var options = new InsertManyOptions
            {
                IsOrdered = ignoreErrors ? false : true
            };

            try
            {
                await this.collection.InsertManyAsync(items, options);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);

                if (!ignoreErrors)
                {
                    throw;
                }
            }
        }
    }
}
