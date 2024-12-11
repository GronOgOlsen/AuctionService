using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using System.Collections.Generic;

namespace AuctionServiceAPI.Services
{
    public class CatalogServiceClient : ICatalogService
    {
        private readonly HttpClient _httpClient;

        public CatalogServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductDTO>> GetApprovedProductsAsync()
        {
            var response = await _httpClient.GetAsync("http://catalogservice:82/api/catalog/products/approved");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task UpdateProductStatusAsync(Guid productId, string status)
        {
            var response = await _httpClient.PutAsync(
                $"http://catalogservice:82/api/catalog/product/{productId}/status/{status}",
                null);
            response.EnsureSuccessStatusCode();
        }
    }
}