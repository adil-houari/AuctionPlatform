namespace VeilingPlatform.Entities
{
    public class AuctionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal StartingPrice { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public AuctionItemStatus Status { get; set; }
        public int CategoryId { get; set; }
        public string SellerId { get; set; }

        public Category Category { get; set; }
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();

    }


    public enum AuctionItemStatus
    {
        Initial,
        Cancelled,
        Paid
    }

}


