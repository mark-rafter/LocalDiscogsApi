using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using LocalDiscogsApi.Exceptions;
using VinylHub = LocalDiscogsApi.Models.VinylHub;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text.RegularExpressions;

namespace LocalDiscogsApi.Clients
{
    public interface IVinylHubClient
    {
        Task<VinylHub.ShopResponse> GetAllShops();

        Task<string> GetSellerNameByDocId(int docid);
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

        public async Task<string> GetSellerNameByDocId(int docid)
        {
            using (HttpResponseMessage httpResponse = await httpClient.GetAsync($"shop/{docid}"))
            {
                // todo: 404 test.

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new RestRequestException(httpResponse, await httpResponse.Content?.ReadAsStringAsync());
                }

                string responseString = await httpResponse.Content.ReadAsStringAsync();

                return GetSellerNameFromResponse(responseString);
            }
        }

        private string GetSellerNameFromResponse(string responseString)
        {
            string pattern = @"discogs\.com\/seller\/(\S+)""";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            Match match = regex.Match(responseString);

            if (match.Success && match.Groups.Count > 1)
            {
                // match.Groups[1] will be either: 
                // SELLER_NAME/....
                // SELLER_NAME/
                // SELLER_NAME
                return match.Groups[1].Value.Split('/')[0];
            }

            // todo: log? / exception?
            return null;
        }
    }
}