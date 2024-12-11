using AuctionServiceAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Interfaces
{
    public interface ICatalogService
    {
        Task<bool> IsProductAvailableAsync(Guid productId);
         Task SetProductInAuctionAsync(Guid productId);
    }
}