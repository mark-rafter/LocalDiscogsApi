using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using LocalDiscogsApi.Exceptions;
using Discogs = LocalDiscogsApi.Models.Discogs;

namespace LocalDiscogsApi.Clients
{
    public interface IDiscogsClient
    {
        Task<Discogs.InventoryResponse> GetInventoryPageForUser(string userName, int pageNum);
        Task<Discogs.Inventory> GetInventoryForUser(string userName);

        Task<Discogs.WantlistResponse> GetWantlistPageForUser(string userName, int pageNum);
        Task<Discogs.Wantlist> GetWantlistForUser(string userName);
    }

    public class DiscogsClient : IDiscogsClient
    {
        private readonly HttpClient httpClient;

        public DiscogsClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Discogs.InventoryResponse> GetInventoryPageForUser(string userName, int pageNum)
        {
            if (pageNum < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(pageNum), pageNum.ToString(), $"{nameof(pageNum)} must be greater than 0");
            }

            var queryParams = new Dictionary<string, string>
            {
                { "sort", "listed" },
                { "sort_order", "desc" },
                { "per_page", "100" },
                { "page", pageNum.ToString() }
            };

            string requestUrl = QueryHelpers.AddQueryString($"users/{userName}/inventory", queryParams);

            using (HttpResponseMessage httpResponse = await httpClient.GetAsync(requestUrl))
            {
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new RestRequestException(httpResponse, await httpResponse.Content?.ReadAsStringAsync());
                }

                string responseString = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Discogs.InventoryResponse>(responseString);
            }
        }

        //public async Task<Discogs.Inventory> GetInventoryForUser(string userName, IEnumerable<Discogs.Listing> currentInventory)
        //{
        //    Discogs.InventoryResponse pageOne = await GetInventoryPageForUser(userName, 1);

        //    if (pageOne.Pagination.Items == currentInventory.Count()
        //        && pageOne.Items.FirstOrDefault() == currentInventory.FirstOrDefault())
        //    {
        //        // inventory hasn't changed since last check.
        //        return new Discogs.Inventory(currentInventory.ToArray());
        //    }
        //    else
        //    {
        //        var getPageTasks = new List<Task<Discogs.InventoryResponse>>();

        //        for (int i = 2; i < pageOne.Pagination.Pages; i++)
        //        {
        //            getPageTasks.Add(GetInventoryPageForUser(userName, i));
        //        }

        //        await Task.WhenAll(getPageTasks);

        //        Discogs.Listing[] allItems = pageOne.Items.Concat(getPageTasks?.SelectMany(t => t.Result.Items)).ToArray();

        //        return new Discogs.Inventory(allItems);
        //    }
        //}

        // todo: make this private.
        // have public method pass in existing list. 
        // call GetInventoryPageForUser(userName, 1) - compare counts and first item(s) until matching?
        public async Task<Discogs.Inventory> GetInventoryForUser(string userName)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "sort", "listed" },
                { "sort_order", "desc" },
                { "per_page", "100" },
                { "page", "1" }
            };

            string requestUrl = QueryHelpers.AddQueryString($"users/{userName}/inventory", queryParams);

            // todo: massive optimisation potential by diff checking with existing inventory
            // pagination.items = count
            // there will be situations where listing(s) far into the pages has been delisted.
            // might need some binary search algorithm to find the delisted item(s) and diff compare accordingly
            Discogs.Listing[] listings = await GetPageItems<Discogs.Listing, Discogs.InventoryResponse>(requestUrl);

            return new Discogs.Inventory(listings);
        }

        public async Task<Discogs.WantlistResponse> GetWantlistPageForUser(string userName, int pageNum)
        {
            if (pageNum < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(pageNum), pageNum.ToString(), $"{nameof(pageNum)} must be greater than 0");
            }

            var queryParams = new Dictionary<string, string>
            {
                { "per_page", "100" },
                { "page", pageNum.ToString() }
            };

            string requestUrl = QueryHelpers.AddQueryString($"users/{userName}/inventory", queryParams);

            using (HttpResponseMessage httpResponse = await httpClient.GetAsync(requestUrl))
            {
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new RestRequestException(httpResponse, await httpResponse.Content?.ReadAsStringAsync());
                }

                string responseString = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Discogs.WantlistResponse>(responseString);
            }
        }

        public async Task<Discogs.Wantlist> GetWantlistForUser(string userName)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "per_page", "100" },
                { "page", "1" }
            };

            string requestUrl = QueryHelpers.AddQueryString($"users/{userName}/wants", queryParams);

            Discogs.Want[] wants = await GetPageItems<Discogs.Want, Discogs.WantlistResponse>(requestUrl);

            return new Discogs.Wantlist(wants);
        }

        private async Task<TItem[]> GetPageItems<TItem, TResponse>(string requestUrl)
            where TResponse : Discogs.PaginatedResponse<TItem>
        {
            TResponse response;

            using (HttpResponseMessage httpResponse = await httpClient.GetAsync(requestUrl))
            {
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new RestRequestException(httpResponse, await httpResponse.Content?.ReadAsStringAsync());
                }

                string responseString = await httpResponse.Content.ReadAsStringAsync();
                response = JsonConvert.DeserializeObject<TResponse>(responseString);
            }

            if (!string.IsNullOrEmpty(response.Pagination.Urls.Next))
            {
                TItem[] nextItems = await GetPageItems<TItem, TResponse>(response.Pagination.Urls.Next);

                return response.Items.Concat(nextItems).ToArray();
            }
            else
            {
                return response.Items;
            }
        }
    }
}