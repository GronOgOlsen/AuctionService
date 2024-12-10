using AuctionServiceAPI.Models;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

public class Auction
{
    [BsonId]
    public Guid _id { get; set; }
    public Item item { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public List<Bid> bids { get; set; }
    public User seller { get; set; }
    public Guid auctionId { get; set; } = Guid.NewGuid();
}