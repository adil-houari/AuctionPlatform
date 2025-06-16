namespace VeilingPlatform.Entities
{
    public class Favorite
    {
        public int Id { get; set; }
        public int AuctionItemId { get; set; }
        public string UserId { get; set; }

        public AuctionItem AuctionItem { get; set; }
    }

}
