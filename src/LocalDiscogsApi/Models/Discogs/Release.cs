namespace LocalDiscogsApi.Models.Discogs
{
    public class Release
    {
        public Release(long id, string description)
        {
            Id = id;
            Description = description;
        }

        public long Id { get; private set; }
        public string Description { get; private set; }
    }
}
