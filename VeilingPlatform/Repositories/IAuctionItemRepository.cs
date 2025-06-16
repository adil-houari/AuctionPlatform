using VeilingPlatform.DTO.Bidding;
using VeilingPlatform.Entities;

public interface IAuctionItemRepository
{
    // Items verkopen
    Task<AuctionItem> AddAsync(AuctionItem auctionItem);
    Task<bool> CancelAsync(int itemId, string userId);
    Task<IEnumerable<AuctionItem>> GetSoldItemsByUserIdAsync(string userId);
    Task<IEnumerable<Bid>> GetBidsByItemIdAsync(int itemId);
    Task<AuctionItem> GetByIdAsync(int itemId);


    // Bieden op items
    Task<IEnumerable<AuctionItem>> SearchItemsAsync(SearchAuctionItemsDto searchParams);
    Task<Bid> PlaceBidAsync(int itemId, string userId, decimal amount);
    Task<AuctionItem> GetHighestBidAuctionItemAsync(int itemId);
    Task<bool> MakePaymentAsync(int itemId, string userId);


}
