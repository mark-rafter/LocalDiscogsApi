using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Database;
using LocalDiscogsApi.Models;
using Discogs = LocalDiscogsApi.Models.Discogs;

namespace LocalDiscogsApi.Services
{
    public interface IInventoryService
    {
        Task<SellerInventory> Get(string sellername);
    }

    public class InventoryService : IInventoryService
    {
        private readonly IDiscogsClient discogsClient;
        private readonly IDbContext dbContext;
        private readonly IMapper mapper;

        public InventoryService(IDiscogsClient discogsClient, IDbContext dbContext, IMapper mapper)
        {
            this.discogsClient = discogsClient;
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task<SellerInventory> Get(string sellername)
        {
            if (string.IsNullOrEmpty(sellername))
            {
                throw new ArgumentNullException(nameof(sellername));
            }

            SellerInventory existingInventory = await dbContext.GetSellerInventory(sellername);

            if (existingInventory == null)
            {
                existingInventory = await GetFullInventoryFromDiscogs(sellername);
            }
            else if (IsExpired(existingInventory.LastUpdated, hours: 12))
            {
                // todo: queue update
            }

            return existingInventory;
        }

        private async Task<SellerInventory> GetFullInventoryFromDiscogs(string sellername)
        {
            SellerInventory sellerInventory;
            var listings = new HashSet<SellerListing>(new SellerListingComparer());

            // check exists on discogs
            Discogs.InventoryResponse pageOne = await discogsClient.GetInventoryPageForUser(sellername, 1);

            if (pageOne == null)
            {
                sellerInventory = new SellerInventory(
                    id: null,
                    username: sellername,
                    avatarUrl: null,
                    inventory: listings,
                    lastUpdated: DateTimeOffset.Now);

                // store empty seller in db
                sellerInventory = await dbContext.SetSellerInventory(sellerInventory);

                return sellerInventory;
            }

            // map page1 to SellerInventory
            string avatarUrl = pageOne.Items?.FirstOrDefault() is Discogs.Listing firstListing
                            ? firstListing.Seller.AvatarUrl
                            : string.Empty;

            listings.UnionWith(mapper.Map<List<SellerListing>>(pageOne.Items));

            sellerInventory = new SellerInventory(
                null,
                sellername,
                avatarUrl,
                listings,
                DateTimeOffset.Now);

            // store SellerInventory in db
            sellerInventory = await dbContext.SetSellerInventory(sellerInventory);

            // ditto for the rest of the pages.
            // note: do not parallelise this. implement queue system instead.
            for (int pageNum = 1; pageNum < pageOne.Pagination?.Pages; pageNum++)
            {
                try
                {
                    Discogs.InventoryResponse page = await discogsClient.GetInventoryPageForUser(sellername, pageNum);

                    if (page == null)
                    {
                        break;
                    }

                    listings.UnionWith(mapper.Map<List<SellerListing>>(page.Items));

                    sellerInventory.Inventory = listings;

                    sellerInventory = await dbContext.SetSellerInventory(sellerInventory);
                }
                catch (Exception ex)
                {
                    // todo:
                }
            }

            return sellerInventory;
        }

        private bool IsExpired(DateTimeOffset? lastUpdated, int hours)
        {
            return lastUpdated == null || lastUpdated < DateTimeOffset.UtcNow.AddHours(-hours);
        }
    }
}