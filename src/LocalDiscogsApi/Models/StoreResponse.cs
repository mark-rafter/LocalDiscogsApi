using MongoDB.Driver.GeoJsonObjectModel;

namespace LocalDiscogsApi.Models
{
    public class StoreResponse
    {
        public string Sellername { get; set; }
        public string Address { get; set; }
    }
}