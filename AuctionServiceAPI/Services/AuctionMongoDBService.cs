using AuctionServiceAPI.Interfaces;
using AuctionServiceAPI.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Services
{
    public class AuctionMongoDBService : IAuctionService
    {
        private readonly IMongoCollection<Auction> _auctions;

        public AuctionMongoDBService(IMongoDatabase database)
        {
            _auctions = database.GetCollection<Auction>("Auctions");
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
    }
}