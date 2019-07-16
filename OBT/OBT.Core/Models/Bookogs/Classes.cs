namespace OBT.Core.Models.Bookogs
{
    public class Rootobject
    {
        public Entity[] entities { get; set; }
        public int total { get; set; }
        public Filters filters { get; set; }
        public Sort_Options[] sort_options { get; set; }
        public string title { get; set; }
        public Aggregations aggregations { get; set; }
    }

    public class Filters
    {
    }

    public class Aggregations
    {
        public Marketplace marketplace { get; set; }
        public Decades decades { get; set; }
        public Format format { get; set; }
        public Genre genre { get; set; }
        public Languages languages { get; set; }
    }

    public class Marketplace
    {
        public Bucket[] buckets { get; set; }
    }

    public class Bucket
    {
        public float from { get; set; }
        public string key { get; set; }
        public int doc_count { get; set; }
    }

    public class Decades
    {
        public Bucket1[] buckets { get; set; }
    }

    public class Bucket1
    {
        public float to { get; set; }
        public float from { get; set; }
        public string key { get; set; }
        public int doc_count { get; set; }
    }

    public class Format
    {
        public Bucket2[] buckets { get; set; }
        public int sum_other_doc_count { get; set; }
        public int doc_count_error_upper_bound { get; set; }
    }

    public class Bucket2
    {
        public string key { get; set; }
        public int doc_count { get; set; }
    }

    public class Genre
    {
        public Bucket3[] buckets { get; set; }
        public int sum_other_doc_count { get; set; }
        public int doc_count_error_upper_bound { get; set; }
    }

    public class Bucket3
    {
        public string key { get; set; }
        public int doc_count { get; set; }
    }

    public class Languages
    {
        public Bucket4[] buckets { get; set; }
        public int sum_other_doc_count { get; set; }
        public int doc_count_error_upper_bound { get; set; }
    }

    public class Bucket4
    {
        public string key { get; set; }
        public int doc_count { get; set; }
    }

    public class Entity
    {
        public string item_id { get; set; }
        public string subtitle { get; set; }
        public string thumbnail_alt { get; set; }
        public bool removable { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public object list_id { get; set; }
        public object badge { get; set; }
        public string thumbnail { get; set; }
        public Doctype doctype { get; set; }
    }

    public class Doctype
    {
        public string mount { get; set; }
        public string icon { get; set; }
        public bool builtin { get; set; }
        public string title { get; set; }
    }

    public class Sort_Options
    {
        public bool selected { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
}
