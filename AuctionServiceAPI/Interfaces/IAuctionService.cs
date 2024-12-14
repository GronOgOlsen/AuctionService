using AuctionServiceAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Interfaces
{
    public interface IAuctionService
    {
        Task CreateAuction(Auction auction);
        Task<List<Auction>> GetAuctions();
        Task<Auction> GetAuctionById(Guid auctionId);
        Task<bool> ProcessBidAsync(Bid bid);
        Task<List<Auction>> GetExpiredAuctionsAsync();
        Task EndAuctionAsync(Auction auction);
    }
}