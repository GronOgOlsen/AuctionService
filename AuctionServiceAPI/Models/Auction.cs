using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Models
{
    public class Auction
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid AuctionId { get; set; } 

        [BsonRepresentation(BsonType.String)]
        public Guid ProductId { get; set; } 

        public ProductDTO Product { get; set; } // Det fulde produktobjekt, hentes fra CatalogService

        public decimal StartingPrice { get; set; } 

        public List<Bid> Bids { get; set; } 

        public DateTime StartTime { get; set; } 

        public DateTime EndTime { get; set; } 

        public User Seller { get; set; }

        public string Status { get; set; } 
    }
}
