using System.Threading.Tasks;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Database;
using LocalDiscogsApi.Models;

namespace LocalDiscogsApi.Services
{
    public interface IStoreService
    {
        Task<Store> GetStoresByLocation(double lat, double lng, int radiusKm);
    }

    public class StoreService : IStoreService
    {
        private readonly IVinylHubClient vinylHubClient;
        private readonly IDbContext dbContext;

        public StoreService(IVinylHubClient vinylHubClient, IDbContext dbContext)
        {
            this.vinylHubClient = vinylHubClient;
            this.dbContext = dbContext;
        }

        public Task<Store> GetStoresByLocation(double lat, double lng, int radiusKm)
        {
            // check db

            // populate db.
        }
    }
}