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
            _catalogService = catalogService;
        }

        public async Task<Guid> CreateAuctionAsync(Auction auction)
        {
            // Henter produktet
            var product = await _catalogService.GetAvailableProductAsync(auction.ProductId);
            if (product == null)
            {
                throw new ArgumentException("The product does not exist, or has not yet been set to available by admin");
            }

            // Genererer AuctionId, hvis det ikke allerede er sat
            if (auction.AuctionId == Guid.Empty)
                auction.AuctionId = Guid.NewGuid();

            // Opdaterer produkt-status i CatalogService
            await _catalogService.SetProductInAuctionAsync(auction.ProductId, auction.AuctionId);

            // Sætter auktionens start-, sluttid, status og produkt
            auction.Product = product;
            auction.Status = "Active";
            auction.Bids = new List<Bid>();
            auction.StartTime = DateTime.UtcNow;

            if (auction.EndTime == default)
                auction.EndTime = DateTime.UtcNow.AddDays(1);

            // Opretter auktionen i databasen
            await _auctions.InsertOneAsync(auction);

            return auction.AuctionId; // Returnerer Guid
        }

        public async Task<List<Auction>> GetAuctions()
        {
            return await _auctions.Find(_ => true).ToListAsync();
        }

        public async Task<Auction> GetAuctionById(Guid auctionId)
        {
            return await _auctions.Find(a => a.AuctionId == auctionId).FirstOrDefaultAsync();
        }

        public async Task<List<Auction>> GetActiveAuctions()
        {
            return await _auctions.Find(a => a.Status == "Active").ToListAsync();
        }

        public async Task<List<Auction>> GetExpiredAuctionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _auctions.Find(a => a.EndTime <= now && a.Status == "Active").ToListAsync();
        }

        public async Task<string> DeleteAuctionAsync(Guid auctionId)
        {
            // 1. Hent auktionen for at få fat i ProductId
            var auction = await GetAuctionById(auctionId);
            if (auction == null)
            {
                return "Auction not found.";
            }

            // 2. Slet auktionen fra databasen (ingen behov for at tjekke DeletedCount)
            await _auctions.DeleteOneAsync(a => a.AuctionId == auctionId);

            // 3. Opdater produktstatus i CatalogService til "Available"
            var productUpdated = await _catalogService.SetProductStatusToAvailableAsync(auction.ProductId);
            if (!productUpdated)
            {
                return "Auction deleted, but failed to update product status.";
            }

            return $"Auction with ID: {auctionId} deleted successfully.";
        }

        public async Task<bool> ProcessBidAsync(Bid bid)
        {
            // Bygger en opdaterings-definition, som tilføjer det nye bud til Bids-listen i auktionen.
            var update = Builders<Auction>.Update.Push(a => a.Bids, bid);

            // Opdaterer auktionen i databasen
            var updateResult = await _auctions.UpdateOneAsync(
                a => a.AuctionId == bid.AuctionId,
                update
            );

            return updateResult.ModifiedCount > 0;
        }

        public async Task EndAuctionAsync(Auction auction)
        {
            // Find det højeste bud, hvis der er nogen
            var highestBid = auction.Bids?.OrderByDescending(b => b.Amount).FirstOrDefault();

            if (highestBid != null)
            {
                // Hvis der er et bud, markeres auktionen som 'Completed'
                auction.Status = "Completed";
                auction.WinningBid = highestBid;

                // Opdater produktstatus i CatalogService til 'Sold'
                var updated = await _catalogService.SetProductStatusToSoldAsync(auction.ProductId);
                if (!updated)
                {
                    // Log en fejl og kast en undtagelse, hvis opdateringen fejler
                    throw new Exception($"Failed to update product status to 'Sold' for ProductId: {auction.ProductId}");
                }
            }
            else
            {
                // Hvis der ikke er nogen bud, markeres auktionen som 'Failed'
                auction.Status = "Failed";

                // Opdater produktstatus i CatalogService til 'FailedInAuction'
                var updated = await _catalogService.SetProductStatusToFailedAuctionAsync(auction.ProductId);
                if (!updated)
                {
                    throw new Exception($"Failed to update product status to 'FailedInAuction' for ProductId: {auction.ProductId}");
                }
            }

            // Gem den opdaterede auktion tilbage i databasen
            var filter = Builders<Auction>.Filter.Eq(a => a.AuctionId, auction.AuctionId);
            await _auctions.ReplaceOneAsync(filter, auction);
        }


    }
}