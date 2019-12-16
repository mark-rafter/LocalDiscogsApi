using System;
using Discogs = LocalDiscogsApi.Models.Discogs;

namespace LocalDiscogsApi.Test.Helpers
{
    public class ListingBuilder
    {
        private long listingId = 1;
        private string condition = "condition";
        private string sleeveCondition = "sleeveCondition";
        private string currency = "currency";
        private decimal value = 9.99m;
        private DateTimeOffset posted = DateTimeOffset.Parse("2019-01-20T07:00:00+00:00");
        private long releaseId = 99;
        private string releaseDesc = "releaseDesc";
        private string sellerName = "sellerName";
        private string sellerAvatarUrl = "sellerAvatarUrl";

        public Discogs.Listing Build()
        {
            return new Discogs.Listing(
                    id: listingId,
                    condition: condition,
                    sleeveCondition: sleeveCondition,
                    posted: posted,
                    price: new Discogs.Price(currency: currency, value: value),
                    release: new Discogs.Release(id: releaseId, description: releaseDesc),
                    seller: new Discogs.Seller(username: sellerName, avatarUrl: sellerAvatarUrl));
        }

        public ListingBuilder WithListingId(long id)
        {
            listingId = id;
            return this;
        }

        public ListingBuilder WithCondition(string condition, string sleeveCondition)
        {
            this.condition = condition;
            this.sleeveCondition = sleeveCondition;
            return this;
        }

        public ListingBuilder WithPrice(string currency, decimal value)
        {
            this.currency = currency;
            this.value = value;
            return this;
        }

        public ListingBuilder WithPosted(string posted)
        {
            this.posted = DateTimeOffset.Parse(posted);
            return this;
        }

        public ListingBuilder WithReleaseId(long id)
        {
            releaseId = id;
            return this;
        }

        public ListingBuilder WithReleaseDescription(string description)
        {
            releaseDesc = description;
            return this;
        }

        public ListingBuilder WithSeller(string username, string avatarUrl)
        {
            sellerName = username;
            sellerAvatarUrl = avatarUrl;
            return this;
        }
    }
}
