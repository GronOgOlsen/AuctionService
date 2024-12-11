using Microsoft.AspNetCore.Mvc;
using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly ICatalogService _catalogService;

        public AuctionController(IAuctionService auctionService, ICatalogService catalogService)
        {
            _auctionService = auctionService;
            _catalogService = catalogService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] Auction auction)
        {
            // Validate product status from CatalogService
            var approvedProducts = await _catalogService.GetApprovedProductsAsync();
            if (!approvedProducts.Exists(p => p.ProductId == auction.ProductId))
            {
                return BadRequest("Product is not approved for auction.");
            }

            // Update product status in CatalogService
            await _catalogService.UpdateProductStatusAsync(auction.ProductId, "InAuction");

            // Create auction
            await _auctionService.CreateAuction(auction);
            return Ok("Auction created successfully");
        }

        [HttpGet]
        public async Task<ActionResult<List<Auction>>> GetAuctions()
        {
            var auctions = await _auctionService.GetAuctions();
            return Ok(auctions);
        }
    }
}