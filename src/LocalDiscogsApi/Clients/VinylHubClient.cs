using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using LocalDiscogsApi.Exceptions;
using VinylHub = LocalDiscogsApi.Models.VinylHub;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace LocalDiscogsApi.Clients
{
    public interface IVinylHubClient
    {
        Task<VinylHub.ShopResponse> GetAllShops();
    }

    public class VinylHubClient : IVinylHubClient
    {
        private readonly HttpClient httpClient;

        private readonly string markersJsonPath;

        public VinylHubClient(HttpClient httpClient, IWebHostEnvironment env)
        {
            this.httpClient = httpClient;

            markersJsonPath = env.ContentRootPath + Path.DirectorySeparatorChar
                + "Data" + Path.DirectorySeparatorChar
                + "markers_2019-12-17.json";
        }

        public async Task<VinylHub.ShopResponse> GetAllShops()
        {
            // todo: dev only. use external call in prod
            using (StreamReader fileStream = File.OpenText(markersJsonPath))
            {
                string fileString = await fileStream.ReadToEndAsync();

                // todo: look into possible yield return scenario to lessen memory load?
                return JsonConvert.DeserializeObject<VinylHub.ShopResponse>(fileString);
            }

            // using (HttpResponseMessage httpResponse = await httpClient.GetAsync("api/markers"))
            // {
            //     if (!httpResponse.IsSuccessStatusCode)
            //     {
            //         throw new RestRequestException(httpResponse, await httpResponse.Content?.ReadAsStringAsync());
            //     }

            //     string responseString = await httpResponse.Content.ReadAsStringAsync();
            //     return JsonConvert.DeserializeObject<IList<VinylHub.Store>>(responseString);
            // }
        }

        // todo: get shop with httpClient.GetAsync("shop/{Docid}") 
        // scrape for https://www.discogs.com/seller/XXXXXXX
        // <td id="external-links" class="" data-props='{"links": [{"nofollow": true, "title": "piccadillyrecords.com", "url": "http://www.piccadillyrecords.com"}, {"nofollow": false, "notes": "Discogs Seller Page", "title": "Discogs", "url": "https://www.discogs.com/seller/Piccadillyrecords/profile"}], "user": false}'/>
    }
}