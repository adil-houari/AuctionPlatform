using System.Text.Json.Serialization;

namespace VeilingPlatform.Entities
{
    public class Bid
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime BidDateTime { get; set; }
        public string BidderId { get; set; }
        public int AuctionItemId { get; set; }

        [JsonIgnore]
        public AuctionItem AuctionItem { get; set; }
    }

}
