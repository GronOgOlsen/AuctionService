using AuctionServiceAPI.Models;

namespace AuctionServiceAPI.Interfaces
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
}