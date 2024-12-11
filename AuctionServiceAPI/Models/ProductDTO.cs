using System;

namespace AuctionServiceAPI.Models
{
    public class ProductDTO
    {
        public Guid ProductId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } // Pending, Approved, InAuction, Sold
    }
}