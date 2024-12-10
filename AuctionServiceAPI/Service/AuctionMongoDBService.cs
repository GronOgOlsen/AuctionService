using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuctionServiceAPI.Models;
using AuctionServiceAPI.Service;

namespace AuctionServiceAPI.Models
{
    public interface IAuctionService
    {
        Task<Auction?> GetAuction(Guid auctionId);
        Task<IEnumerable<Auction>?> GetAuctionList();
        Task<Guid> AddAuction(Auction auction);
        Task<long> UpdateAuction(Auction auction);
        Task<long> DeleteAuction(Guid auctionId);
        Task ProcessBidAsync (Bid bid);
    }

    public class AuctionMongoDBService : IAuctionService
    {
        private readonly ILogger<AuctionMongoDBService> _logger;
        private readonly IMongoCollection<Auction> _auctionCollection;
        public async Task ProcessBidAsync(Bid bid)
        {
            var filter = Builders<Auction>.Filter.Eq(a => a.auctionId, bid.auctionId);
            var update = Builders<Auction>.Update.Push(a => a.bids, bid);
            var result = await _auctionCollection.UpdateOneAsync(filter, update);

            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                _logger.LogInformation($"Bid processed and added to auction {bid.auctionId}");
            }
            else
            {
                _logger.LogWarning($"Failed to process bid for auction {bid.auctionId}");
            }
        }

        public AuctionMongoDBService(ILogger<AuctionMongoDBService> logger, MongoDBContext dbContext, IConfiguration configuration)
        {
            var collectionName = configuration["collectionName"];
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ApplicationException("AuctionCollectionName is not configured.");
            }

            _logger = logger;
            _auctionCollection = dbContext.GetCollection<Auction>(collectionName);
            _logger.LogInformation($"Collection name: {collectionName}");
        }

        public async Task<Auction?> GetAuction(Guid auctionId)
        {
            var filter = Builders<Auction>.Filter.Eq(x => x._id, auctionId);
            return await _auctionCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Auction>?> GetAuctionList()
        {
            return await _auctionCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Guid> AddAuction(Auction auction)
        {
            auction.auctionId = Guid.NewGuid();
            await _auctionCollection.InsertOneAsync(auction);
            return auction.auctionId;
        }

        public async Task<long> UpdateAuction(Auction auction)
        {
            var filter = Builders<Auction>.Filter.Eq(x => x._id, auction._id);
            var result = await _auctionCollection.ReplaceOneAsync(filter, auction);
            return result.ModifiedCount;
        }

        public async Task<long> DeleteAuction(Guid auctionId)
        {
            var filter = Builders<Auction>.Filter.Eq(x => x._id, auctionId);
            var result = await _auctionCollection.DeleteOneAsync(filter);
            return result.DeletedCount;
        }
    }
}