using MongoDB.Bson.Serialization.Attributes;
using AuctionServiceAPI.Models;

namespace AuctionServiceAPI.Models
{
    public class Bid
    {
        [BsonId]
        public Guid _id { get; set; }

        public User? user { get; set; }  // Markeres som nullable
        public float bidPrice { get; set; }
        public Guid auctionId { get; set; }
        public DateTime? dateTime { get; set; } = DateTime.Now;

        public Bid(User? user = null)
        {
            this.user = user;
        }
    }
}
