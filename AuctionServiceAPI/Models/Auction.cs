using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AuctionServiceAPI.Models
{
    public class Auction
    {
        [BsonId]
        public Guid _id { get; set; }

        // Sørg for at initialisere disse egenskaber i konstruktøren eller markér dem som nullable
        public Item? item { get; set; }  // Markeres som nullable
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }

        public List<Bid> bids { get; set; } = new List<Bid>();  // Initialisering af liste
        public User? seller { get; set; }  // Markeres som nullable

        public Guid auctionId { get; set; } = Guid.NewGuid();

        public Auction(Item? item = null, User? seller = null)
        {
            this.item = item;
            this.seller = seller;
        }
    }
}
