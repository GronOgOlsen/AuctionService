using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AuctionServiceAPI.Interfaces;
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

        public async Task<bool> IsProductAvailableAsync(Guid productId)
        {
            _logger.LogInformation("Checking if ProductId: {ProductId} is available.", productId);
            var response = await _httpClient.GetAsync($"api/catalog/product/{productId}/available");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to validate ProductId: {ProductId}. Status code: {StatusCode}", productId, response.StatusCode);
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content);
        }

        public async Task SetProductInAuctionAsync(Guid productId)
        {
            _logger.LogInformation("Setting ProductId: {ProductId} status to 'InAuction'.", productId);
            var response = await _httpClient.PutAsync($"api/catalog/product/{productId}/set-in-auction", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
