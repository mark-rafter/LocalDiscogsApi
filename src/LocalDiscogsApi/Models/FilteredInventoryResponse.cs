using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalDiscogsApi.Models
{
    public class FilteredInventoryResponse
    {
        public FilteredInventoryResponse(SellerInventory sellerInventory, IEnumerable<long> wantlistReleaseIds)
        {
            if (sellerInventory == null)
            {
                throw new ArgumentNullException(nameof(sellerInventory));
            }

            if (wantlistReleaseIds == null)
            {
                throw new ArgumentNullException(nameof(wantlistReleaseIds));
            }

            Sellername = sellerInventory.Username;
            AvatarUrl = sellerInventory.AvatarUrl;
            LastUpdated = sellerInventory.LastUpdated;

            InventoryCount = sellerInventory.Inventory?.Count() ?? 0;
            FilteredInventory = sellerInventory.Inventory?.Where(l => wantlistReleaseIds.Contains(l.ReleaseId)) ?? new List<SellerListing>();
        }

        public string Sellername { get; private set; }
        public string AvatarUrl { get; private set; }
        public IEnumerable<SellerListing> FilteredInventory { get; private set; }
        public int InventoryCount { get; private set; }
        public DateTimeOffset LastUpdated { get; private set; }
    }
}