using AuctionServiceAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Interfaces
{
    public interface IAuctionService
    {
        Task CreateAuction(Auction auction);
        Task<List<Auction>> GetAuctions();
    }
}