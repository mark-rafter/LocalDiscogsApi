using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace LocalDiscogsApi.Models.Discogs
{
    public class Wantlist : ReadOnlyCollection<Want>
    {
        public Wantlist(params Want[] wantlist) : base(wantlist)
        {
        }
    }

    public sealed class WantlistResponse : PaginatedResponse<Want>
    {
        public WantlistResponse(Want[] wants, Pagination pagination) : base(wants, pagination)
        {
        }

        [JsonProperty("wants")]
        public override Want[] Items { get; protected set; }
    }
}
