using AuctionServiceAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Interfaces
{
    public interface IAuctionService
    {
        Task<Guid> CreateAuctionAsync(Auction auction);
        Task<List<Auction>> GetAuctions();
        Task<Auction> GetAuctionById(Guid auctionId);
        Task<List<Auction>> GetActiveAuctions();
        Task DeleteAuction(Guid auctionId);
        Task<bool> ProcessBidAsync(Bid bid);
        Task<List<Auction>> GetExpiredAuctionsAsync();
        Task EndAuctionAsync(Auction auction);
    }
}