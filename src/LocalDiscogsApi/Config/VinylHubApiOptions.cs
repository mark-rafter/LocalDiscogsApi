namespace LocalDiscogsApi.Config
{
    public interface IVinylHubApiOptions
    {
        string Url { get; set; }
    }

    public class VinylHubApiOptions : IVinylHubApiOptions
    {
        public string Url { get; set; }
    }
}