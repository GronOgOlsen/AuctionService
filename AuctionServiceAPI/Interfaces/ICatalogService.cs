using AuctionServiceAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Interfaces
{
    public interface ICatalogService
    {
        Task<ProductDTO> GetAvailableProductAsync(Guid productId);
         Task SetProductInAuctionAsync(Guid productId);
    }
}