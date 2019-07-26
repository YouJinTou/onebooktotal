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

            return book.GetHashCode().ToString();
        }

        public bool IsEmpty(object id)
        {
            return id == null || string.IsNullOrWhiteSpace(id.ToString());
        }
    }
}
