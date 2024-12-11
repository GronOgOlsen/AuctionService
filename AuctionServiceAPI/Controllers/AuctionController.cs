using Microsoft.AspNetCore.Mvc;
using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] Auction auction)
        {
            _logger.LogInformation("Attempting to create an auction for ProductId: {ProductId}", auction.ProductId);

            try
            {
                // Validate product status from CatalogService
                var availableProducts = await _catalogService.GetApprovedProductsAsync();
                if (!availableProducts.Exists(p => p.ProductId == auction.ProductId))
                {
                    _logger.LogWarning("ProductId: {ProductId} is not available for auction.", auction.ProductId);
                    return BadRequest("Product is not available for auction.");
                }

                // Prepare the product for auction in CatalogService
                _logger.LogInformation("Preparing ProductId: {ProductId} for auction.", auction.ProductId);
                await _catalogService.UpdateProductStatusAsync(auction.ProductId, "prepare-auction");

                // Update the product status to "InAuction"
                _logger.LogInformation("Setting ProductId: {ProductId} as 'InAuction'.", auction.ProductId);
                await _catalogService.UpdateProductStatusAsync(auction.ProductId, "in-auction");

                // Create the auction
                _logger.LogInformation("Creating auction for ProductId: {ProductId}.", auction.ProductId);
                await _auctionService.CreateAuction(auction);

                _logger.LogInformation("Auction created successfully for ProductId: {ProductId}.", auction.ProductId);
                return Ok("Auction created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating auction for ProductId: {ProductId}.", auction.ProductId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the auction.");
            }
        }
        [HttpGet]
        public async Task<ActionResult<List<Auction>>> GetAuctions()
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
