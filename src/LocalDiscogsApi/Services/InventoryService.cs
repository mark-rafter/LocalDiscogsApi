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
                // check exists on discogs
                Discogs.InventoryResponse page1 = await discogsClient.GetInventoryPageForUser(sellername, 1);

                if (page1 == null)
                {
                    return null;
                }

                // retrieve latest from discogs
                Discogs.Inventory latestInventory = await discogsClient.GetInventoryForUser(sellername);

                string avatarUrl = latestInventory?.FirstOrDefault() is Discogs.Listing firstListing
                    ? firstListing.Seller.AvatarUrl
                    : string.Empty;

                // todo: test what happens when inventory exists but listings = 0? (might be covered in DiscogsClient tests)
                List<SellerListing> mappedInventory = mapper.Map<List<SellerListing>>(latestInventory);

                var sellerInventory = new SellerInventory(
                    null,
                    sellername,
                    avatarUrl,
                    mappedInventory,
                    DateTimeOffset.Now);

                // update db, populates sellerInventory id.
                sellerInventory = await dbContext.SetSellerInventory(sellerInventory);

                return sellerInventory;
            }
            else if (IsExpired(existingInventory.LastUpdated, hours: 12))
            {
                // todo: queue update
            }

            return existingInventory;
        }

        private bool IsExpired(DateTimeOffset? lastUpdated, int hours)
        {
            return lastUpdated == null || lastUpdated < DateTimeOffset.UtcNow.AddHours(-hours);
        }
    }
}