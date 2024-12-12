using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;

namespace AuctionServiceAPI.Services
{
    public class AuctionCompletionService : BackgroundService
    {
        private readonly IAuctionService _auctionService;
        private readonly ILogger<AuctionCompletionService> _logger;

        public AuctionCompletionService(IAuctionService auctionService, ILogger<AuctionCompletionService> logger)
        {
            _auctionService = auctionService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AuctionCompletionService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Checking for expired auctions...");

                    // Hent alle udløbne auktioner
                    var expiredAuctions = await _auctionService.GetExpiredAuctionsAsync();

                    foreach (var auction in expiredAuctions)
                    {
                        try
                        {
                            _logger.LogInformation($"Ending auction with ID: {auction.AuctionId}");

                            // Afslut auktionen ved at kalde metoden i IAuctionService
                            await _auctionService.EndAuctionAsync(auction);

                            _logger.LogInformation($"Auction with ID: {auction.AuctionId} ended successfully.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to process auction with ID: {auction.AuctionId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while checking or processing expired auctions.");
                }

                // Vent et minut før næste kontrol
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("AuctionCompletionService is stopping.");
        }
    }
}
