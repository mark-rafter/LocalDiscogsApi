using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using LocalDiscogsApi.Exceptions;
using VinylHub = LocalDiscogsApi.Models.VinylHub;

namespace LocalDiscogsApi.Clients
{
    public interface IVinylHubClient
    {
        Task<IList<VinylHub.Store>> GetStores();
    }

    public class VinylHubClient : IVinylHubClient
    {
        private readonly HttpClient httpClient;

        public VinylHubClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IList<VinylHub.Store>> GetStores()
        {
            // todo: use local JSON first.

            using (HttpResponseMessage httpResponse = await httpClient.GetAsync("api/markers"))
            {
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new RestRequestException(httpResponse, await httpResponse.Content?.ReadAsStringAsync());
                }

                string responseString = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IList<VinylHub.Store>>(responseString);
            }
        }

        // todo: get store with httpClient.GetAsync("shop/{Docid}") 
        // scrape for https://www.discogs.com/seller/XXXXXXX
        // <td id="external-links" class="" data-props='{"links": [{"nofollow": true, "title": "piccadillyrecords.com", "url": "http://www.piccadillyrecords.com"}, {"nofollow": false, "notes": "Discogs Seller Page", "title": "Discogs", "url": "https://www.discogs.com/seller/Piccadillyrecords/profile"}], "user": false}'/>
    }
}