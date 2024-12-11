using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Models
{
    public class Auction
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid AuctionId { get; set; }
        public ProductDTO Product { get; set; }
        public decimal StartingPrice { get; set; }
        public List<Bid> Bids { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public User Seller { get; set; }
        public string Status { get; set; } // Active, Completed, Cancelled
    }
}