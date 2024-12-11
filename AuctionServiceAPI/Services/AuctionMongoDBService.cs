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
    }
}