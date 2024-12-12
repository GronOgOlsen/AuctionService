using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AuctionServiceAPI.Models
{
    public class Bid
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid _id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid AuctionId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}