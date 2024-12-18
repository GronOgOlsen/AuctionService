using Microsoft.AspNetCore.Mvc;
using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

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

        // Opretter en auktion (kun tilgængelig for administratorer)
        [HttpPost("create-auction")]
        [Authorize(Roles = "2")]
        public async Task<ActionResult<string>> CreateAuction([FromBody] Auction auction)
        {
            _logger.LogInformation("Attempting to create an auction for ProductId: {ProductId}", auction.ProductId);

            try
            {
                var auctionId = await _auctionService.CreateAuctionAsync(auction); 
                _logger.LogInformation("Auction created successfully with AuctionId: {AuctionId}", auctionId);
                return Ok($"Auction with id: {auctionId} created successfully."); 
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating auction for ProductId: {ProductId}", auction.ProductId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the auction.");
            }
        }


        // Henter alle aktive auktioner (tilgængelig for både brugere og administratorer)
        [HttpGet("active-auctions")]
        [Authorize(Roles = "1, 2")]
        public async Task<IActionResult> GetActiveAuctions()
        {
            _logger.LogInformation("Fetching list of active auctions...");
            try
            {
                var auctions = await _auctionService.GetActiveAuctions();
                _logger.LogInformation("Successfully fetched {Count} active auctions.", auctions.Count);
                return Ok(auctions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching active auctions.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching active auctions.");
            }
        }

        // Henter alle auktioner (tilgængelig for både brugere og administratorer)
        [HttpGet]
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

        // Henter en specifik auktion (tilgængelig for både brugere og administratorer)
        [HttpGet("{id}")]
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

        // Sletter en auktion (kun tilgængelig for administratorer)
        [HttpDelete("{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> DeleteAuction(Guid id)
        {
            _logger.LogInformation("Attempting to delete auction with ID: {AuctionId}", id);
            try
            {
                var auction = await _auctionService.GetAuctionById(id);
                if (auction == null)
                {
                    _logger.LogWarning("Auction with ID: {AuctionId} not found.", id);
                    return NotFound("Auction not found.");
                }

                // 1) Slet auktionen fra AuctionService-databasen
                await _auctionService.DeleteAuction(id);

                // 2) Opdater produkt-status i CatalogService 
                await _catalogService.SetProductStatusToAvailableAsync(auction.ProductId);

                _logger.LogInformation("Auction with ID: {AuctionId} deleted successfully.", id);
                return Ok("Auction deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting auction with ID: {AuctionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the auction.");
            }
        }
       

        [HttpGet("version")]
        public async Task<Dictionary<string, string>> GetVersion()
        {
            var properties = new Dictionary<string, string>();
            var assembly = typeof(Program).Assembly;
            properties.Add("service", "AuctionService");
            var ver = FileVersionInfo.GetVersionInfo(typeof(Program)
            .Assembly.Location).ProductVersion;
            properties.Add("version", ver!);
            try
            {
                var hostName = System.Net.Dns.GetHostName();
                var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
                var ipa = ips.First().MapToIPv4().ToString();
                properties.Add("hosted-at-address", ipa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                properties.Add("hosted-at-address", "Could not resolve IP-address");
            }
            return properties;
        }
    }
}
