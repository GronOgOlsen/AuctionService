using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuctionServiceAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AuctionServiceAPI.Service;
using Microsoft.AspNetCore.Authorization;


namespace AuctionServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly ILogger<AuctionController> _logger;

        public AuctionController(IAuctionService auctionService, ILogger<AuctionController> logger)
        {
            _auctionService = auctionService;
            _logger = logger;
        }

        private string GetIpAddress()
        {
            var hostName = Dns.GetHostName();
            var ips = Dns.GetHostAddresses(hostName);
            var ipaddr = ips.First().MapToIPv4().ToString();
            return ipaddr;
        }

        [HttpGet("{_id}")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<Auction>> GetAuction(Guid _id)
        {
            _logger.LogInformation(1, $"Auction service responding from {GetIpAddress()}");

            var auction = await _auctionService.GetAuction(_id);
            if (auction == null)
            {
                return NotFound();
            }
            return auction;
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<IEnumerable<Auction>>> GetAuctionList()
        {
            _logger.LogInformation(1, $"Auction service responding from {GetIpAddress()}");

            var auctionList = await _auctionService.GetAuctionList();

            if (auctionList == null)
            {
                throw new ApplicationException("Auction list is null");
            }
            return Ok(auctionList);
        }

        [HttpPost]
        [Authorize(Roles = "2")]
        public async Task<ActionResult<int>> AddAuction(Auction auction)
        {
            try
            {
                _logger.LogInformation($"Received request to add auction: {auction._id}");
                _logger.LogInformation(1, $"Auction service responding from {GetIpAddress()}");

                var auctionId = await _auctionService.AddAuction(auction);
                return Ok($"Auction with id {auctionId}, was added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error occurred while adding an auction.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpPut("{_id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> UpdateAuction(Guid _id, Auction auction)
        {
            _logger.LogInformation(1, $"auction Service responding from {GetIpAddress()}");

            if (_id != auction._id)
            {
                return BadRequest("Bad request, ids not matching");
            }

            var result = await _auctionService.UpdateAuction(auction);
            if (result == 0)
            {
                return NotFound("Auction not found");
            }

            return Ok($"Auction with id {_id} updated successfully");
        }

        [HttpDelete("{_id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> DeleteAuction(Guid _id)
        {
            _logger.LogInformation(1, $"auction service responding from {GetIpAddress()}");

            var result = await _auctionService.DeleteAuction(_id);
            if (result == 0)
            {
                return NotFound();
            }

            return Ok($"Auction with id {_id} deleted successfully");
        }
    }
}