using LocalDiscogsApi.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discogs = LocalDiscogsApi.Models.Discogs;

namespace LocalDiscogsApi.Services
{
    public interface IDiscogsService
    {
        Task<Discogs.Inventory> GetInventoryForUser(string userName);
        Task<Discogs.Wantlist> GetWantlistForUser(string userName);
        Task<IEnumerable<Discogs.Listing>> GetWantlistItemsInStock(string wantlistUser, params string[] sellerNames);
    }

    public class DiscogsService : IDiscogsService
    {
        private readonly IDiscogsClient discogsClient;

        public DiscogsService(IDiscogsClient discogsClient)
        {
            this.discogsClient = discogsClient ?? throw new ArgumentNullException(nameof(discogsClient));
        }

        public async Task<Discogs.Inventory> GetInventoryForUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            Discogs.Inventory result = await discogsClient.GetInventoryForUser(userName);

            return result;
        }

        public async Task<Discogs.Wantlist> GetWantlistForUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            Discogs.Wantlist result = await discogsClient.GetWantlistForUser(userName);

            return result;
        }

        public async Task<IEnumerable<Discogs.Listing>> GetWantlistItemsInStock(string wantlistUser, params string[] sellerNames)
        {
            if (string.IsNullOrEmpty(wantlistUser))
            {
                throw new ArgumentNullException(nameof(wantlistUser));
            }

            if (sellerNames == null || !sellerNames.Any())
            {
                throw new ArgumentNullException(nameof(sellerNames));
            }

            Discogs.Wantlist wantlist = await GetWantlistForUser(wantlistUser);

            if (!wantlist.Any())
            {
                return Enumerable.Empty<Discogs.Listing>();
            }

            var inventoryTasks = new List<Task<Discogs.Inventory>>();

            foreach (string sellerName in sellerNames)
            {
                inventoryTasks.Add(GetInventoryForUser(sellerName));
            }

            await Task.WhenAll(inventoryTasks);

            IEnumerable<Discogs.Listing> summedInventories = inventoryTasks?.SelectMany(task => task.Result);

            IEnumerable<long> wantlistReleaseIds = wantlist.Select(w => w.ReleaseId);

            IEnumerable<Discogs.Listing> wantlistListingsInStock = summedInventories?.Where(l => wantlistReleaseIds.Contains(l.Release.Id));

            return wantlistListingsInStock;
        }
    }
}