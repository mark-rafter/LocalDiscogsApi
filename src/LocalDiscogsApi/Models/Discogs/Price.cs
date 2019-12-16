using System;

namespace LocalDiscogsApi.Models.Discogs
{
    public class Price
    {
        public Price(string currency, decimal value)
        {
            Currency = currency;
            Value = value;
        }

        public string Currency { get; private set; }
        public decimal Value { get; private set; }
    }
}
