using MongoDB.Bson.Serialization;
using OBT.Core.Tools;

namespace OBT.Core.DAL.Models.Mongo
{
    public class IdGenerator : IIdGenerator
    {
        public object GenerateId(object container, object document)
        {
            var book = document as DbBook;

            Validator.ThrowIfNull(book);

            var authors = string.Join(string.Empty, book.Authors ?? new string[] { });

            unchecked
            {
                var hash = 17;
                hash = hash * 23 + book.Isbn10?.GetHashCode() ?? 3;
                hash = hash * 23 + book.Isbn13?.GetHashCode() ?? 5;
                hash = hash * 23 + book.Title?.GetHashCode() ?? 7;
                hash = hash * 23 + book.Year?.GetHashCode() ?? 11;
                hash = hash * 23 + book.Format?.GetHashCode() ?? 13;
                hash = hash * 23 + authors.GetHashCode();

                return hash.ToString();
            }
        }

        public bool IsEmpty(object id)
        {
            return id == null || string.IsNullOrWhiteSpace(id.ToString());
        }
    }
}
