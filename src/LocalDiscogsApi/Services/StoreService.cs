using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Database;
using LocalDiscogsApi.Models;
using VinylHub = LocalDiscogsApi.Models.VinylHub;

namespace LocalDiscogsApi.Services
{
    public interface IStoreService
    {
        Task<List<Store>> GetStoresByLocation(double lat, double lng, int radius);
        Task PopulateStores();
    }

    public class StoreService : IStoreService
    {
        private readonly IVinylHubClient vinylHubClient;
        private readonly IDbContext dbContext;
        private readonly IMapper mapper;

        public StoreService(IVinylHubClient vinylHubClient, IDbContext dbContext, IMapper mapper)
        {
            this.vinylHubClient = vinylHubClient;
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task<List<Store>> GetStoresByLocation(double lat, double lng, int radius)
        {
            return await dbContext.GetStoresByLocation(lat, lng, radius);
        }

        public async Task PopulateStores()
        {
            // retrieve from vinylhub
            VinylHub.ShopResponse shopsResponse = await vinylHubClient.GetAllShops();

            IEnumerable<VinylHub.Shop> openShops = shopsResponse.Shops.Where(s => !s.PermanentlyClosed);

            List<Store> stores = mapper.Map<List<Store>>(openShops);

            // update db
            await dbContext.PopulateStores(stores);
        }
    }
}