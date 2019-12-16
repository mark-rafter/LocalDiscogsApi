namespace LocalDiscogsApi.Config
{
    public interface IDatabaseOptions
    {
        string Name { get; set; }
        string ConnectionString { get; set; }
    }

    public class DatabaseOptions : IDatabaseOptions
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}