using Newtonsoft.Json;
using System;

namespace LocalDiscogsApi.Models.Discogs
{
    public class Want
    {
        public Want(long releaseId, DateTimeOffset dateAdded)
        {
            ReleaseId = releaseId;
            DateAdded = dateAdded;
        }

        [JsonProperty("id")]
        public long ReleaseId { get; private set; }

        [JsonProperty("date_added")]
        public DateTimeOffset DateAdded { get; private set; }
    }
}
