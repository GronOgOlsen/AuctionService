using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AuctionServiceAPI.Interfaces;
using Microsoft.Extensions.Logging;
using AuctionServiceAPI.Models;

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

        public async Task<ProductDTO> GetAvailableProductAsync(Guid productId)
        {
            _logger.LogInformation("Fetching available product with ProductId: {ProductId}", productId);
            var response = await _httpClient.GetAsync($"product/{productId}/available");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ProductId: {ProductId} is not available. Status code: {StatusCode}", productId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductDTO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task SetProductInAuctionAsync(Guid productId)
        {
            _logger.LogInformation("Setting ProductId: {ProductId} status to 'InAuction'.", productId);
            var response = await _httpClient.PutAsync($"api/catalog/product/{productId}/set-in-auction", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
