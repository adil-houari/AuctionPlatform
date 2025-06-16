using VeilingPlatform.Entities;

public interface IFavoriteRepository
{
    Task<Favorite> AddToFavorite(int auctionItemId, string userId);
    Task<bool> RemoveFromFavorite(int favoriteId, string userId);
    Task<IEnumerable<Favorite>> GetUserFavorites(string userId);
}
