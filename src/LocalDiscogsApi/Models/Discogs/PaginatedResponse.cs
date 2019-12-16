using System;

namespace LocalDiscogsApi.Models.Discogs
{
    public abstract class PaginatedResponse<TItem>
    {
        public PaginatedResponse(TItem[] items, Pagination pagination)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        }

        public abstract TItem[] Items { get; protected set; }
        public Pagination Pagination { get; }
    }
}
