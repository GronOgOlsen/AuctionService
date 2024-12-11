using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Models
{
    public class ProductDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid ProductId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ProductCategory ProductCategory { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal StartingPrice { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ProductStatus Status { get; set; } = ProductStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? SellerId { get; set; }
        public string? CurrentAuctionId { get; set; }
    }

    public enum ProductCategory
    {
        None = 0,
        Art = 1,
        Electronics = 2,
        Fashion = 3,
        Collectibles = 4,
        Jewelry = 5,
        Vehicles = 6,
        Other = 7
    }

    public enum ProductStatus
    {
        Pending = 0,        // Produkt er under godkendelse
        Available = 1,      // Produkt er tilgængeligt for auktion
        InAuction = 2,      // Produkt er aktivt i en auktion
        Sold = 3,           // Produkt er solgt gennem auktion
    }
}