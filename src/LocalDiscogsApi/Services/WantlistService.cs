using System.Threading.Tasks;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Models;

namespace LocalDiscogsApi.Services
{
    public interface IWantlistService
    {
        Task<bool> Exists(string username);
        Task<UserWantlist> Get(string username);
    }

    public class WantlistService : IWantlistService
    {
        private readonly IDiscogsClient discogsClient;

        public WantlistService(IDiscogsClient discogsClient)
        {
            this.discogsClient = discogsClient;
        }

        public async Task<bool> Exists(string username)
        {
            Models.Discogs.WantlistResponse response = await discogsClient.GetWantlistPageForUser(username, 1);
            return response != null;
        }

        public Task<UserWantlist> Get(string username)
        {
            throw new System.NotImplementedException();
            // check db.
            // if db empty, retrieve from discogs, update db, return value.
            // if OOD, return db. queue update.
        }
    }
}