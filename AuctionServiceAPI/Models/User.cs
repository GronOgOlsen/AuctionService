using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Models
{
    public class User
    {
        [BsonId]
        public Guid _id { get; set; }
        public string username { get; set; }
    }
}