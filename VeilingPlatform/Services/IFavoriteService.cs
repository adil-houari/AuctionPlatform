using VeilingPlatform.Entities;

public interface IFavoriteService
{
    Task<Favorite> AddToFavorite(int auctionItemId, string userId);
    Task<bool> RemoveFromFavorite(int favoriteId, string userId);
    Task<IEnumerable<Favorite>> GetUserFavorites(string userId);
}
