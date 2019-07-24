using System.Collections.Generic;
using System.Linq;

namespace OBT.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<IEnumerable<T>> ToBatches<T>(
            this IEnumerable<T> collection, int batchSize)
        {
            var batches = new List<IEnumerable<T>>();
            
            for (int b = 0; b < collection.Count(); b += batchSize)
            {
                batches.Add(collection.Skip(b).Take(batchSize));
            }

            return batches;
        }
    }
}
