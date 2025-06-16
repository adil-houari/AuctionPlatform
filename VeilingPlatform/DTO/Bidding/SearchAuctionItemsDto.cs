namespace VeilingPlatform.DTO.Bidding
{
    public class SearchAuctionItemsDto
    {
        public List<int> CategoryIds { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}