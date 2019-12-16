using Newtonsoft.Json;

namespace LocalDiscogsApi.Models.Discogs
{
    public class Seller
    {
        public Seller(string username, string avatarUrl)
        {
            Username = username;
            AvatarUrl = avatarUrl;
        }

        public string Username { get; private set; }

        [JsonProperty("avatar_url")] // todo: globalise snake case deserialisation and remove these attributes
        public string AvatarUrl { get; private set; }
    }
}
