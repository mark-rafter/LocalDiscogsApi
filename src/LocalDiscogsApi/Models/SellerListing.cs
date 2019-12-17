using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocalDiscogsApi.Models
{
    public class SellerListing
    {
        public SellerListing() { }

        public long Id { get; private set; }

        public string Description { get; private set; }

        public string Condition { get; private set; }

        public string SleeveCondition { get; private set; }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset Posted { get; private set; }

        public string Price { get; private set; }

        public long ReleaseId { get; private set; }
    }
}
