using System;
using System.Linq;
using System.Threading.Tasks;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Database;
using LocalDiscogsApi.Models;
using Discogs = LocalDiscogsApi.Models.Discogs;

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
        private readonly IDbContext dbContext;

        public WantlistService(IDiscogsClient discogsClient, IDbContext dbContext)
        {
            this.discogsClient = discogsClient;
            this.dbContext = dbContext;
        }

        public async Task<bool> Exists(string username)
        {
            Models.Discogs.WantlistResponse response = await discogsClient.GetWantlistPageForUser(username, 1);
            return response != null;
        }

        public async Task<UserWantlist> Get(string username)
        {
            UserWantlist existingWantlist = await dbContext.GetUserWantlist(username);

            if (existingWantlist == null)
            {
                // retrieve latest from discogs
                Discogs.Wantlist latestWantlist = await discogsClient.GetWantlistForUser(username);

                var userWantlist = new UserWantlist(
                        existingWantlist?.Id,
                        username,
                        latestWantlist.Select(w => w.ReleaseId),
                        DateTimeOffset.UtcNow);

                // update db
                await dbContext.SetUserWantlist(userWantlist);

                return userWantlist;
            }
            else if (IsExpired(existingWantlist.LastUpdated, hours: 6))
            {
                // todo: queue update
            }

            return existingWantlist;
        }

        private bool IsExpired(DateTimeOffset? lastUpdated, int hours)
        {
            return lastUpdated == null || lastUpdated < DateTimeOffset.Now.AddHours(-hours);
        }
    }
}