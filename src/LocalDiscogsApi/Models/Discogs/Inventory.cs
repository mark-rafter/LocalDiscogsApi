using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace LocalDiscogsApi.Models.Discogs
{
    public class Inventory : ReadOnlyCollection<Listing>
    {
        public Inventory(params Listing[] listings) : base(listings)
        {
        }
    }

    public sealed class InventoryResponse : PaginatedResponse<Listing>
    {
        public InventoryResponse(Listing[] listings, Pagination pagination) : base(listings, pagination)
        {
        }

        [JsonProperty("listings")]
        public override Listing[] Items { get; protected set; }
    }
}
