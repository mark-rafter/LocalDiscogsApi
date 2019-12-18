using System.Collections.Generic;
using Newtonsoft.Json;

namespace LocalDiscogsApi.Models.VinylHub
{
    public class ShopResponse
    {
        public ShopResponse(Shop[] shops)
        {
            Shops = shops;
        }

        public Shop[] Shops { get; protected set; }
    }

    public class Shop
    {
        [JsonProperty("permanently_closed")]
        public bool PermanentlyClosed { get; set; }
        public string Title { get; set; }
        public Coordinates Coordinates { get; set; }
        public List<OpeningHours> Hours { get; set; }
        public string Address { get; set; }
        public int Docid { get; set; }
    }

    public class Coordinates
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class OpeningHours
    {
        public string Close { get; set; }
        public string Notes { get; set; }
        public string Open { get; set; }
        public string Day { get; set; }
        public bool? Opened { get; set; }
    }
}