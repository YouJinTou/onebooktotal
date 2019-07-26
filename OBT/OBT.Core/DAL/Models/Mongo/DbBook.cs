using MongoDB.Bson.Serialization.Attributes;
using OBT.Core.Extensions;
using System.Collections.Generic;

namespace OBT.Core.DAL.Models.Mongo
{
    public class DbBook
    {
        [BsonId(IdGenerator = typeof(IdGenerator))]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Isbn10 { get; set; }

        public string Isbn13 { get; set; }

        public string Genre { get; set; }

        public IEnumerable<string> Authors { get; set; }

        public string Language { get; set; }

        public int? Year { get; set; }

        public string Format { get; set; }

        public int? Pages { get; set; }

        public override int GetHashCode()
        {
            var authors = string.Join(string.Empty, this.Authors ?? new string[] { });
            var hashCode = $"{this.Isbn10}{this.Isbn13}{this.Title}{this.Year}{this.Format}"
                .GetDeterministicHashCode();

            return hashCode;
        }
    }
}
