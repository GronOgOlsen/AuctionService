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
            var response = await _httpClient.GetAsync("products/available"); // Ret stien
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response from CatalogService: {Content}", content);

            return JsonSerializer.Deserialize<List<ProductDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task UpdateProductStatusAsync(Guid productId, string status)
        {
            string endpoint = status.ToLower() switch
            {
                "prepare-auction" => $"product/{productId}/prepare-auction",
                "in-auction" => $"product/{productId}/set-in-auction",
                "sold" => $"product/{productId}/set-sold",
                _ => throw new ArgumentException("Invalid status")
            };

            var response = await _httpClient.PutAsync(endpoint, null);
            response.EnsureSuccessStatusCode();
        }
    }
}
