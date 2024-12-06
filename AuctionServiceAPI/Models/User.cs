using MongoDB.Bson.Serialization.Attributes;
using AuctionServiceAPI.Models;

namespace AuctionServiceAPI.Models
{
    public class User
    {
        [BsonId]
        public Guid _id { get; set; }

        // Gør username nullable eller sørg for at det initialiseres
        public string? username { get; set; }  // Markere som nullable (string? betyder, at det kan være null)

        public User(string? username = null) // Hvis du vil sikre, at den kan initialiseres
        {
            this.username = username;
        }
    }
}
