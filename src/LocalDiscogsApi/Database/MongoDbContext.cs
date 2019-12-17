using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalDiscogsApi.Config;
using LocalDiscogsApi.Models;
using MongoDB.Driver;

namespace LocalDiscogsApi.Database
{
    public interface IDbContext
    {
        Task<SellerInventory> GetSellerInventory(string userName);
        Task<List<SellerInventory>> GetSellerInventories(params string[] userNames);
        Task<SellerInventory> SetSellerInventory(SellerInventory sellerInventory);

        Task<UserWantlist> GetUserWantlist(string userName);
        Task<UserWantlist> SetUserWantlist(UserWantlist userWantlist);
    }

    public class MongoDbContext : IDbContext
    {
        IMongoDatabase database;
        IMongoCollection<SellerInventory> sellerInventories;
        IMongoCollection<UserWantlist> userWantlists;

        public MongoDbContext(IDatabaseOptions dbOptions)
        {
            MongoClient client = new MongoClient(dbOptions.ConnectionString);
            database = client.GetDatabase(dbOptions.Name);

            sellerInventories = database.GetCollection<SellerInventory>(nameof(SellerInventory));
            userWantlists = database.GetCollection<UserWantlist>(nameof(UserWantlist));
        }

        #region SellerInventory

        public async Task<SellerInventory> GetSellerInventory(string userName)
        {
            IAsyncCursor<SellerInventory> result = await sellerInventories.FindAsync(x => x.Username == userName);
            return result.FirstOrDefault();
        }

        public async Task<List<SellerInventory>> GetSellerInventories(params string[] userNames)
        {
            IAsyncCursor<SellerInventory> result = await sellerInventories.FindAsync(x => userNames.Contains(x.Username));
            return result.ToList();
        }

        public async Task<SellerInventory> SetSellerInventory(SellerInventory sellerInventory)
            => await Upsert(sellerInventories, sellerInventory);

        #endregion

        #region UserWantlist

        public async Task<UserWantlist> GetUserWantlist(string userName)
        {
            IAsyncCursor<UserWantlist> result = await userWantlists.FindAsync(x => x.Username == userName);
            return result.FirstOrDefault();
        }

        public async Task<UserWantlist> SetUserWantlist(UserWantlist userWantlist)
            => await Upsert(userWantlists, userWantlist);

        #endregion

        private async Task<T> Upsert<T>(IMongoCollection<T> collection, T entity) where T : IDbEntity
        {
            if (entity.Id == default)
            {
                entity.CreatedOn = DateTimeOffset.UtcNow;
            }
            else
            {
                entity.ModifiedOn = DateTimeOffset.UtcNow;
            }

            ReplaceOneResult result = await collection.ReplaceOneAsync(
                x => x.Id == entity.Id,
                entity,
                new ReplaceOptions { IsUpsert = true });

            // todo: check result.IsAcknowledged

            return entity;
        }
    }
}