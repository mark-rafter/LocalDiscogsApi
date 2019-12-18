using MongoDB.Driver.GeoJsonObjectModel;

namespace LocalDiscogsApi.Models
{
    public class Store : DbEntity
    {
        public Store() { }

        public Store(string id, double lat, double lng, string address, int docid)
        {
            base.Id = id ?? default;
            Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(lng, lat));
            Address = address;
            Docid = docid;
        }

        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; private set; }
        public string Address { get; private set; }
        public int Docid { get; private set; }
    }
}