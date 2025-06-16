using VeilingPlatform.DTO.AuctionItems;
using VeilingPlatform.DTO.Bidding;
using VeilingPlatform.Entities;

public interface IAuctionItemService
{
    Task<AuctionItem> CreateAuctionItemAsync(CreateAuctionItemDto dto, string userId);
    Task<bool> CancelAuctionItemAsync(int itemId, string userId);
    Task<AuctionItem> GetAuctionItemByIdAsync(int itemId);
    Task<IEnumerable<Bid>> GetBidsForItemAsync(int itemId);

    // 
    Task<IEnumerable<AuctionItem>> GetSoldItemsAsync(string userId);


    // Bieden op items
    Task<IEnumerable<AuctionItem>> SearchItemsAsync(SearchAuctionItemsDto searchParams);
    Task<Bid> PlaceBidAsync(int itemId, string userId, decimal amount);
    Task<bool> MakePaymentAsync(int itemId, string userId);


}
