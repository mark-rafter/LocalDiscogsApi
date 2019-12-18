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
        Task<List<StoreResponse>> GetStoresByLocation(double lat, double lng, int radius);
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

        public async Task<List<StoreResponse>> GetStoresByLocation(double lat, double lng, int radius)
        {
            // get stores
            List<Store> stores = await dbContext.GetStoresByLocation(lat, lng, radius);

            var result = new List<StoreResponse>();

            // todo: parallelise!!
            foreach (Store store in stores)
            {
                // get sellername from db
                SellerName sellerName = await dbContext.GetSellerNameByDocId(store.Docid);

                if (sellerName == null)
                {
                    // get missing sellername from vinylhub
                    string sellername = await vinylHubClient.GetSellerNameByDocId(store.Docid);

                    // todo: if (sellername == null) ...
                    // currently it just stores an empty string so that we don't keep re-checking vinylhub
                    sellerName = new SellerName(
                        id: null,
                        store.Docid,
                        sellername ?? string.Empty);

                    // insert into db.
                    sellerName = await dbContext.SetSellerName(sellerName);
                }

                if (!string.IsNullOrEmpty(sellerName.Sellername))
                {
                    // todo: mapper
                    result.Add(new StoreResponse
                    {
                        Sellername = sellerName.Sellername,
                        Address = store.Address,
                        Location = store.Location
                    });
                }
            }

            return result;
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