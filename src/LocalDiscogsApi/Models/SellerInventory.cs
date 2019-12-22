using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalDiscogsApi.Models
{
    public class SellerInventory : DbEntity
    {
        public SellerInventory() { }

        public SellerInventory(string id, string username, string avatarUrl, HashSet<SellerListing> inventory, DateTimeOffset lastUpdated)
        {
            base.Id = id ?? default;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            AvatarUrl = avatarUrl;
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            LastUpdated = lastUpdated;
        }

        public string Username { get; private set; }
        public string AvatarUrl { get; private set; }
        public HashSet<SellerListing> Inventory { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset LastUpdated { get; private set; }
    }
}
