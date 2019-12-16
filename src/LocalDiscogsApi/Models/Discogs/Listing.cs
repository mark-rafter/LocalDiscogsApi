using Newtonsoft.Json;
using System;

namespace LocalDiscogsApi.Models.Discogs
{
    public class Listing
    {
        public Listing(long id, string condition, string sleeveCondition, DateTimeOffset posted, Price price, Release release, Seller seller)
        {
            Id = id;
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            SleeveCondition = sleeveCondition ?? throw new ArgumentNullException(nameof(sleeveCondition));
            Posted = posted;
            Price = price ?? throw new ArgumentNullException(nameof(price));
            Release = release ?? throw new ArgumentNullException(nameof(release));
            Seller = seller ?? throw new ArgumentNullException(nameof(seller));
        }

        public long Id { get; private set; }

        public string Condition { get; private set; }

        [JsonProperty("sleeve_condition")]
        public string SleeveCondition { get; private set; }

        public DateTimeOffset Posted { get; private set; }

        public Price Price { get; private set; }

        public Release Release { get; private set; }

        public Seller Seller { get; private set; }
    }
}
