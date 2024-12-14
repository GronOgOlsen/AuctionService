using Microsoft.AspNetCore.Mvc;
using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace AuctionServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly ICatalogService _catalogService;
        private readonly ILogger<AuctionController> _logger;

        public AuctionController(IAuctionService auctionService, ICatalogService catalogService, ILogger<AuctionController> logger)
        {
            _auctionService = auctionService;
            _catalogService = catalogService;
            _logger = logger;
        }

        [HttpPost("create-auction")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> CreateAuction([FromBody] Auction auction)
        {
            _logger.LogInformation("Attempting to create an auction for ProductId: {ProductId}", auction.ProductId);

            try
            {
                // 1) Hent produktet 
                var product = await _catalogService.GetAvailableProductAsync(auction.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("ProductId: {ProductId} is not available for auction.", auction.ProductId);
                    return BadRequest("Product is not available for auction.");
                }

                // 2) Generer AuctionId, hvis det ikke allerede er sat
                if (auction.AuctionId == Guid.Empty)
                    auction.AuctionId = Guid.NewGuid();

                // 3) Opdater produkt-status i CatalogService 
                await _catalogService.SetProductInAuctionAsync(auction.ProductId, auction.AuctionId);

                // 4) Udfyld resten af auktionen
                auction.Product = product;
                auction.Status = "Active";
                auction.Bids = new List<Bid>();
                auction.StartTime = DateTime.UtcNow;
                auction.EndTime = DateTime.UtcNow.AddDays(1);

                 // 5) Opret selve auktionen i AuctionService-databasen
                await _auctionService.CreateAuction(auction);

                _logger.LogInformation("Auction created successfully for ProductId: {ProductId}", auction.ProductId);
                return Ok("Auction created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating auction for ProductId: {ProductId}", auction.ProductId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the auction.");
            }
        }


        [HttpGet("auctions")]
        [Authorize(Roles = "1, 2")]
        public async Task<IActionResult> GetAuctions()
        {
            _logger.LogInformation("Fetching list of auctions...");
            try
            {
                var auctions = await _auctionService.GetAuctions();
                _logger.LogInformation("Successfully fetched {Count} auctions.", auctions.Count);
                return Ok(auctions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching auctions.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching auctions.");
            }
        }
    }
}
