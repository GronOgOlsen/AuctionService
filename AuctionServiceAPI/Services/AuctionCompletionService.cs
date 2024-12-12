using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Services
{
    public class AuctionMongoDBService : IAuctionService
    {
        private readonly IMongoCollection<Auction> _auctions;
        private readonly ICatalogService _catalogService;

        public AuctionMongoDBService(IMongoDatabase database, ICatalogService catalogService)
        {
            _auctions = database.GetCollection<Auction>("Auctions");
            _catalogService = catalogService; // Initialiser ICatalogService
        }

        public async Task CreateAuction(Auction auction)
        {
            await _auctions.InsertOneAsync(auction);
        }

        public async Task<List<Auction>> GetAuctions()
        {
            return await _auctions.Find(_ => true).ToListAsync();
        }

        public async Task<bool> ProcessBidAsync(Bid bid)
        {
            // Find auktionen
            var auction = await _auctions.Find(a => a.AuctionId == bid.AuctionId).FirstOrDefaultAsync();

            if (auction == null)
            {
                // Auktionen findes ikke
                return false;
            }

            // Tjek om buddet er højere end det nuværende højeste bud
            var highestBid = auction.Bids?.Count > 0
                ? auction.Bids.Max(b => b.Amount)
                : auction.StartingPrice;

            if (bid.Amount <= highestBid)
            {
                // Buddet er ikke højere end det nuværende højeste bud
                return false;
            }

            // Tilføj buddet til listen
            auction.Bids ??= new List<Bid>();
            auction.Bids.Add(bid);

            // Opdater auktionen i databasen
            var updateResult = await _auctions.ReplaceOneAsync(
                a => a.AuctionId == bid.AuctionId,
                auction
            );

            return updateResult.ModifiedCount > 0;
        }

        public async Task<List<Auction>> GetExpiredAuctionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _auctions.Find(a => a.EndTime <= now && a.Status == "Active").ToListAsync();
        }

        public async Task EndAuctionAsync(Auction auction)
        {
            var highestBid = auction.Bids?.OrderByDescending(b => b.Amount).FirstOrDefault();

            if (highestBid != null)
            {
                auction.Status = "Completed";
                auction.WinningBid = highestBid;

                // Opdater produktstatus i CatalogService
                var updated = await _catalogService.SetProductStatusToSoldAsync(auction.ProductId);
                if (!updated)
                {
                    throw new Exception($"Failed to update product status to 'Sold' for ProductId: {auction.ProductId}");
                }
            }
            else
            {
                auction.Status = "Failed"; // Ingen bud
            }

            // Gem opdateret auktion
            var filter = Builders<Auction>.Filter.Eq(a => a.AuctionId, auction.AuctionId);
            await _auctions.ReplaceOneAsync(filter, auction);
        }
    }
}
