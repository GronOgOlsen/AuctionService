using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Models
{
    public class Item
    {    
    [BsonId]
     public  Guid _id { get; set; }
     public string title { get; set; }
    public string description { get; set; }
    public string category { get; set; }
    public  float startingPrice { get; set; }
    public  string status { get; set; }
    }
}