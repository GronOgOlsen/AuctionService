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

        // Opret en auktion (kun tilgængelig for administratorer)
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
                    _logger.LogWarning("ProductId: {ProductId} is not a product or not available for auction.", auction.ProductId);
                    return BadRequest("The product does not exist, or has not yet been set to available by admin");
                }

                // 2) Generer AuctionId, hvis det ikke allerede er sat
                if (auction.AuctionId == Guid.Empty)
                    auction.AuctionId = Guid.NewGuid();

                // 3) Opdater produkt-status i CatalogService 
                await _catalogService.SetProductInAuctionAsync(auction.ProductId, auction.AuctionId);

                // 4) Sæt auktionens start-, sluttid, status og produkt
                auction.Product = product;
                auction.Status = "Active";
                auction.Bids = new List<Bid>();
                auction.StartTime = DateTime.UtcNow;
                // Hvis brugeren ikke har sendt en EndTime (dvs. den er default(DateTime)), sætter vi den til nu + 1 dag
                if (auction.EndTime == default)
                {
                    auction.EndTime = DateTime.UtcNow.AddDays(1);
                }

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

        // Hent alle auktioner (tilgængelig for både brugere og administratorer)
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

        // Hent en specifik auktion (tilgængelig for både brugere og administratorer)
        [HttpGet("auction/{id}")]
        [Authorize(Roles = "1, 2")]
        public async Task<IActionResult> GetAuctionById(Guid id)
        {
            _logger.LogInformation("Fetching auction with ID: {AuctionId}", id);
            try
            {
                var auction = await _auctionService.GetAuctionById(id);
                if (auction == null)
                {
                    _logger.LogWarning("Auction with ID: {AuctionId} not found.", id);
                    return NotFound("Auction not found.");
                }

                _logger.LogInformation("Successfully fetched auction with ID: {AuctionId}.", id);
                return Ok(auction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching auction with ID: {AuctionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the auction.");
            }
        }

        // Opdater en auktion (kun tilgængelig for administratorer)
        // [HttpPut("auction/{id}")]
        // [Authorize(Roles = "2")]
        // public async Task<IActionResult> UpdateAuction(Guid id, Auction auction)
        // {
        //     if (id != auction.AuctionId)
        //     {
        //         _logger.LogWarning("AuctionId in URL does not match AuctionId in body.");
        //         return BadRequest("AuctionId in URL does not match AuctionId in body.");
        //     }

        //     _logger.LogInformation("Attempting to update auction with ID: {AuctionId}", id);
        //     try
        //     {
        //         var updated = await _auctionService.UpdateAuction(auction);
        //         if (!updated)
        //         {
        //             _logger.LogWarning("Auction with ID: {AuctionId} not found.", id);
        //             return NotFound("Auction not found.");
        //         }

        //         _logger.LogInformation("Auction with ID: {AuctionId} updated successfully.", id);
        //         return Ok("Auction with ID: {AuctionId} updated successfully.", id);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error occurred while updating auction with ID: {AuctionId}", id);
        //         return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the auction.");
        //     }
        // }
    }
}
