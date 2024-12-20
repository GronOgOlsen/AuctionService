using System.Net.Http;
using System.Text;
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
            var response = await _httpClient.GetAsync($"api/catalog/product/{productId}/available");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ProductId: {ProductId} is not available. Status code: {StatusCode}", productId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductDTO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task SetProductInAuctionAsync(Guid productId, Guid auctionId)
        {
            _logger.LogInformation("Setting ProductId: {ProductId} status to 'InAuction' with AuctionId: {AuctionId}", productId, auctionId);

            // Serializér auctionId med System.Text.Json
            var json = JsonSerializer.Serialize(auctionId);

            // Opret request-body med den serialiserede JSON
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Udfør PUT-anmodning
            var response = await _httpClient.PutAsync($"api/catalog/product/{productId}/set-in-auction", content);

            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> SetProductStatusToSoldAsync(Guid productId)
        {
            _logger.LogInformation("Setting ProductId: {ProductId} status to 'Sold'.", productId);
            var response = await _httpClient.PutAsync($"api/catalog/product/{productId}/set-sold", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully set ProductId: {ProductId} to 'Sold'.", productId);
                return true;
            }

            _logger.LogWarning("Failed to set ProductId: {ProductId} to 'Sold'. Status code: {StatusCode}", productId, response.StatusCode);
            return false;
        }

        public async Task<bool> SetProductStatusToFailedAuctionAsync(Guid productId)
        {
            _logger.LogInformation("Setting ProductId: {ProductId} status to 'FailedInAuction'.", productId);
            var response = await _httpClient.PutAsync($"api/catalog/product/{productId}/set-failed-in-auction", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully set ProductId: {ProductId} to 'FailedInAuction'.", productId);
                return true;
            }

            _logger.LogWarning("Failed to set ProductId: {ProductId} to 'FailedInAuction'. Status code: {StatusCode}", productId, response.StatusCode);
            return false;
        }

        public async Task<bool> SetProductStatusToAvailableAsync(Guid productId)
        {
            _logger.LogInformation("Setting ProductId: {ProductId} status to 'Available'.", productId);
            var response = await _httpClient.PutAsync($"api/catalog/product/{productId}/prepare-auction", null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully set ProductId: {ProductId} to 'Available'.", productId);
                return true;
            }

            _logger.LogWarning("Failed to set ProductId: {ProductId} to 'Available'. Status code: {StatusCode}", productId, response.StatusCode);
            return false;
        }
    }
}
