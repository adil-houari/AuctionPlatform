namespace VeilingPlatform.DTO.AuctionItems
{
    public class CreateAuctionItemDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal StartingPrice { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int CategoryId { get; set; }
    }
}
