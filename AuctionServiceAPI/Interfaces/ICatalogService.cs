using AuctionServiceAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Interfaces
{
    public interface ICatalogService
    {
        Task<List<ProductDTO>> GetApprovedProductsAsync();
        Task UpdateProductStatusAsync(Guid productId, string status);
    }
}