namespace LocalDiscogsApi.Config
{
    public class DiscogsApiOptions
    {
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public string RatelimitRemainingHeaderName { get; set; }
        public string RatelimitTimeout { get; set; }
    }
}