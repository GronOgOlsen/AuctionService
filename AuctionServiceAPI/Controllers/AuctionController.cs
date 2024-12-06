using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuctionServiceAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly ILogger<AuctionController> _logger;

        public AuctionController(ILogger<AuctionController> logger)
        {
            _logger = logger;
        }

        // CRUD: Oprette en auktion
        [HttpPost]
        public async Task<ActionResult<Auction>> CreateAuction(Auction auction)
        {
            try
            {
                // Simulere oprettelse i database (skal erstattes med rigtig MongoDB-lagring)
                _logger.LogInformation("Creating new auction");
                auction._id = Guid.NewGuid();
                // Returnere oprettet auktion
                return CreatedAtAction(nameof(GetAuctionById), new { id = auction._id }, auction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error creating auction");
                return StatusCode(500, "Internal server error");
            }
        }

        // CRUD: Hent auktion via ID
        [HttpGet("{id}")]
        public ActionResult<Auction> GetAuctionById(Guid id)
        {
            // Simulere hentning fra database (skal erstattes med MongoDB hentning)
            var auction = new Auction { _id = id, startTime = DateTime.Now, endTime = DateTime.Now.AddHours(1) };
            if (auction == null)
            {
                return NotFound();
            }
            return Ok(auction);
        }

        // CRUD: Hent alle auktioner
        [HttpGet]
        public ActionResult<List<Auction>> GetAllAuctions()
        {
            // Simulere hentning af alle auktioner (skal erstattes med MongoDB hentning)
            var auctions = new List<Auction>
            {
                new Auction { _id = Guid.NewGuid(), startTime = DateTime.Now, endTime = DateTime.Now.AddHours(1) },
                new Auction { _id = Guid.NewGuid(), startTime = DateTime.Now.AddHours(2), endTime = DateTime.Now.AddHours(3) }
            };
            return Ok(auctions);
        }

        // CRUD: Opdater auktion
        [HttpPut("{id}")]
        public ActionResult UpdateAuction(Guid id, Auction auction)
        {
            try
            {
                // Simulere opdatering i database (skal erstattes med MongoDB opdatering)
                if (auction._id != id)
                {
                    return BadRequest("Auction ID mismatch");
                }

                _logger.LogInformation($"Updating auction with ID: {id}");
                return NoContent(); // Returnere 204 No Content for succesfuld opdatering
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error updating auction");
                return StatusCode(500, "Internal server error");
            }
        }

        // CRUD: Slet auktion
        [HttpDelete("{id}")]
        public ActionResult DeleteAuction(Guid id)
        {
            try
            {
                // Simulere sletning fra database (skal erstattes med MongoDB sletning)
                _logger.LogInformation($"Deleting auction with ID: {id}");
                return NoContent(); // Returnere 204 No Content for succesfuld sletning
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error deleting auction");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
