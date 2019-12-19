using System;

namespace LocalDiscogsApi.Models.Discogs
{
    public class Pagination
    {
        public Pagination(int items, int page, Urls urls, int pages)
        {
            Items = items;
            Page = page;
            Urls = urls;
            Pages = pages;
        }

        public int Items { get; private set; }
        public int Page { get; private set; }
        public Urls Urls { get; private set; }
        public int Pages { get; private set; }
    }
}
