using System;

namespace LocalDiscogsApi.Models
{
    public class SellerName : DbEntity
    {
        public SellerName() { }

        public SellerName(string id, int docid, string sellername)
        {
            base.Id = id ?? default;
            Docid = docid;
            Sellername = sellername ?? throw new ArgumentNullException(nameof(sellername));
        }

        public int Docid { get; private set; }
        public string Sellername { get; private set; }
    }
}