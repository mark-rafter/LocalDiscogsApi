namespace LocalDiscogsApi.Models.Discogs
{
    public class Urls
    {
        public Urls(string next)
        {
            Next = next;
        }

        public string Next { get; private set; }
    }
}
