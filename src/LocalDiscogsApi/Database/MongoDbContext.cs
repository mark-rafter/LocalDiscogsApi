using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalDiscogsApi.Config;
using LocalDiscogsApi.Models;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace LocalDiscogsApi.Database
{
    public interface IDbContext
    {
        Task<SellerInventory> GetSellerInventory(string userName);
        Task<List<SellerInventory>> GetSellerInventories(params string[] userNames);
        Task<SellerInventory> SetSellerInventory(SellerInventory sellerInventory);

        Task<UserWantlist> GetUserWantlist(string userName);
        Task<UserWantlist> SetUserWantlist(UserWantlist userWantlist);

        Task<List<Store>> GetStoresByLocation(double lat, double lng, int radius);
        Task<Store> SetStore(Store store);
        Task PopulateStores(IEnumerable<Store> stores);

        Task<SellerName> GetSellerNameByDocId(int docid);
        Task<SellerName> SetSellerName(SellerName sellerName);
    }

    public class MongoDbContext : IDbContext
    {
        IMongoDatabase database;
        IMongoCollection<SellerInventory> sellerInventories;
        IMongoCollection<UserWantlist> userWantlists;
        IMongoCollection<Store> stores;
        IMongoCollection<SellerName> sellerNames;

        public MongoDbContext(IDatabaseOptions dbOptions)
        {
            MongoClient client = new MongoClient(dbOptions.ConnectionString);
            database = client.GetDatabase(dbOptions.Name);

            sellerInventories = database.GetCollection<SellerInventory>(nameof(SellerInventory));
            userWantlists = database.GetCollection<UserWantlist>(nameof(UserWantlist));
            stores = database.GetCollection<Store>(nameof(Store));
            sellerNames = database.GetCollection<SellerName>(nameof(SellerName));
        }

        private async Task<T> Upsert<T>(IMongoCollection<T> collection, T entity) where T : IDbEntity
        {
            if (entity.Id == default)
            {
                entity.CreatedOn = DateTimeOffset.UtcNow;
                await collection.InsertOneAsync(entity);
            }
            else
            {
                entity.ModifiedOn = DateTimeOffset.UtcNow;
                ReplaceOneResult result = await collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
                // todo: check result.IsAcknowledged
            }

            return entity;
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

        #region Store

        public async Task<List<Store>> GetStoresByLocation(double lat, double lng, int radius)
        {
            GeoJsonPoint<GeoJson2DGeographicCoordinates> point = GeoJson.Point(GeoJson.Geographic(lng, lat));

            FilterDefinition<Store> locationQuery =
                new FilterDefinitionBuilder<Store>().Near(store => store.Location, point, radius);

            IAsyncCursor<Store> result = await stores.FindAsync(locationQuery);
            return result.ToList();
        }

        public async Task<Store> SetStore(Store store)
        {
            store.ModifiedOn = DateTimeOffset.UtcNow;

            ReplaceOneResult result = await stores.ReplaceOneAsync(
                x => x.Docid == store.Docid,
                store,
                new ReplaceOptions { IsUpsert = true });

            return store;
        }

        public async Task PopulateStores(IEnumerable<Store> newStores)
        {
            await stores.InsertManyAsync(newStores);

            var builder = Builders<Store>.IndexKeys;
            IndexKeysDefinition<Store> keys = builder.Geo2DSphere(tag => tag.Location);
            await stores.Indexes.CreateOneAsync(new CreateIndexModel<Store>(keys));
        }

        #endregion

        #region SellerName

        public async Task<SellerName> GetSellerNameByDocId(int docid)
        {
            IAsyncCursor<SellerName> result = await sellerNames.FindAsync(x => x.Docid == docid);
            return result.FirstOrDefault();
        }

        public async Task<SellerName> SetSellerName(SellerName sellerName)
            => await Upsert(sellerNames, sellerName);

        #endregion
    }
}