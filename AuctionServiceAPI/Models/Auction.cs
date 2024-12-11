using AuctionServiceAPI.Models;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class Auction
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid _id { get; set; }
    public Product product { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public List<Bid> bids { get; set; }
    public User seller { get; set; }
    public Guid auctionId { get; set; } = Guid.NewGuid();
}