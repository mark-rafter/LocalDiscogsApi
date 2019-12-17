using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LocalDiscogsApi.Models
{
    public interface IDbEntity
    {
        string Id { get; set; }
        DateTimeOffset? CreatedOn { get; set; }
        DateTimeOffset? ModifiedOn { get; set; }
    }

    public abstract class DbEntity : IDbEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset? CreatedOn { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset? ModifiedOn { get; set; }

        public DbEntity() { }
    }
}
