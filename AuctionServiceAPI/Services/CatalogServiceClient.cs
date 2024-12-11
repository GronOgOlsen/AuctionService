using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Services
{
    public class CatalogServiceClient : ICatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatalogServiceClient> _logger;

        public CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductDTO>> GetApprovedProductsAsync()
        {
            _logger.LogInformation("Fetching approved products from CatalogService...");
            var response = await _httpClient.GetAsync("products/approved"); // Brug base-URL
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response from CatalogService: {Content}", content);

            return JsonSerializer.Deserialize<List<ProductDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task UpdateProductStatusAsync(Guid productId, string status)
        {
            _logger.LogInformation("Updating product status for ProductId: {ProductId} to {Status}", productId, status);
            var response = await _httpClient.PutAsJsonAsync($"product/{productId}/status/{status}", new { });
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Product status updated successfully.");
        }
    }
}
