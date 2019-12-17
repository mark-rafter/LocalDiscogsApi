using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalDiscogsApi.Models
{
    public class UserWantlist : DbEntity
    {
        public UserWantlist() { }

        public UserWantlist(string id, string username, IEnumerable<long> releaseIds, DateTimeOffset lastUpdated)
        {
            base.Id = id ?? default;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            ReleaseIds = releaseIds ?? throw new ArgumentNullException(nameof(releaseIds));
            LastUpdated = lastUpdated;
        }

        public string Username { get; private set; }
        public IEnumerable<long> ReleaseIds { get; private set; }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset LastUpdated { get; private set; }
    }
}
