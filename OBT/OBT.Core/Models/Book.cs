namespace OBT.Core.Models
{
    public class Book
    {
        public string Isbn10 { get; set; }

        public string Isbn13 { get; set; }

        public string Title { get; set; }

        public string Genre { get; set; }

        public string Author { get; set; }

        public string Language { get; set; }

        public int Year { get; set; }

        public string Format { get; set; }

        public int Pages { get; set; }
    }
}
