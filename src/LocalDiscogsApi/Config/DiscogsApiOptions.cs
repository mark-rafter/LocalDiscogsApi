namespace LocalDiscogsApi.Config
{
    public interface IDiscogsApiOptions
    {
        string Url { get; set; }
        string UserAgent { get; set; }
        string RatelimitRemainingHeaderName { get; set; }
        int RatelimitTimeout { get; set; }
    }

    public class DiscogsApiOptions : IDiscogsApiOptions
    {
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public string RatelimitRemainingHeaderName { get; set; }
        public int RatelimitTimeout { get; set; }
    }
}